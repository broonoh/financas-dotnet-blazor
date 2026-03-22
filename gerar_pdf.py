from fpdf import FPDF

class PDF(FPDF):
    def header(self):
        self.set_fill_color(89, 74, 226)
        self.rect(0, 0, 210, 18, 'F')
        self.set_font('Helvetica', 'B', 14)
        self.set_text_color(255, 255, 255)
        self.set_y(4)
        self.cell(0, 10, 'Minhas Financas - Resumo do Projeto', align='C')
        self.set_text_color(0, 0, 0)
        self.ln(14)

    def footer(self):
        self.set_y(-12)
        self.set_font('Helvetica', 'I', 8)
        self.set_text_color(150, 150, 150)
        self.cell(0, 10, f'Pagina {self.page_no()}', align='C')

    def section_title(self, title):
        self.ln(4)
        self.set_fill_color(89, 74, 226)
        self.set_text_color(255, 255, 255)
        self.set_font('Helvetica', 'B', 11)
        self.cell(0, 8, f'  {title}', fill=True, ln=True)
        self.set_text_color(0, 0, 0)
        self.ln(2)

    def body_text(self, text):
        self.set_font('Helvetica', '', 10)
        self.set_text_color(40, 40, 40)
        self.multi_cell(0, 6, text)
        self.ln(1)

    def bullet(self, text):
        self.set_font('Helvetica', '', 10)
        self.set_text_color(40, 40, 40)
        self.set_x(14)
        self.multi_cell(0, 6, f'* {text}')

    def table_header(self, cols, widths):
        self.set_fill_color(230, 230, 245)
        self.set_font('Helvetica', 'B', 9)
        self.set_text_color(30, 30, 30)
        for col, w in zip(cols, widths):
            self.cell(w, 7, col, border=1, fill=True)
        self.ln()

    def table_row(self, cols, widths, fill=False):
        self.set_fill_color(248, 248, 252)
        self.set_font('Helvetica', '', 9)
        self.set_text_color(40, 40, 40)
        for col, w in zip(cols, widths):
            self.cell(w, 6, col, border=1, fill=fill)
        self.ln()


pdf = PDF()
pdf.set_auto_page_break(auto=True, margin=15)
pdf.add_page()

# ── O QUE É ──────────────────────────────────────────
pdf.section_title('O QUE E O PROJETO?')
pdf.body_text(
    'Sistema web de controle de financas pessoais desenvolvido do zero, com backend em .NET 9, '
    'banco de dados PostgreSQL e frontend em Blazor WebAssembly (SPA). Cada usuario gerencia '
    'suas proprias financas de forma isolada e segura, com autenticacao via JWT + Refresh Token.'
)

# ── TECNOLOGIAS ───────────────────────────────────────
pdf.section_title('TECNOLOGIAS UTILIZADAS')
tech = [
    ('Backend', '.NET 9 / ASP.NET Core / C#'),
    ('ORM / Banco', 'Entity Framework Core 9 + PostgreSQL (Npgsql)'),
    ('CQRS', 'MediatR 12'),
    ('Validacao', 'FluentValidation 11'),
    ('Seguranca', 'JWT Bearer + BCrypt'),
    ('Frontend', 'Blazor WebAssembly (SPA)'),
    ('UI Components', 'MudBlazor 8'),
    ('State Management', 'Fluxor (padrao Redux)'),
    ('Logs', 'Serilog'),
    ('Testes', 'xUnit + FluentAssertions (38 testes)'),
    ('Containers', 'Docker + Docker Compose'),
    ('CI/CD', 'GitHub Actions'),
]
pdf.table_header(['Camada', 'Tecnologia'], [50, 135])
for i, (c, t) in enumerate(tech):
    pdf.table_row([c, t], [50, 135], fill=(i % 2 == 0))
pdf.ln(2)

