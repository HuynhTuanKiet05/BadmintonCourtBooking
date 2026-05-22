document.addEventListener('DOMContentLoaded', function () {
    const canvas = document.getElementById('revenue-bar-chart');
    if (!canvas) return;

    // Retrieve data injected from Razor View
    const chartLabels = window.revenueChartData?.labels || ["T2", "T3", "T4", "T5", "T6", "T7", "CN"];
    const chartValues = window.revenueChartData?.values || [1.8, 2.1, 2.6, 2.2, 3.4, 4.1, 3.7];

    const ctx = canvas.getContext('2d');
    
    // Create Chart.js Bar Chart
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: chartLabels,
            datasets: [{
                data: chartValues,
                backgroundColor: '#047857', // Primary Emerald
                hoverBackgroundColor: '#065f46', // Primary Hover
                borderRadius: {
                    topLeft: 6,
                    topRight: 6,
                    bottomLeft: 0,
                    bottomRight: 0
                },
                borderSkipped: false
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false // Hide default dataset legend
                },
                tooltip: {
                    backgroundColor: '#ffffff',
                    titleColor: '#0f172a',
                    bodyColor: '#475569',
                    borderColor: '#e2e8f0',
                    borderWidth: 1,
                    padding: 10,
                    cornerRadius: 8,
                    displayColors: false,
                    callbacks: {
                        label: function (context) {
                            return `Doanh thu: ${context.parsed.y} triệu VNĐ`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false,
                        drawBorder: false
                    },
                    ticks: {
                        color: '#64748b', // Slate 500
                        font: {
                            family: "'Inter', sans-serif",
                            size: 11
                        }
                    }
                },
                y: {
                    grid: {
                        color: '#f1f5f9', // Slate 100
                        drawBorder: false
                    },
                    ticks: {
                        color: '#64748b', // Slate 500
                        font: {
                            family: "'Inter', sans-serif",
                            size: 11
                        },
                        callback: function (value) {
                            return value + 'tr';
                        }
                    }
                }
            }
        }
    });
});
