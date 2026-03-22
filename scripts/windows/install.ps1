#Requires -Version 5.1
<#
.SYNOPSIS
    Instalador — Minhas Finanças
    Windows 11 + Task Scheduler + nginx + Atalho no Menu Iniciar
.DESCRIPTION
    Publica a API e o frontend, configura nginx como servidor web,
    registra tarefas no Task Scheduler para iniciar automaticamente
    no logon e cria atalho no Menu Iniciar.
.NOTES
    Execute como Administrador para que o nginx inicie antes do logon.
    Execute como usuário normal para iniciar apenas após o logon.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Cores ────────────────────────────────────────────────────────────────────
function Write-Step  { param($msg) Write-Host "▶ $msg" -ForegroundColor Cyan }
function Write-OK    { param($msg) Write-Host "✓ $msg" -ForegroundColor Green }
function Write-Warn  { param($msg) Write-Host "⚠ $msg" -ForegroundColor Yellow }
function Write-Fail  { param($msg) Write-Host "✗ $msg" -ForegroundColor Red }

# ── Caminhos ─────────────────────────────────────────────────────────────────
$ScriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir  = Split-Path -Parent $ScriptDir
$InstallDir  = "$env:LOCALAPPDATA\MinhasFinancas"
$ApiDir      = "$InstallDir\api"
$WebDir      = "$InstallDir\web"
$NginxDir    = "$InstallDir\nginx"
$TaskNameApi = "MinhasFinancas-API"
$TaskNameNgx = "MinhasFinancas-Nginx"
$Port        = 8080
$ApiPort     = 5090

Write-Host ""
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Blue
Write-Host "  Minhas Finanças — Instalador Windows 11           " -ForegroundColor Blue
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Blue
Write-Host ""

# ── 1. Pré-requisitos ─────────────────────────────────────────────────────────
Write-Step "Verificando pré-requisitos..."

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Fail "dotnet não encontrado. Instale o .NET 9 SDK:"
    Write-Host "  https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Yellow
    exit 1
}

$dotnetVersion = (dotnet --version)
Write-OK "dotnet $dotnetVersion encontrado"

# Verificar nginx
$nginxExe = "$NginxDir\nginx.exe"
if (-not (Test-Path $nginxExe)) {
    Write-Warn "nginx não encontrado em $NginxDir"
    Write-Host ""
    Write-Host "  Faça o download do nginx para Windows:" -ForegroundColor Yellow
    Write-Host "  1. Acesse: https://nginx.org/en/download.html" -ForegroundColor Yellow
    Write-Host "  2. Baixe a versão Stable (nginx/Windows)" -ForegroundColor Yellow
    Write-Host "  3. Extraia o conteúdo para: $NginxDir" -ForegroundColor Yellow
    Write-Host "     (a pasta deve conter nginx.exe)" -ForegroundColor Yellow
    Write-Host ""
    $resposta = Read-Host "  O nginx já está extraído em $NginxDir? (s/N)"
    if ($resposta -ne 's' -and $resposta -ne 'S') {
        Write-Fail "Instale o nginx e execute novamente."
        exit 1
    }
}

if (-not (Test-Path $nginxExe)) {
    Write-Fail "nginx.exe não encontrado em $NginxDir"
    exit 1
}
Write-OK "nginx encontrado"

# ── 2. Parar serviços em execução ─────────────────────────────────────────────
Write-Step "Parando serviços anteriores (se existirem)..."

# Parar Task Scheduler tasks
foreach ($task in @($TaskNameApi, $TaskNameNgx)) {
    $t = Get-ScheduledTask -TaskName $task -ErrorAction SilentlyContinue
    if ($t -and $t.State -eq 'Running') {
        Stop-ScheduledTask -TaskName $task
        Write-OK "Tarefa '$task' parada"
    }
}

# Parar processos nginx
Get-Process -Name nginx -ErrorAction SilentlyContinue | Stop-Process -Force

