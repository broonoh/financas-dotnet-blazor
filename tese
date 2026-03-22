mkdir -p docker
nano docker/Dockerfile.api
dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj e restaurar
COPY src/MinhasFinancas.API/MinhasFinancas.API.csproj src/MinhasFinancas.API/
COPY src/MinhasFinancas.Application/MinhasFinancas.Application.csproj src/MinhasFinancas.Application/
COPY src/MinhasFinancas.Domain/MinhasFinancas.Domain.csproj src/MinhasFinancas.Domain/
COPY src/MinhasFinancas.Infrastructure/MinhasFinancas.Infrastructure.csproj src/MinhasFinancas.Infrastructure/
RUN dotnet restore src/MinhasFinancas.API/MinhasFinancas.API.csproj

# Copiar todo código
COPY . .
WORKDIR /src/src/MinhasFinancas.API
RUN dotnet publish -c Release -o /app/publish

# Build runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MinhasFinancas.API.dll"]
Dockerfile para Web
bash
nano docker/Dockerfile.web
dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/MinhasFinancas.Web/MinhasFinancas.Web.csproj src/MinhasFinancas.Web/
RUN dotnet restore src/MinhasFinancas.Web/MinhasFinancas.Web.csproj

COPY . .
WORKDIR /src/src/MinhasFinancas.Web
RUN dotnet publish -c Release -o /app/publish

FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY docker/nginx.conf /etc/nginx/nginx.conf
Docker Compose
bash
nano docker/docker-compose.yml
yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: minhasfinancas_db
    environment:
      POSTGRES_DB: minhasfinancas
      POSTGRES_USER: ${DB_USER:-financas_user}
      POSTGRES_PASSWORD: ${DB_PASSWORD:-Financas@123}
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - financas_network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U financas_user"]
      interval: 10s
      timeout: 5s
      retries: 5

  api:
    build:
      context: ..
      dockerfile: docker/Dockerfile.api
    container_name: minhasfinancas_api
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=minhasfinancas;Username=${DB_USER:-financas_user};Password=${DB_PASSWORD:-Financas@123}
      - ASPNETCORE_ENVIRONMENT=Production
      - JWT__Secret=${JWT_SECRET}
    depends_on:
      postgres:
        condition: service_healthy
    ports:
      - "5001:80"
    networks:
      - financas_network
    restart: always

  web:
    build:
      context: ..
      dockerfile: docker/Dockerfile.web
    container_name: minhasfinancas_web
    depends_on:
      - api
    ports:
      - "5002:80"
    networks:
      - financas_network
    restart: always

volumes:
  postgres_data:

networks:
  financas_network:
    driver: bridge
Nginx Configuration
bash
nano docker/nginx.conf
nginx
events {
    worker_connections 1024;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    server {
        listen 80;
        server_name localhost;
        root /usr/share/nginx/html;
        index index.html;

        # Blazor WebAssembly
        location / {
            try_files $uri $uri/ /index.html =404;
        }

        # Proxy para API
        location /api/ {
            proxy_pass http://api:80/;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection keep-alive;
            proxy_set_header Host $host;
            proxy_cache_bypass $http_upgrade;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }
}
9. CRIAR ARQUIVO .env (variáveis de ambiente)
bash
nano .env.example
env
# Banco de Dados
DB_USER=financas_user
DB_PASSWORD=Financas@123

# JWT
JWT_SECRET=sua-chave-secreta-aqui-com-pelo-menos-32-caracteres

# Email (para recuperação de senha)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=seu-email@gmail.com
SMTP_PASSWORD=sua-senha
10. COMANDOS FINAIS PARA SUBIR
bash
cd ~/Projects/MinhasFinancas

# Verificar arquivos antes do commit
git status

# Adicionar novos arquivos
git add .

# Commit final
git commit -m "chore: add Docker, GitHub Actions and documentation

- Add Dockerfiles for API and Web
- Add docker-compose configuration
- Add GitHub Actions workflows (CI and Deploy)
- Add .env.example
- Add nginx configuration
- Update README with Docker instructions
- Add license file"

# Fazer push para o GitHub
git push origin main

# Verificar se subiu
git log --oneline -5
11. VERIFICAR NO GITHUB
Acesse: https://github.com/seu-usuario/MinhasFinancas

Você verá:

✅ Todos os arquivos do projeto

✅ README.md bonito e informativo

✅ LICENSE

✅ .gitignore configurado

✅ Dockerfiles

✅ GitHub Actions

12. CONFIGURAR SECRETS NO GITHUB (para deploy)
Acesse seu repositório no GitHub

Vá em Settings → Secrets and variables → Actions

Adicione os seguintes secrets:

bash
DEPLOY_HOST        # IP ou domínio do servidor
DEPLOY_USER        # usuário SSH
DEPLOY_KEY         # chave privada SSH
JWT_SECRET         # segredo do JWT
DB_PASSWORD        # senha do banco
13. COMANDOS ÚTEIS PARA O DIA A DIA
bash
# Verificar status
git status

# Adicionar mudanças específicas
git add src/MinhasFinancas.Domain/

# Commit com mensagem
git commit -m "feat: add new feature"

# Push para GitHub
git push origin main

# Criar branch para nova feature
git checkout -b feature/nova-funcionalidade

# Merge após terminar
git checkout main
git merge feature/nova-funcionalidade
git push origin main

# Criar tag para release
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0
14. CONFIGURAR RIDER PARA USAR GIT
Abra o Rider

VCS → Enable Version Control Integration → Git

VCS → Commit (Ctrl+K) para commitar

VCS → Update Project (Ctrl+T) para puxar mudanças

VCS → Push (Ctrl+Shift+K) para enviar

✅ RESUMO - O QUE VOCÊ FEZ
✅ Configurou Git no Fedora

✅ Criou repositório no GitHub

✅ Criou .gitignore e README.md

✅ Adicionou LICENSE

✅ Criou Dockerfiles e docker-compose

✅ Configurou GitHub Actions (CI/CD)

✅ Subiu tudo para o GitHub

Agora seu projeto está no GitHub, com CI/CD configurado, pronto para colaboração e deploy! 🚀

Próximos passos:

Adicionar colaboradores ao repositório

Configurar GitHub Pages (se quiser)

Criar releases

Adicionar badges no README

Configurar CodeQL para segurança

Quer que eu continue com a implementação completa do código da API e Frontend?

