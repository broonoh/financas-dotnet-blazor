// Instâncias dos gráficos para evitar duplicação
const charts = {};

function destroyChart(id) {
    if (charts[id]) {
        charts[id].destroy();
        delete charts[id];
    }
}

window.renderizarGraficoLinha = (id, labels, receitas, despesas, saldo) => {
    destroyChart(id);
    const ctx = document.getElementById(id);
    if (!ctx) return;

    charts[id] = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Receitas',
                    data: receitas,
                    borderColor: '#4CAF50',
                    backgroundColor: 'rgba(76, 175, 80, 0.1)',
                    tension: 0.3,
                    fill: true
                },
                {
                    label: 'Despesas',
                    data: despesas,
                    borderColor: '#F44336',
                    backgroundColor: 'rgba(244, 67, 54, 0.1)',
                    tension: 0.3,
                    fill: true
                },
                {
                    label: 'Saldo',
                    data: saldo,
                    borderColor: '#2196F3',
                    backgroundColor: 'rgba(33, 150, 243, 0.1)',
                    tension: 0.3,
                    fill: true
                }
            ]
        },
        options: {
            responsive: true,
            plugins: { legend: { position: 'top' } },
            scales: {
                y: {
                    ticks: {
                        callback: val => 'R$ ' + val.toFixed(2).replace('.', ',')
                    }
                }
            }
        }
    });
};

window.renderizarGraficoPizza = (id, labels, valores) => {
    destroyChart(id);
    const ctx = document.getElementById(id);
    if (!ctx) return;

    const cores = ['#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF', '#FF9F40', '#FF6384'];

    charts[id] = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: valores,
                backgroundColor: cores.slice(0, labels.length),
                hoverOffset: 4
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { position: 'bottom' },
                tooltip: {
                    callbacks: {
                        label: ctx => `${ctx.label}: R$ ${ctx.parsed.toFixed(2).replace('.', ',')}`
                    }
                }
            }
        }
    });
};

window.renderizarGraficoBarras = (id, labels, receitas, despesas) => {
    destroyChart(id);
    const ctx = document.getElementById(id);
    if (!ctx) return;

    charts[id] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Receitas',
                    data: receitas,
                    backgroundColor: 'rgba(76, 175, 80, 0.8)',
                    borderRadius: 4
                },
                {
                    label: 'Despesas',
                    data: despesas,
                    backgroundColor: 'rgba(244, 67, 54, 0.8)',
                    borderRadius: 4
                }
            ]
        },
        options: {
            responsive: true,
            plugins: { legend: { position: 'top' } },
            scales: {
                y: {
                    ticks: {
                        callback: val => 'R$ ' + val.toFixed(2).replace('.', ',')
                    }
                }
            }
        }
    });
};