Write-OK "Serviços anteriores encerrados"

# ── 3. Publicar API ───────────────────────────────────────────────────────────
Write-Step "Publicando API..."
New-Item -ItemType Directory -Force -Path $ApiDir | Out-Null

dotnet publish "$ProjectDir\src\MinhasFinancas.API" `
    -c Release `
    -o $ApiDir `
    --self-contained false `
    --nologo `
    -v quiet

if ($LASTEXITCODE -ne 0) {
    Write-Fail "Falha ao publicar a API"
    exit 1
}
Write-OK "API publicada em $ApiDir"

# ── 4. Publicar Frontend ──────────────────────────────────────────────────────
Write-Step "Publicando frontend Blazor WASM..."
New-Item -ItemType Directory -Force -Path $WebDir | Out-Null

dotnet publish "$ProjectDir\frontend\MinhasFinancas.Web" `
    -c Release `
    -o $WebDir `
    --nologo `
    -v quiet

if ($LASTEXITCODE -ne 0) {
    Write-Fail "Falha ao publicar o frontend"
    exit 1
}
Write-OK "Frontend publicado em $WebDir"

# ── 5. appsettings de produção ────────────────────────────────────────────────
Write-Step "Configurando appsettings..."
$prodSettings = "$ApiDir\appsettings.Production.json"

if (-not (Test-Path $prodSettings)) {
    Copy-Item "$ProjectDir\src\MinhasFinancas.API\appsettings.Development.json" $prodSettings
    Write-Warn "Edite o arquivo $prodSettings"
    Write-Warn "Ajuste a ConnectionString de produção se necessário."
}
Write-OK "appsettings configurado"

# ── 6. Configurar nginx ───────────────────────────────────────────────────────
Write-Step "Configurando nginx..."
$nginxConfDir = "$NginxDir\conf"
New-Item -ItemType Directory -Force -Path $nginxConfDir | Out-Null

# Caminho do wwwroot (usar forward slashes para nginx)
$wwwrootPath = "$WebDir\wwwroot" -replace '\\', '/'

$nginxConf = @"
worker_processes  1;

events {
    worker_connections  1024;
}

http {
    include       mime.types;
    default_type  application/octet-stream;
    sendfile      on;
    keepalive_timeout  65;

    types {
        application/wasm  wasm;
    }

    server {
        listen       $Port;
        server_name  localhost;

        root   $wwwrootPath;
        index  index.html;

        # Proxy para a API .NET
        location /api/ {
            proxy_pass         http://localhost:$ApiPort;
            proxy_http_version 1.1;
            proxy_set_header   Host              `$host;
            proxy_set_header   X-Real-IP         `$remote_addr;
            proxy_set_header   X-Forwarded-For   `$proxy_add_x_forwarded_for;
            proxy_set_header   X-Forwarded-Proto `$scheme;
        }

        # SPA fallback — Blazor Router
        location / {
            try_files `$uri `$uri/ /index.html;
        }
    }
}
"@

Set-Content -Path "$nginxConfDir\nginx.conf" -Value $nginxConf -Encoding UTF8
Write-OK "nginx configurado na porta $Port"

# ── 7. Copiar script de abertura ──────────────────────────────────────────────
Write-Step "Instalando scripts de suporte..."
Copy-Item "$ScriptDir\abrir.ps1"            "$InstallDir\abrir.ps1" -Force
Copy-Item "$ScriptDir\..\minhas-financas.svg" "$InstallDir\minhas-financas.svg" -Force -ErrorAction SilentlyContinue

# Criar wrapper .bat para o atalho (permite duplo clique sem abrir PowerShell visível)
$batContent = @"
@echo off
powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File "%~dp0abrir.ps1"
"@
Set-Content -Path "$InstallDir\abrir.bat" -Value $batContent -Encoding ASCII

Write-OK "Scripts instalados"

# ── 8. Task Scheduler — API ───────────────────────────────────────────────────
Write-Step "Registrando tarefa de inicialização da API..."

$apiAction = New-ScheduledTaskAction `
    -Execute "dotnet.exe" `
    -Argument "$ApiDir\MinhasFinancas.API.dll" `
    -WorkingDirectory $ApiDir

