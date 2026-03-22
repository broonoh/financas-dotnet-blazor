#Requires -Version 5.1
<#
.SYNOPSIS
    Remove completamente o Minhas Finanças do Windows.
#>

$TaskNameApi = "MinhasFinancas-API"
$TaskNameNgx = "MinhasFinancas-Nginx"
$InstallDir  = "$env:LOCALAPPDATA\MinhasFinancas"

Write-Host "Removendo Minhas Finanças..." -ForegroundColor Cyan

# Parar e remover tarefas
foreach ($task in @($TaskNameApi, $TaskNameNgx)) {
    Stop-ScheduledTask -TaskName $task -ErrorAction SilentlyContinue
    Unregister-ScheduledTask -TaskName $task -Confirm:$false -ErrorAction SilentlyContinue
    Write-Host "  Tarefa '$task' removida" -ForegroundColor Green
}

# Parar nginx
Get-Process -Name nginx -ErrorAction SilentlyContinue | Stop-Process -Force

# Remover atalhos
$startMenu = [Environment]::GetFolderPath("StartMenu") + "\Programs\Minhas Finanças.lnk"
$desktop   = [Environment]::GetFolderPath("Desktop") + "\Minhas Finanças.lnk"
Remove-Item $startMenu -ErrorAction SilentlyContinue
Remove-Item $desktop   -ErrorAction SilentlyContinue
Write-Host "  Atalhos removidos" -ForegroundColor Green

# Remover arquivos instalados (mantém o nginx que o usuário baixou)
$toRemove = @("$InstallDir\api", "$InstallDir\web", "$InstallDir\abrir.ps1", "$InstallDir\abrir.bat")
foreach ($path in $toRemove) {
    Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
}
Write-Host "  Arquivos da aplicação removidos" -ForegroundColor Green
Write-Host ""
Write-Host "Minhas Finanças foi removido." -ForegroundColor Green
Write-Host "O diretório $InstallDir foi mantido (contém o nginx e configurações)." -ForegroundColor DarkGray
