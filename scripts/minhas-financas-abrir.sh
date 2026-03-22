#!/bin/bash
# Inicia o serviço se não estiver rodando e abre o browser

SERVICE="minhas-financas-api"
URL="http://localhost:5171"

if ! systemctl --user is-active --quiet "$SERVICE"; then
    notify-send "Minhas Finanças" "Iniciando serviço..." --icon=utilities-finance-manager -t 3000
    systemctl --user start "$SERVICE"

    # Aguarda a API responder (máx 15s)
    for i in $(seq 1 15); do
        if curl -sf http://localhost:5090/api/health > /dev/null 2>&1; then
            break
        fi
        sleep 1
    done
fi

xdg-open "$URL"