$apiEnv = @(
    "ASPNETCORE_ENVIRONMENT=Production",
    "ASPNETCORE_URLS=http://localhost:$ApiPort"
)

# Trigger: ao fazer logon
$apiTrigger = New-ScheduledTaskTrigger -AtLogOn

$apiSettings = New-ScheduledTaskSettingsSet `
    -ExecutionTimeLimit (New-TimeSpan -Hours 0) `
    -RestartCount 3 `
    -RestartInterval (New-TimeSpan -Minutes 1) `
    -StartWhenAvailable

# Remover tarefa anterior se existir
Unregister-ScheduledTask -TaskName $TaskNameApi -Confirm:$false -ErrorAction SilentlyContinue

Register-ScheduledTask `
    -TaskName $TaskNameApi `
    -Action $apiAction `
    -Trigger $apiTrigger `
    -Settings $apiSettings `
    -RunLevel Limited `
    -Description "Minhas Finanças API — Inicia automaticamente no logon" `
    | Out-Null

# Aplicar variáveis de ambiente via XML (Register-ScheduledTask não suporta diretamente)
$task = Get-ScheduledTask -TaskName $TaskNameApi
$taskXml = [xml]($task | Export-ScheduledTask)
$ns = "http://schemas.microsoft.com/windows/2004/02/mit/task"

$actionsNode = $taskXml.Task.Actions.Exec
$envNode = $taskXml.CreateElement("EnvironmentVariables", $ns)
foreach ($ev in $apiEnv) {
    $parts = $ev -split '=', 2
    $varNode = $taskXml.CreateElement($parts[0], $ns)
    $varNode.InnerText = $parts[1]
    $envNode.AppendChild($varNode) | Out-Null
}
$actionsNode.AppendChild($envNode) | Out-Null

$taskXml.Save("$env:TEMP\mf-api-task.xml")
Unregister-ScheduledTask -TaskName $TaskNameApi -Confirm:$false -ErrorAction SilentlyContinue
Register-ScheduledTask -TaskName $TaskNameApi -Xml (Get-Content "$env:TEMP\mf-api-task.xml" -Raw) | Out-Null

Write-OK "Tarefa '$TaskNameApi' registrada"

# ── 9. Task Scheduler — nginx ─────────────────────────────────────────────────
Write-Step "Registrando tarefa de inicialização do nginx..."

$ngxAction = New-ScheduledTaskAction `
    -Execute $nginxExe `
    -WorkingDirectory $NginxDir

$ngxTrigger = New-ScheduledTaskTrigger -AtLogOn

$ngxSettings = New-ScheduledTaskSettingsSet `
    -ExecutionTimeLimit (New-TimeSpan -Hours 0) `
    -RestartCount 3 `
    -RestartInterval (New-TimeSpan -Minutes 1) `
    -StartWhenAvailable

Unregister-ScheduledTask -TaskName $TaskNameNgx -Confirm:$false -ErrorAction SilentlyContinue

Register-ScheduledTask `
    -TaskName $TaskNameNgx `
    -Action $ngxAction `
    -Trigger $ngxTrigger `
    -Settings $ngxSettings `
    -RunLevel Limited `
    -Description "Minhas Finanças nginx — Inicia automaticamente no logon" `
    | Out-Null

Write-OK "Tarefa '$TaskNameNgx' registrada"

# ── 10. Iniciar serviços agora ────────────────────────────────────────────────
Write-Step "Iniciando serviços..."

# API
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "http://localhost:$ApiPort"
Start-ScheduledTask -TaskName $TaskNameApi
Start-Sleep -Seconds 2

