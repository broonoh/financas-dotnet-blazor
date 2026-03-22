#!/bin/bash
# ============================================================
# Instalador — Minhas Finanças
# Fedora + GNOME + systemd (user service) + nginx
# ============================================================
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
INSTALL_DIR="$HOME/.local/share/minhas-financas"
SERVICE_NAME="minhas-financas-api"

GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

log() { echo -e "${BLUE}▶${NC} $1"; }
ok()  { echo -e "${GREEN}✓${NC} $1"; }

# ── 1. Pré-requisitos ────────────────────────────────────────
log "Verificando pré-requisitos..."
command -v dotnet >/dev/null || { echo "dotnet não encontrado"; exit 1; }
command -v nginx  >/dev/null || sudo dnf install -y nginx
ok "Pré-requisitos OK"

# ── 2. Build & Publish ───────────────────────────────────────
log "Publicando API..."
dotnet publish "$PROJECT_DIR/src/MinhasFinancas.API" \
    -c Release \
    -o "$INSTALL_DIR/api" \
    --self-contained false

log "Publicando frontend Web..."
dotnet publish "$PROJECT_DIR/frontend/MinhasFinancas.Web" \
    -c Release \
    -o "$INSTALL_DIR/web"

ok "Build concluído"

# ── 3. Copiar arquivos de suporte ────────────────────────────
log "Instalando scripts e ícone..."
mkdir -p "$INSTALL_DIR"
cp "$SCRIPT_DIR/minhas-financas-abrir.sh" "$INSTALL_DIR/"
cp "$SCRIPT_DIR/minhas-financas.svg"      "$INSTALL_DIR/"
chmod +x "$INSTALL_DIR/minhas-financas-abrir.sh"
ok "Scripts instalados"

# ── 4. appsettings de produção ───────────────────────────────
log "Configurando appsettings de produção..."
if [ ! -f "$INSTALL_DIR/api/appsettings.Production.json" ]; then
    cp "$PROJECT_DIR/src/MinhasFinancas.API/appsettings.Development.json" \
       "$INSTALL_DIR/api/appsettings.Production.json"
    echo ""
    echo "  ⚠  Edite $INSTALL_DIR/api/appsettings.Production.json"
    echo "     e ajuste a ConnectionString de produção se necessário."
    echo ""
fi

# ── 5. Systemd user service ──────────────────────────────────
log "Instalando serviço systemd..."
mkdir -p "$HOME/.config/systemd/user"
cp "$SCRIPT_DIR/$SERVICE_NAME.service" "$HOME/.config/systemd/user/"

systemctl --user daemon-reload
systemctl --user enable "$SERVICE_NAME"
systemctl --user start  "$SERVICE_NAME"
ok "Serviço systemd ativado (inicia automaticamente no login)"

# Habilitar lingering — serviço inicia mesmo antes do login gráfico
loginctl enable-linger "$(whoami)"
ok "Lingering habilitado (inicia no boot)"

# ── 6. nginx ─────────────────────────────────────────────────
log "Configurando nginx..."
sudo mkdir -p /var/www/minhas-financas
sudo cp -r "$INSTALL_DIR/web/wwwroot/." /var/www/minhas-financas/
sudo chown -R nginx:nginx /var/www/minhas-financas
sudo setsebool -P httpd_enable_homedirs on 2>/dev/null || true
sudo cp "$SCRIPT_DIR/nginx-minhas-financas.conf" \
        /etc/nginx/conf.d/minhas-financas.conf

# Verificar config antes de reiniciar
sudo nginx -t
sudo systemctl enable --now nginx
sudo systemctl reload nginx
ok "nginx configurado na porta 8080"

# ── 7. GNOME .desktop ────────────────────────────────────────
log "Instalando ícone no GNOME..."
mkdir -p "$HOME/.local/share/applications"
cp "$SCRIPT_DIR/minhas-financas.desktop" "$HOME/.local/share/applications/"
update-desktop-database "$HOME/.local/share/applications/" 2>/dev/null || true
ok "Ícone instalado em Atividades do GNOME"

# ── Resumo ───────────────────────────────────────────────────
echo ""
echo -e "${GREEN}════════════════════════════════════════${NC}"
echo -e "${GREEN}  Minhas Finanças instalado com sucesso!${NC}"
echo -e "${GREEN}════════════════════════════════════════${NC}"
echo ""
echo "  API:      http://localhost:5090"
echo "  App:      http://localhost:8080"
echo ""
echo "  Clique em 'Minhas Finanças' nas Atividades do GNOME"
echo "  para abrir o app (inicia o serviço automaticamente)."
echo ""
echo "  Gerenciamento:"
echo "    systemctl --user status  $SERVICE_NAME"
echo "    systemctl --user restart $SERVICE_NAME"
echo "    journalctl --user -u     $SERVICE_NAME -f"
echo ""
