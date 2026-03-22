# Minhas Finanças — Instalação no Windows 11

## Pré-requisitos

| Ferramenta | Versão | Download |
|---|---|---|
| .NET SDK | 9.0 | https://dotnet.microsoft.com/download/dotnet/9.0 |
| nginx para Windows | Stable | https://nginx.org/en/download.html |
| PostgreSQL | 16 | https://www.postgresql.org/download/windows/ |

## Preparação do nginx

1. Baixe o **nginx/Windows** (arquivo `.zip`) em https://nginx.org/en/download.html
2. Extraia o conteúdo para:
   ```
   %LOCALAPPDATA%\MinhasFinancas\nginx\
   ```
   A pasta deve conter `nginx.exe` diretamente nela.

## Instalação

Abra o **PowerShell** e execute:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
cd caminho\para\financas\scripts\windows
.\install.ps1
```

O script fará automaticamente:
- Publicar a API em `%LOCALAPPDATA%\MinhasFinancas\api\`
- Publicar o frontend em `%LOCALAPPDATA%\MinhasFinancas\web\`
- Configurar o nginx na porta **8080** (proxying `/api/` → porta 5090)
- Registrar tarefas no **Task Scheduler** para iniciar na abertura do Windows
- Criar atalho na **Área de Trabalho** e no **Menu Iniciar**

## Uso

Após instalar, basta clicar em **"Minhas Finanças"** na Área de Trabalho ou no Menu Iniciar. O script `abrir.bat` verifica se os serviços estão rodando, inicia se necessário e abre o browser em `http://localhost:8080`.

## Gerenciamento manual

```powershell
# Iniciar serviços
Start-ScheduledTask -TaskName "MinhasFinancas-API"
Start-ScheduledTask -TaskName "MinhasFinancas-Nginx"

# Parar serviços
Stop-ScheduledTask -TaskName "MinhasFinancas-API"
Stop-ScheduledTask -TaskName "MinhasFinancas-Nginx"
Get-Process nginx | Stop-Process -Force

# Ver status
Get-ScheduledTask -TaskName "MinhasFinancas-API"
Get-ScheduledTask -TaskName "MinhasFinancas-Nginx"

# Ver logs da API (Event Viewer)
Get-EventLog Application -Source ".NET*" -Newest 50
```

## Desinstalar

```powershell
.\desinstalar.ps1
```

## Portas utilizadas

| Serviço | Porta |
|---|---|
| API (.NET) | 5090 |
| Frontend (nginx) | 8080 |
| PostgreSQL | 5432 |

## Estrutura de arquivos após instalação

```
%LOCALAPPDATA%\MinhasFinancas\
├── api\                  ← API publicada
│   ├── MinhasFinancas.API.dll
│   ├── appsettings.json
│   └── appsettings.Production.json  ← edite a ConnectionString aqui
├── web\
│   └── wwwroot\          ← Frontend Blazor WASM
├── nginx\
│   ├── nginx.exe
│   └── conf\
│       └── nginx.conf    ← gerado pelo install.ps1
├── abrir.ps1
└── abrir.bat             ← chamado pelo atalho
```