# ── ARQUITETURA ───────────────────────────────────────
pdf.section_title('ARQUITETURA - CLEAN ARCHITECTURE')
pdf.body_text(
    'O projeto e dividido em 4 camadas com dependencias de dentro para fora. '
    'A regra principal e que camadas internas nao conhecem camadas externas.'
)
layers = [
    ('Domain', 'Entidades, regras de negocio puras, interfaces de repositorio. Zero dependencias externas.'),
    ('Application', 'Casos de uso (Commands + Queries), Handlers MediatR, Validators FluentValidation.'),
    ('Infrastructure', 'Implementacao dos repositorios, EF Core, PostgreSQL, migrações.'),
    ('API', 'Controllers REST, autenticacao JWT, middlewares de erro, Swagger.'),
    ('Web (Frontend)', 'Blazor WASM independente, consome a API via HTTP, gerencia estado com Fluxor.'),
]
pdf.table_header(['Camada', 'Responsabilidade'], [45, 140])
for i, (c, r) in enumerate(layers):
    pdf.table_row([c, r], [45, 140], fill=(i % 2 == 0))
pdf.ln(2)

# ── FUNCIONALIDADES ───────────────────────────────────
pdf.section_title('FUNCIONALIDADES IMPLEMENTADAS')

pdf.set_font('Helvetica', 'B', 10)
pdf.cell(0, 7, 'Autenticacao e Seguranca', ln=True)
auths = [
    'Cadastro com senha hasheada via BCrypt',
    'Login com JWT Access Token (curta duracao) + Refresh Token em cookie HttpOnly',
    'Renovacao automatica de token sem novo login',
    'Logout com invalidacao do refresh token no banco',
    'Edicao de perfil (nome, e-mail, senha)',
]
for a in auths:
    pdf.bullet(a)
pdf.ln(2)

pdf.set_font('Helvetica', 'B', 10)
pdf.cell(0, 7, 'Receitas', ln=True)
for r in ['Cadastro com descricao, valor, data, categoria e status de recebimento',
          'Listagem com filtros, edicao e exclusao',
          'Categorias dinamicas gerenciadas pelo proprio usuario']:
    pdf.bullet(r)
pdf.ln(2)

pdf.set_font('Helvetica', 'B', 10)
pdf.cell(0, 7, 'Despesas', ln=True)
for d in [
    'Despesa Fixa Parcelada: valor dividido em N parcelas (2 a 48), geradas automaticamente',
    'Arredondamento inteligente: centavos sao absorvidos na ultima parcela',
    'Despesa Extra: gasto unico nao recorrente',
    'Edicao e exclusao por tipo',
    'Categorias dinamicas separadas por tipo (receita / despesa)',
]:
    pdf.bullet(d)
pdf.ln(2)

pdf.set_font('Helvetica', 'B', 10)
pdf.cell(0, 7, 'Dividas a Receber', ln=True)
for d in ['Cadastro de valores que terceiros devem ao usuario',
          'Controle por parcelas com data de vencimento',
          'Marcacao de parcelas recebidas']:
    pdf.bullet(d)
pdf.ln(2)

pdf.set_font('Helvetica', 'B', 10)
pdf.cell(0, 7, 'Dashboard e Parcelas', ln=True)
for d in ['Visao geral do periodo: total receitas vs despesas',
          'Graficos com Chart.js',
          'Tela de parcelas do mes com marcacao de pago/nao pago']:
    pdf.bullet(d)

# ── BANCO DE DADOS ────────────────────────────────────
pdf.section_title('BANCO DE DADOS - TABELAS E MIGRACOES')
tables = [
    ('usuarios', 'Cadastro e autenticacao dos usuarios'),
    ('receitas', 'Receitas financeiras por usuario'),
    ('despesas', 'Despesas fixas e extras'),
    ('parcelas', 'Parcelas geradas automaticamente para despesas fixas'),
    ('dividas', 'Dividas que terceiros devem ao usuario'),
    ('parcelas_divida', 'Parcelas das dividas'),
    ('categorias_receita', 'Categorias personalizadas de receita por usuario'),
    ('categorias_despesa', 'Categorias personalizadas de despesa por usuario'),
]
pdf.table_header(['Tabela', 'Descricao'], [55, 130])
for i, (t, d) in enumerate(tables):
    pdf.table_row([t, d], [55, 130], fill=(i % 2 == 0))
pdf.ln(2)
pdf.body_text('3 migrations EF Core aplicadas: InitialCreate, AddDividas, AdicionarCategoriasDinamicas.')

