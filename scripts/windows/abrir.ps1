#Requires -Version 5.1
<#
.SYNOPSIS
    Abre o Minhas Finanças no browser.
    Inicia a API e o nginx se não estiverem rodando.
#>

$TaskNameApi = "MinhasFinancas-API"
$TaskNameNgx = "MinhasFinancas-Nginx"
$ApiPort     = 5090
$AppUrl      = "http://localhost:8080"

# ── Iniciar API se não estiver rodando ───────────────────────────────────────
$apiTask = Get-ScheduledTask -TaskName $TaskNameApi -ErrorAction SilentlyContinue
if (-not $apiTask) {
    [System.Windows.Forms.MessageBox]::Show(
        "Minhas Finanças não está instalado.`nExecute o install.ps1 primeiro.",
        "Minhas Finanças",
        [System.Windows.Forms.MessageBoxButtons]::OK,
        [System.Windows.Forms.MessageBoxIcon]::Error)
    exit 1
}

$apiRunning = $false
try {
    $r = Invoke-WebRequest "http://localhost:$ApiPort/api/health" -UseBasicParsing -TimeoutSec 1 -ErrorAction SilentlyContinue
    $apiRunning = ($r.StatusCode -eq 200)
} catch {}

if (-not $apiRunning) {
    # Mostrar notificação via balloon tip
    Add-Type -AssemblyName System.Windows.Forms
    $notify = New-Object System.Windows.Forms.NotifyIcon
    $notify.Icon    = [System.Drawing.SystemIcons]::Application
    $notify.Visible = $true
    $notify.ShowBalloonTip(3000, "Minhas Finanças", "Iniciando serviço...", [System.Windows.Forms.ToolTipIcon]::Info)

    Start-ScheduledTask -TaskName $TaskNameApi

    # Aguardar API responder (máx 20s)
    for ($i = 1; $i -le 20; $i++) {
        Start-Sleep -Seconds 1
        try {
            $r = Invoke-WebRequest "http://localhost:$ApiPort/api/health" -UseBasicParsing -TimeoutSec 1 -ErrorAction SilentlyContinue
            if ($r.StatusCode -eq 200) { break }
        } catch {}
    }

    $notify.Dispose()
}

# ── Iniciar nginx se não estiver rodando ─────────────────────────────────────
$ngxRunning = Get-Process -Name nginx -ErrorAction SilentlyContinue
if (-not $ngxRunning) {
    Start-ScheduledTask -TaskName $TaskNameNgx
    Start-Sleep -Seconds 1
}

# ── Abrir no browser padrão ──────────────────────────────────────────────────
Start-Process $AppUrl
