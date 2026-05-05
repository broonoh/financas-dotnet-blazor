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

window.renderizarGraficoPizzaFinancas = (id, labels, valores) => {
    destroyChart(id);
    const ctx = document.getElementById(id);
    if (!ctx) return;

    const cores = ['#4CAF50', '#5C35CC', '#E65100'];
    const total = valores.reduce((a, b) => a + b, 0);

    charts[id] = new Chart(ctx, {
        type: 'doughnut',
        plugins: [ChartDataLabels],
        data: {
            labels: labels,
            datasets: [{
                data: valores,
                backgroundColor: cores,
                hoverOffset: 6,
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { position: 'bottom' },
                tooltip: {
                    callbacks: {
                        label: c => {
                            const t = c.dataset.data.reduce((a, b) => a + b, 0);
                            const pct = t > 0 ? ((c.parsed / t) * 100).toFixed(1) : 0;
                            return `${c.label}: R$ ${c.parsed.toFixed(2).replace('.', ',')} (${pct}%)`;
                        }
                    }
                },
                datalabels: {
                    color: '#fff',
                    font: { weight: 'bold', size: 13 },
                    textShadowBlur: 6,
                    textShadowColor: 'rgba(0,0,0,0.55)',
                    formatter: (value) => {
                        if (total === 0 || value === 0) return '';
                        return (value / total * 100).toFixed(1) + '%';
                    }
                }
            }
        }
    });
};

window.renderizarGraficoDevedores = (id, labels, totais, saldos) => {
    destroyChart(id);
    const ctx = document.getElementById(id);
    if (!ctx) return;

    charts[id] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Total Emprestado',
                    data: totais,
                    backgroundColor: 'rgba(33, 150, 243, 0.8)',
                    borderColor: '#1565C0',
                    borderWidth: 1,
                    borderRadius: 4,
                    barPercentage: 0.75,
                    categoryPercentage: 0.9
                },
                
                
                {
                    label: 'Saldo Restante',
                    data: saldos,
                    backgroundColor: 'rgba(244, 67, 54, 0.8)',
                    borderColor: '#B71C1C',
                    borderWidth: 1,
                    borderRadius: 4,
                    barPercentage: 0.75,
                    categoryPercentage: 0.9
                }
            ]
        },
        options: {
            indexAxis: 'y',
            responsive: true,
            plugins: {
                legend: { position: 'top' },
                tooltip: {
                    callbacks: {
                        label: ctx => `${ctx.dataset.label}: R$ ${ctx.parsed.x.toFixed(2).replace('.', ',')}`
                    }
                }
            },
            scales: {
                x: {
                    ticks: {
                        callback: val => 'R$ ' + val.toFixed(2).replace('.', ',')
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