# ── PADROES ───────────────────────────────────────────
pdf.section_title('PADROES E BOAS PRATICAS')
padroes = [
    ('CQRS com MediatR', 'Cada operacao e um Command (escrita) ou Query (leitura) com Handler proprio.'),
    ('FluentValidation', 'Cada Command tem Validator dedicado; invalido retorna 400 com detalhes.'),
    ('Factory Method no Domain', 'Entidades sao criadas via metodo estatico Criar() que valida regras internas.'),
    ('Repositorio com Interface', 'Application depende de interface; Infrastructure faz a implementacao.'),
    ('Error Middleware', 'Captura excecoes e retorna JSON padronizado com HTTP code correto.'),
    ('Cookie HttpOnly', 'Refresh Token armazenado em cookie nao acessivel por JavaScript (anti-XSS).'),
    ('Indice Unico DB', 'Categorias tem indice unico (usuario_id + nome) para evitar duplicidade.'),
]
pdf.table_header(['Padrao', 'Descricao'], [55, 130])
for i, (p, d) in enumerate(padroes):
    pdf.table_row([p, d], [55, 130], fill=(i % 2 == 0))

# ── TESTES ────────────────────────────────────────────
pdf.section_title('TESTES UNITARIOS - 38 TESTES, 100% PASSANDO')
testes = [
    'Geracao correta de parcelas (quantidade, datas sequenciais)',
    'Arredondamento de centavos na ultima parcela',
    'Soma das parcelas deve igualar o valor total',
    'Excecao ao criar com quantidade de parcelas invalida (< 2 ou > 48)',
    'Excecao ao criar com valor zero ou negativo',
    'Excecao ao criar com data anterior ao mes atual',
    'Validadores FluentValidation: descricao curta, valor zero, data passada',
]
for t in testes:
    pdf.bullet(t)

# ── TELAS ─────────────────────────────────────────────
pdf.section_title('TELAS DO FRONTEND (BLAZOR WASM)')
telas = [
    ('/login', 'Autenticacao com e-mail e senha'),
    ('/cadastrar', 'Registro de novo usuario'),
    ('/dashboard', 'Visao geral e graficos financeiros'),
    ('/parcelas', 'Parcelas do mes com marcacao de pago'),
    ('/receitas', 'CRUD completo de receitas'),
    ('/despesas', 'CRUD de despesas fixas, extras e dividas'),
    ('/categorias/receita', 'Gerenciar categorias de receita'),
    ('/categorias/despesa', 'Gerenciar categorias de despesa'),
    ('/perfil', 'Editar dados do usuario'),
]
pdf.table_header(['Rota', 'Descricao'], [60, 125])
for i, (r, d) in enumerate(telas):
    pdf.table_row([r, d], [60, 125], fill=(i % 2 == 0))

# ── NUMEROS ───────────────────────────────────────────
pdf.section_title('RESUMO EM NUMEROS')
nums = [
    ('Projetos .NET na solucao', '6'),
    ('Endpoints REST (API)', '~35'),
    ('Telas no frontend', '10'),
    ('Entidades de dominio', '10'),
    ('Commands e Queries', '~30'),
    ('Testes unitarios', '38 (100% passando)'),
    ('Migrations EF Core', '3'),
    ('Tabelas no banco', '8'),
]
pdf.table_header(['Item', 'Quantidade'], [120, 65])
for i, (k, v) in enumerate(nums):
    pdf.table_row([k, v], [120, 65], fill=(i % 2 == 0))

# ── INFRAESTRUTURA ────────────────────────────────────
pdf.section_title('INFRAESTRUTURA')
pdf.bullet('Docker Compose: sobe API + Web + PostgreSQL com um unico comando')
pdf.bullet('GitHub Actions CI/CD: build automatico, testes e publicacao a cada push')
pdf.bullet('Serilog: logs estruturados com nivel configuravel por ambiente')
pdf.bullet('Swagger: documentacao interativa da API em /swagger')

out = '/home/broonoh/Workspace/financas/MinhasFinancas_Resumo_Projeto.pdf'
pdf.output(out)
print(f'PDF gerado: {out}')
