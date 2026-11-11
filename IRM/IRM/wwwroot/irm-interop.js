/* ============================================ */
/*  IRM v1.0.2 — JS Interop                     */
/* ============================================ */

window.irmInterop = {
    // ── Dark Mode ──
    setTheme: function (theme) {
        theme = theme === 'dark' ? 'dark' : 'light';
        document.documentElement.setAttribute('data-theme', theme);
        try { localStorage.setItem('irm-theme', theme); } catch (_) { /* Session preference still applies. */ }
    },
    getTheme: function () {
        return window.irmInterop.initTheme();
    },
    initTheme: function () {
        var saved = 'light';
        try { saved = localStorage.getItem('irm-theme') === 'dark' ? 'dark' : 'light'; } catch (_) { /* Use light fallback. */ }
        document.documentElement.setAttribute('data-theme', saved);
        return saved;
    },
    applyTheme: async function (theme) {
        window.irmInterop.setTheme(theme);
        await new Promise(function (resolve) { requestAnimationFrame(resolve); });
        window.irmInterop.refreshCharts();
    },

    // ── Table Density (Comfortable vs Compact) ──
    setDensity: function (density) {
        density = density === 'compact' ? 'compact' : 'comfortable';
        document.documentElement.setAttribute('data-density', density);
        try { localStorage.setItem('irm-density', density); } catch (_) { /* Session preference still applies. */ }
        return density;
    },
    getDensity: function () {
        var saved = 'comfortable';
        try { saved = localStorage.getItem('irm-density') === 'compact' ? 'compact' : 'comfortable'; } catch (_) { /* Default comfortable */ }
        document.documentElement.setAttribute('data-density', saved);
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
        var style = getComputedStyle(document.documentElement);
        var read = function (name) { return style.getPropertyValue(name).trim(); };
        var series = [1, 2, 3, 4, 5].map(function (index) { return read('--chart-' + index); });
        return {
            textColor: read('--text-secondary'),
            gridColor: read('--border'),
            borderColor: read('--border'),
            surfaceColor: read('--bg-surface'),
            series: series,
            permits: [series[3], series[0], series[4], read('--text-muted') || read('--text-secondary')],
            warningFill: read('--chart-warning-fill') || 'transparent',
            successFill: read('--chart-success-fill') || 'transparent'
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
                    backgroundColor: colors.series,
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
                    backgroundColor: colors.permits,
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
                        borderColor: colors.series[4],
                        backgroundColor: colors.warningFill,
                        fill: true,
                        tension: 0.4,
                        pointRadius: 4,
                        pointHoverRadius: 7,
                        pointBackgroundColor: colors.series[4],
                        borderWidth: 2.5
                    },
                    {
                        label: dataset2.name,
                        data: dataset2.data,
                        borderColor: colors.series[3],
                        backgroundColor: colors.successFill,
                        fill: true,
                        tension: 0.4,
                        pointRadius: 4,
                        pointHoverRadius: 7,
                        pointBackgroundColor: colors.series[3],
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

                chart.options.color = colors.textColor;
                Object.values(chart.options.scales || {}).forEach(function (scale) {
                    scale.ticks = scale.ticks || {};
                    scale.ticks.color = colors.textColor;
                    if (scale.grid) scale.grid.color = colors.gridColor;
                });
                var legend = chart.options.plugins && chart.options.plugins.legend;
                if (legend && legend.labels) legend.labels.color = colors.textColor;

                chart.data.datasets.forEach(function (ds, index) {
                    if (chart.config.type === 'doughnut') {
                        ds.borderColor = colors.surfaceColor;
                        ds.backgroundColor = colors.permits;
                    } else if (chart.config.type === 'bar') {
                        ds.backgroundColor = colors.series;
                    } else if (chart.config.type === 'line') {
                        ds.borderColor = colors.series[index === 0 ? 4 : 3];
                        ds.pointBackgroundColor = ds.borderColor;
                        ds.backgroundColor = index === 0 ? colors.warningFill : colors.successFill;
                    }
                });

                chart.update();
            }
        });
    },

    // ── Responsive Table Data-Label Sync (for <= 768px Mobile Card View) ──
    syncTableLabels: function () {
        if (typeof document === 'undefined' || typeof document.querySelectorAll !== 'function') return;
        var tables = document.querySelectorAll('.mud-table-root, .mud-simple-table table');
        tables.forEach(function (table) {
            var headers = Array.prototype.map.call(table.querySelectorAll('thead th'), function (th) {
                var titleEl = th.querySelector('.mud-column-header-text');
                return (titleEl ? titleEl.textContent : th.textContent || '').trim();
            });
            if (!headers.length) return;
            var rows = table.querySelectorAll('tbody tr:not(.group-header)');
            rows.forEach(function (row) {
                var cells = row.querySelectorAll('td');
                cells.forEach(function (td, idx) {
                    if (headers[idx] && !td.getAttribute('data-label')) {
                        td.setAttribute('data-label', headers[idx]);
                    }
                });
            });
        });
    },

    // ── Global Shortcut Listener (Ctrl+K / Cmd+K) ──
    _dotNetRef: null,
    registerShortcutHandler: function (dotNetHelper) {
        window.irmInterop._dotNetRef = dotNetHelper;
    },
    initGlobalShortcuts: function () {
        document.addEventListener('keydown', function (e) {
            if ((e.ctrlKey || e.metaKey) && (e.key === 'k' || e.key === 'K')) {
                e.preventDefault();
                if (window.irmInterop._dotNetRef) {
                    window.irmInterop._dotNetRef.invokeMethodAsync('OpenSpotlight');
                } else {
                    var btn = document.getElementById('btnGlobalSpotlight');
                    if (btn) btn.click();
                }
            }
        });
        window.irmInterop.syncTableLabels();
        if (typeof MutationObserver === 'function' && document.body) {
            var observer = new MutationObserver(function () {
                window.irmInterop.syncTableLabels();
            });
            observer.observe(document.body, { childList: true, subtree: true });
        }
    }
};

// Initialize shortcuts on load
document.addEventListener('DOMContentLoaded', function () {
    window.irmInterop.initGlobalShortcuts();
});