# Aguardar API responder (máx 20s)
$apiOk = $false
for ($i = 1; $i -le 20; $i++) {
    try {
        $r = Invoke-WebRequest "http://localhost:$ApiPort/api/health" -UseBasicParsing -TimeoutSec 1 -ErrorAction SilentlyContinue
        if ($r.StatusCode -eq 200) { $apiOk = $true; break }
    } catch {}
    Start-Sleep -Seconds 1
    Write-Host "  Aguardando API... ($i/20)" -ForegroundColor DarkGray
}

if ($apiOk) {
    Write-OK "API respondendo em http://localhost:$ApiPort"
} else {
    Write-Warn "API ainda não respondeu — verifique os logs em $ApiDir"
}

# nginx
Start-ScheduledTask -TaskName $TaskNameNgx
Start-Sleep -Seconds 2
Write-OK "nginx iniciado na porta $Port"

# ── 11. Atalho no Menu Iniciar ────────────────────────────────────────────────
Write-Step "Criando atalho no Menu Iniciar..."

$startMenuPath = [Environment]::GetFolderPath("StartMenu") + "\Programs"
$shortcutPath  = "$startMenuPath\Minhas Finanças.lnk"

$shell    = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath       = "$InstallDir\abrir.bat"
$shortcut.WorkingDirectory = $InstallDir
$shortcut.Description      = "Minhas Finanças — Controle financeiro pessoal"
$shortcut.WindowStyle      = 7  # Minimizado (sem abrir janela CMD)

# Ícone: usar ícone do Chrome/Edge ou ícone genérico de browser
$edgeExe = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
$chromeExe = "C:\Program Files\Google\Chrome\Application\chrome.exe"
if (Test-Path $edgeExe) {
    $shortcut.IconLocation = "$edgeExe,0"
} elseif (Test-Path $chromeExe) {
    $shortcut.IconLocation = "$chromeExe,0"
}

$shortcut.Save()

# Atalho também na Área de Trabalho
$desktopPath     = [Environment]::GetFolderPath("Desktop")
$shortcutDesktop = $shell.CreateShortcut("$desktopPath\Minhas Finanças.lnk")
$shortcutDesktop.TargetPath       = "$InstallDir\abrir.bat"
$shortcutDesktop.WorkingDirectory = $InstallDir
$shortcutDesktop.Description      = "Minhas Finanças — Controle financeiro pessoal"
$shortcutDesktop.WindowStyle      = 7
if (Test-Path $edgeExe)   { $shortcutDesktop.IconLocation = "$edgeExe,0" }
elseif (Test-Path $chromeExe) { $shortcutDesktop.IconLocation = "$chromeExe,0" }
$shortcutDesktop.Save()

Write-OK "Atalho criado no Menu Iniciar e na Área de Trabalho"

# ── Resumo ────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  Minhas Finanças instalado com sucesso!             " -ForegroundColor Green
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "  API:      http://localhost:$ApiPort" -ForegroundColor White
Write-Host "  App:      http://localhost:$Port" -ForegroundColor White
Write-Host ""
Write-Host "  Clique em 'Minhas Finanças' na Área de Trabalho" -ForegroundColor White
Write-Host "  ou no Menu Iniciar para abrir o aplicativo." -ForegroundColor White
Write-Host ""
Write-Host "  Gerenciamento:" -ForegroundColor DarkGray
Write-Host "    Iniciar API:   Start-ScheduledTask -TaskName $TaskNameApi" -ForegroundColor DarkGray
Write-Host "    Parar API:     Stop-ScheduledTask  -TaskName $TaskNameApi" -ForegroundColor DarkGray
Write-Host "    Iniciar nginx: Start-ScheduledTask -TaskName $TaskNameNgx" -ForegroundColor DarkGray
Write-Host "    Parar nginx:   Stop-ScheduledTask  -TaskName $TaskNameNgx" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  Logs da API: Get-EventLog Application -Source .NET* -Newest 50" -ForegroundColor DarkGray
Write-Host ""
