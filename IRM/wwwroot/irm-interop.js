/* ============================================ */
/*  IRM v2.0 — JS Interop                       */
/* ============================================ */

window.irmInterop = {
    // ── Dark Mode ──
    setTheme: function (theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('irm-theme', theme);
    },
    getTheme: function () {
        return localStorage.getItem('irm-theme') || 'light';
    },
    initTheme: function () {
        var saved = localStorage.getItem('irm-theme') || 'light';
        document.documentElement.setAttribute('data-theme', saved);
        return saved;
    },

    // ── Loading Screen ──
    hideLoadingScreen: function () {
        var el = document.getElementById('irmLoadingScreen');
        if (el) {
            el.classList.add('fade-out');
            setTimeout(function () { el.remove(); }, 500);
        }
    },

    // ── KPI Number Animation ──
    animateNumber: function (elementId, target, duration) {
        var el = document.getElementById(elementId);
        if (!el) return;
        var startTime = performance.now();
        function update(currentTime) {
            var elapsed = currentTime - startTime;
            var progress = Math.min(elapsed / duration, 1);
            var eased = 1 - Math.pow(1 - progress, 3);
            el.textContent = Math.floor(eased * target).toLocaleString('vi-VN');
            if (progress < 1) requestAnimationFrame(update);
        }
        requestAnimationFrame(update);
    },

    // ── File Download ──
    downloadFile: function (fileName, contentType, base64) {
        var link = document.createElement('a');
        link.download = fileName;
        link.href = 'data:' + contentType + ';base64,' + base64;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    },

    // ── Client Error Log ──
    logError: function (message, detail) {
        console.error('[IRM]', message, detail || '');
    },

    // ════════════════════════════════════════════
    //  Chart.js Integration (matching mockup-demo)
    // ════════════════════════════════════════════
    _charts: {},

    _getChartColors: function () {
        var isDark = document.documentElement.getAttribute('data-theme') === 'dark';
        return {
            textColor: isDark ? '#9aa0a6' : '#64748b',
            gridColor: isDark ? '#2d3139' : '#f1f5f9',
            borderColor: isDark ? '#2d3139' : '#e2e8f0',
            surfaceColor: isDark ? '#1c1f26' : '#ffffff'
        };
    },

    destroyAllCharts: function () {
        var self = window.irmInterop;
        Object.keys(self._charts).forEach(function (key) {
            if (self._charts[key]) {
                self._charts[key].destroy();
                delete self._charts[key];
            }
        });
    },

    // ── Bar Chart: Nationality ──
    createBarChart: function (canvasId, labels, data) {
        var self = window.irmInterop;
        if (self._charts[canvasId]) {
            self._charts[canvasId].destroy();
        }
        var ctx = document.getElementById(canvasId);
        if (!ctx) return;
        var colors = self._getChartColors();

        Chart.defaults.color = colors.textColor;
        Chart.defaults.borderColor = colors.borderColor;
        Chart.defaults.font.family = "'Inter', sans-serif";

        self._charts[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Số lao động',
                    data: data,
                    backgroundColor: [
                        '#2563eb', '#7c3aed', '#ec4899', '#059669', '#d97706',
                        '#8b5cf6', '#14b8a6', '#f97316', '#6366f1', '#94a3b8'
                    ],
                    borderRadius: 6,
                    borderSkipped: false
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: { beginAtZero: true, grid: { color: colors.gridColor } },
                    x: { grid: { display: false } }
                },
                animation: { duration: 1500, easing: 'easeOutQuart' }
            }
        });
    },

    // ── Doughnut Chart: Work Permit ──
    createDoughnutChart: function (canvasId, labels, data) {
        var self = window.irmInterop;
        if (self._charts[canvasId]) {
            self._charts[canvasId].destroy();
        }
        var ctx = document.getElementById(canvasId);
        if (!ctx) return;
        var colors = self._getChartColors();

        Chart.defaults.color = colors.textColor;
        Chart.defaults.font.family = "'Inter', sans-serif";

        self._charts[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: ['#059669', '#2563eb', '#d97706', '#94a3b8'],
                    borderWidth: 2,
                    borderColor: colors.surfaceColor,
                    spacing: 3
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '62%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 18,
                            usePointStyle: true,
                            pointStyleWidth: 10,
                            font: { size: 12 }
                        }
                    }
                },
                animation: { animateRotate: true, duration: 1500 }
            }
        });
    },

    // ── Line Chart: Trend ──
    createLineChart: function (canvasId, labels, dataset1, dataset2) {
        var self = window.irmInterop;
        if (self._charts[canvasId]) {
            self._charts[canvasId].destroy();
        }
        var ctx = document.getElementById(canvasId);
        if (!ctx) return;
        var colors = self._getChartColors();

        Chart.defaults.color = colors.textColor;
        Chart.defaults.borderColor = colors.borderColor;
        Chart.defaults.font.family = "'Inter', sans-serif";

        self._charts[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: dataset1.name,
                        data: dataset1.data,
                        borderColor: '#d97706',
                        backgroundColor: 'rgba(217,119,6,0.08)',
                        fill: true,
                        tension: 0.4,
                        pointRadius: 4,
                        pointHoverRadius: 7,
                        pointBackgroundColor: '#d97706',
                        borderWidth: 2.5
                    },
                    {
                        label: dataset2.name,
                        data: dataset2.data,
                        borderColor: '#059669',
                        backgroundColor: 'rgba(5,150,105,0.08)',
                        fill: true,
                        tension: 0.4,
                        pointRadius: 4,
                        pointHoverRadius: 7,
                        pointBackgroundColor: '#059669',
                        borderWidth: 2.5
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        align: 'end',
                        labels: {
                            usePointStyle: true,
                            pointStyleWidth: 10
                        }
                    }
                },
                scales: {
                    y: { beginAtZero: true, grid: { color: colors.gridColor } },
                    x: { grid: { display: false } }
                },
                interaction: { mode: 'index', intersect: false },
                animation: { duration: 1800, easing: 'easeOutQuart' }
            }
        });
    },

    // ── Refresh all charts on theme change ──
    refreshCharts: function () {
        var self = window.irmInterop;
        // Save chart configs and recreate them
        var chartIds = Object.keys(self._charts);
        chartIds.forEach(function (id) {
            var chart = self._charts[id];
            if (chart) {
                var colors = self._getChartColors();
                Chart.defaults.color = colors.textColor;
                Chart.defaults.borderColor = colors.borderColor;

                // Update grid and border colors
                if (chart.options.scales) {
                    if (chart.options.scales.y) {
                        chart.options.scales.y.grid.color = colors.gridColor;
                    }
                }

                // Update doughnut border color
                if (chart.config.type === 'doughnut') {
                    chart.data.datasets.forEach(function (ds) {
                        ds.borderColor = colors.surfaceColor;
                    });
                }

                chart.update();
            }
        });
    }
};
