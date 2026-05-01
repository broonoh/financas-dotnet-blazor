# 💰 Minhas Finanças                                                                                                                                                                                                                        
                                                                                                                                                                                                                                              
  > Aplicação web de gestão financeira pessoal — controle de despesas, dívidas e parcelas em um só lugar.                                                                                                                                     
                                                                                                                                                                                                                                              
  ![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)                                                                                                                                                         
  ![Blazor](https://img.shields.io/badge/Blazor-WASM-512BD4?style=flat-square&logo=blazor)
  ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-336791?style=flat-square&logo=postgresql)                                                                                                                                          
  ![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat-square&logo=docker)
  ![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)                                                                                                                                                                
                  
  ---                                                                                                                                                                                                                                         
                  
  ## ✨ Funcionalidades                                                                                                                                                                                                                       
                  
  | Módulo | Descrição |                                                                                                                                                                                                                      
  |---|---|       
  | **Dashboard** | Visão geral com saldo, despesas do mês, dívidas ativas e próximas parcelas |
  | **Despesas Fixas** | Despesas parceladas com controle individual de cada parcela e status de pagamento |                                                                                                                                  
  | **Despesas Extras** | Lançamentos avulsos categorizados por tipo e forma de pagamento |                                                                                                                                                   
  | **Contas a Receber** | Registro de dívidas de terceiros com geração automática de parcelas |                                                                                                                                              
  | **Parcelas** | Visão consolidada mensal com filtros por status (pagas, pendentes, vencidas) |                                                                                                                                             
                                                                                                                                                                                                                                              
  ---             
                                                                                                                                                                                                                                              
  ## 🏗️  Arquitetura                                                                                                                                                                                                                           
   
  O projeto segue os princípios de **Clean Architecture**, garantindo separação de responsabilidades e alta testabilidade.                                                                                                                    
                  
  src/                                                                                                                                                                                                                                        
  ├── MinhasFinancas.Domain/          # Entidades, regras de negócio, interfaces
  ├── MinhasFinancas.Application/     # Commands, Queries (CQRS), DTOs, Validators                                                                                                                                                            
  ├── MinhasFinancas.Infrastructure/  # EF Core, repositórios, migrações, autenticação                                                                                                                                                        
  └── MinhasFinancas.API/             # Controllers, middlewares, configuração                                                                                                                                                                
                                                                                                                                                                                                                                              
  ### Fluxo de dados                                                                                                                                                                                                                          
                                                                                                                                                                                                                                              
  Controller → MediatR → Command/QueryHandler → Repository → PostgreSQL                                                                                                                                                                       
                      ↓
                FluentValidation                                                                                                                                                                                                              
                                                                                                                                                                                                                                              
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
                                                                                                                                                                                                                                              
  ### Frontend                                                                                                                                                                                                                                
  - **Blazor WebAssembly** (.NET 9)
  - **MudBlazor 8** — componentes Material Design                                                                                                                                                                                             
  - **Fluxor 6** — gerenciamento de estado (Flux/Redux)
                                                                                                                                                                                                                                              
  ### Infraestrutura
  - **Docker + Docker Compose** — containerização                                                                                                                                                                                             
  - **Nginx** — reverse proxy
  - **systemd** — gerenciamento de serviços                                                                                                                                                                                                   
  - **GitHub Actions** — CI/CD                                                                                                                                                                                                                
                                                                                                                                                                                                                                              
  ---                                                                                                                                                                                                                                         
                  
  ## 🚀 Como Rodar

  ### Pré-requisitos                                                                                                                                                                                                                          
   
  - [.NET 9 SDK](https://dotnet.microsoft.com/download)                                                                                                                                                                                       
  - [Docker](https://www.docker.com/) e Docker Compose

  ### ▶️  Com Docker (recomendado)                                                                                                                                                                                                             
   
  ```bash                                                                                                                                                                                                                                     
  cd docker       
  docker compose up -d                                                                                                                                                                                                                        
                  
  ┌────────────┬───────────────────────┐
  │  Serviço   │          URL          │
  ├────────────┼───────────────────────┤                                                                                                                                                                                                      
  │ Frontend   │ http://localhost:80   │
  ├────────────┼───────────────────────┤                                                                                                                                                                                                      
  │ API        │ http://localhost:5090 │                                                                                                                                                                                                      
  ├────────────┼───────────────────────┤                                                                                                                                                                                                      
  │ PostgreSQL │ localhost:5432        │                                                                                                                                                                                                      
  └────────────┴───────────────────────┘                                                                                                                                                                                                      
                  
  ⚙️  Localmente                                                                                                                                                                                                                               
   
  # 1. Suba apenas o banco de dados                                                                                                                                                                                                           
  docker compose -f docker/docker-compose.yml up postgres -d
                                                                                                                                                                                                                                              
  # 2. Inicie a API
  cd src/MinhasFinancas.API                                                                                                                                                                                                                   
  dotnet run      

  # 3. Inicie o frontend (outro terminal)                                                                                                                                                                                                     
  cd frontend/MinhasFinancas.Web
  dotnet run                                                                                                                                                                                                                                  
                  
  🧪 Testes

  dotnet test

  ---
  🔧 Configuração
                                                                                                                                                                                                                                              
  Crie o arquivo src/MinhasFinancas.API/appsettings.Development.json:
                                                                                                                                                                                                                                              
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
                  
  ---
  📁 Estrutura Completa

  financas/
  ├── .github/                                                                                                                                                                                                                                
  │   └── workflows/          # CI/CD pipelines
  ├── docker/                                                                                                                                                                                                                                 
  │   └── docker-compose.yml
  ├── docs/                   # Documentação adicional                                                                                                                                                                                        
  ├── scripts/                # Scripts de deploy e configuração                                                                                                                                                                              
  ├── src/                                                                                                                                                                                                                                    
  │   ├── MinhasFinancas.Domain/                                                                                                                                                                                                              
  │   ├── MinhasFinancas.Application/                                                                                                                                                                                                         
  │   ├── MinhasFinancas.Infrastructure/
  │   └── MinhasFinancas.API/
  ├── frontend/                                                                                                                                                                                                                               
  │   └── MinhasFinancas.Web/
  ├── tests/                                                                                                                                                                                                                                  
  │   ├── MinhasFinancas.Tests.Unit/
  │   └── MinhasFinancas.Tests.Integration/                                                                                                                                                                                                   
  └── MinhasFinancas.sln                                                                                                                                                                                                                      
                                                                                                                                                                                                                                              
  ---                                                                                                                                                                                                                                         
  📄 Licença                                                                                                                                                                                                                                  
                  
  Este projeto está licenciado sob a MIT License.
