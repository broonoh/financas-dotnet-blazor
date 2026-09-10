# 💰 Minhas Finanças

> Aplicação de gestão financeira pessoal — controle de receitas, despesas, dívidas e parcelas, disponível como app web e como app nativo Android.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-WASM-512BD4?style=flat-square&logo=blazor)
![MAUI](https://img.shields.io/badge/.NET%20MAUI-Android-512BD4?style=flat-square&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=flat-square&logo=postgresql)
![SQLite](https://img.shields.io/badge/SQLite-local-003B57?style=flat-square&logo=sqlite)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat-square&logo=docker)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)

---

## 📱 Duas versões, um mesmo domínio

| Versão | Onde roda | Banco | Login |
|---|---|---|---|
| **Web** (Blazor WebAssembly) | Navegador, via Docker/Nginx | PostgreSQL (servidor) | E-mail + senha (JWT) |
| **MAUI** (.NET MAUI/Android) | Tablet/celular, offline | SQLite local, um banco por aparelho | Sem login — usuário único do aparelho |

As duas versões reaproveitam exatamente as mesmas camadas `Domain`, `Application` e `Infrastructure` — o app MAUI usa o `MediatR` diretamente, sem depender da API/HTTP, e persiste tudo localmente no aparelho, sem exigir conexão com o servidor.

---

## ✨ Funcionalidades

| Módulo | Descrição |
|---|---|
| **Dashboard** | Visão geral com saldo, evolução dos últimos 12 meses, comparativo mensal e distribuição por categoria |
| **Despesas Fixas** | Despesas parceladas com controle individual de cada parcela e status de pagamento |
| **Despesas Extras** | Lançamentos avulsos categorizados por tipo, forma de pagamento e credor (opcional) |
| **Contas a Receber** | Dívidas de terceiros vinculadas a um **Devedor** cadastrado, com geração automática de parcelas |
| **Devedores / Credores** | Cadastro de quem deve para você (Devedor, usado em Contas a Receber) e para quem você deve (Credor, opcional em Despesas Fixas/Extras) |
| **Forma de Pagamento** | Cadastro editável e compartilhado entre Despesas Fixas e Extras — substitui listas fixas por opções que você mesmo define |
| **Categorias** | Cadastro de categorias próprias para Receitas e Despesas |
| **Confirmação de Pagamento em Massa** | Um botão marca como pagas todas as parcelas pendentes do período selecionado, nas três abas (Fixas, Extras e Contas a Receber) |
| **Parcelas** | Visão consolidada mensal com filtros por status (pagas, pendentes, vencidas) |
| **Resumo Mensal** | Fechamento do mês por seção, com totais e exportação em PDF |
| **Exportação em PDF** | Relatórios de Despesas Fixas, Extras e Contas a Receber (por devedor), com totais e credor quando aplicável |
| **Perfil** | Nome, e-mail, telefone e data de nascimento editáveis |

---

## 🏗️  Arquitetura

O projeto segue os princípios de **Clean Architecture**, garantindo separação de responsabilidades e alta testabilidade — e permitindo que o app MAUI reaproveite o mesmo núcleo de domínio do backend web.

```
src/
├── MinhasFinancas.Domain/          # Entidades, regras de negócio, interfaces
├── MinhasFinancas.Application/     # Commands, Queries (CQRS), DTOs, Validators
├── MinhasFinancas.Infrastructure/  # EF Core, repositórios, migrações, autenticação
│                                     (dois providers: Npgsql para a API, Sqlite para o MAUI)
├── MinhasFinancas.API/             # Controllers, middlewares, configuração (versão web)
└── MinhasFinancas.Maui/            # App nativo Android (.NET MAUI + MVVM), 100% offline
```

### Fluxo de dados — Web

```
Controller → MediatR → Command/QueryHandler → Repository → PostgreSQL
                            ↓
                     FluentValidation
```

### Fluxo de dados — MAUI (Android)

```
Page (XAML) → ViewModel (MVVM) → MediatR → Command/QueryHandler → Repository → SQLite (local)
```

---

## 🛠️  Tecnologias

### Backend
- **ASP.NET Core 9** — Web API RESTful
- **Entity Framework Core 9** — ORM com migrações automáticas
- **PostgreSQL** via Npgsql
- **MediatR 12** — padrão CQRS
- **FluentValidation 11** — validação de comandos
- **BCrypt.Net** — hash de senhas
- **JWT** — autenticação stateless
- **QuestPDF** — geração de relatórios em PDF

### Frontend Web
- **Blazor WebAssembly** (.NET 9)
- **MudBlazor 8** — componentes Material Design
- **Fluxor 6** — gerenciamento de estado (Flux/Redux)

### App Android (.NET MAUI)
- **.NET MAUI** (`net9.0-android`) + **CommunityToolkit.Mvvm** — MVVM com `ObservableObject`/`RelayCommand`
- **Entity Framework Core + Sqlite** — banco local, um arquivo por aparelho
- **MediatR** — reaproveita os mesmos Commands/Queries da API, sem camada HTTP
- **SkiaSharp** — geração de PDF nativa no aparelho
- **CommunityToolkit.Maui**

### Infraestrutura
- **Docker + Docker Compose** — containerização (API + Web + PostgreSQL)
- **Nginx** — reverse proxy
- **systemd** — gerenciamento de serviços
- **GitHub Actions** — CI/CD

---

## 🚀 Como Rodar — Versão Web

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) e Docker Compose

### ▶️  Com Docker (recomendado)

```bash
cd docker
docker compose up -d
```

| Serviço | URL |
|---|---|
| Frontend | http://localhost:80 |
| API | http://localhost:5090 |
| PostgreSQL | localhost:5432 |

### ⚙️  Localmente

```bash
# 1. Suba apenas o banco de dados
docker compose -f docker/docker-compose.yml up postgres -d

# 2. Inicie a API
cd src/MinhasFinancas.API
dotnet run

# 3. Inicie o frontend (outro terminal)
cd frontend/MinhasFinancas.Web
dotnet run
```

### 🧪 Testes

```bash
dotnet test
```

---

## 📱 Como Rodar — App Android (MAUI)

O app MAUI é 100% offline: não precisa da API nem do PostgreSQL rodando, pois usa um banco SQLite local criado automaticamente no primeiro uso do aparelho.

### Pré-requisitos

- .NET 9 SDK com o workload MAUI (`dotnet workload install maui`)
- Android SDK + JDK (variáveis configuradas em `src/MinhasFinancas.Maui/env.sh`)
- Um keystore de assinatura para gerar o release (não incluído no repositório — veja `release.sh`)

### Scripts disponíveis (em `src/MinhasFinancas.Maui/`)

```bash
cd src/MinhasFinancas.Maui

source env.sh   # configura DOTNET_ROOT, JAVA_HOME, ANDROID_SDK_ROOT
./build.sh      # compila o projeto (Debug)
./run.sh        # instala e roda no aparelho/emulador conectado
./release.sh    # gera o APK de release assinado, pronto para instalar fora da Play Store
```

O caminho do APK gerado é exibido ao final da execução de `release.sh`.

---

## 🔧 Configuração

Crie o arquivo `src/MinhasFinancas.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=minhasfinancas;Username=postgres;Password=sua_senha"
  },
  "Jwt": {
    "SecretKey": "sua_chave_secreta_minima_32_caracteres",
    "Issuer": "MinhasFinancas",
    "Audience": "MinhasFinancas",
    "ExpirationHours": 8
  }
}
```

---

## 📁 Estrutura Completa

```
financas/
├── .github/
│   └── workflows/                  # CI/CD pipelines
├── docker/
│   └── docker-compose.yml
├── docs/                           # Documentação adicional
├── scripts/                        # Scripts de deploy e configuração
├── src/
│   ├── MinhasFinancas.Domain/
│   ├── MinhasFinancas.Application/
│   ├── MinhasFinancas.Infrastructure/
│   ├── MinhasFinancas.API/
│   └── MinhasFinancas.Maui/        # App Android (.NET MAUI)
├── frontend/
│   └── MinhasFinancas.Web/
├── tests/
│   ├── MinhasFinancas.Tests.Unit/
│   └── MinhasFinancas.Tests.Integration/
└── MinhasFinancas.sln
```

---

## 📄 Licença

Este projeto está licenciado sob a MIT License.
