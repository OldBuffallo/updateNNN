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
                    borderRadius: 999,
                    borderSkipped: false,
                    maxBarThickness: 22
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: {
                        beginAtZero: true,
                        border: { display: false },
                        grid: { color: colors.gridColor, tickBorderDash: [4, 4] }
                    },
                    x: {
                        border: { display: false },
                        grid: { display: false }
                    }
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
                    borderWidth: 4,
                    borderColor: colors.surfaceColor,
                    borderRadius: 6,
                    spacing: 3
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '66%',
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
                        pointRadius: 5,
                        pointHoverRadius: 7,
                        pointBackgroundColor: colors.surfaceColor || '#ffffff',
                        pointBorderColor: colors.series[4],
                        pointBorderWidth: 2.5,
                        borderWidth: 2.5
                    },
                    {
                        label: dataset2.name,
                        data: dataset2.data,
                        borderColor: colors.series[3],
                        backgroundColor: colors.successFill,
                        fill: true,
                        tension: 0.4,
                        pointRadius: 5,
                        pointHoverRadius: 7,
                        pointBackgroundColor: colors.surfaceColor || '#ffffff',
                        pointBorderColor: colors.series[3],
                        pointBorderWidth: 2.5,
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
                    y: {
                        beginAtZero: true,
                        border: { display: false },
                        grid: { color: colors.gridColor }
                    },
                    x: {
                        border: { display: false },
                        grid: { display: false }
                    }
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
                        ds.pointBackgroundColor = colors.surfaceColor || '#ffffff';
                        ds.pointBorderColor = ds.borderColor;
                        ds.backgroundColor = index === 0 ? colors.warningFill : colors.successFill;
                    }
                });

                chart.update();
            }
        });
    },

    // ── Responsive Table Data-Label Sync (for <= 768px Mobile Card View) ──
    _labelSyncPending: false,
    syncTableLabels: function () {
        if (typeof document === 'undefined' || typeof document.querySelectorAll !== 'function') return;
        var tables = document.querySelectorAll('.mud-table-root, .mud-simple-table table');
        tables.forEach(function (table) {
            var headers = Array.prototype.map.call(table.querySelectorAll('thead th'), function (th) {
                var titleEl = th.querySelector('.mud-column-header-text');
                if (titleEl) return (titleEl.textContent || '').trim();
                if (typeof th.cloneNode === 'function') {
                    var clone = th.cloneNode(true);
                    Array.prototype.forEach.call(clone.querySelectorAll('.mud-icon-root, .mud-button-root, svg'), function (n) {
                        if (n && n.parentNode) n.parentNode.removeChild(n);
                    });
                    return (clone.textContent || '').trim();
                }
                return (th.textContent || '').trim();
            });
            if (!headers.length) return;
            var rows = table.querySelectorAll('tbody tr:not(.group-header)');
            rows.forEach(function (row) {
                var cells = row.querySelectorAll('td');
                cells.forEach(function (td, idx) {
                    var label = headers[idx];
                    if (label && td.getAttribute('data-label') !== label) {
                        td.setAttribute('data-label', label);
                    }
                });
            });
        });
    },

    // ── Customer Feedback Spotlight & Initial Highlight (R6) ──
    _feedbackDriver: null,
    initFeedbackSpotlight: function (forceReplay) {
        try {
            if (!forceReplay && localStorage.getItem('irm-r6-feedback-seen') === '1') {
                return false;
            }
        } catch (e) {
            if (!forceReplay) return false;
        }

        var btn = document.getElementById('btnCustomerFeedback');
        if (!btn) return false;

        if (btn.classList && typeof btn.classList.add === 'function') {
            btn.classList.add('irm-feedback-highlight');
        }
        if (!btn._irmFeedbackBound && typeof btn.addEventListener === 'function') {
            btn._irmFeedbackBound = true;
            btn.addEventListener('click', function () {
                window.irmInterop.dismissFeedbackSpotlight();
            });
        }

        if (window.driver && window.driver.js && typeof window.driver.js.driver === 'function') {
            try {
                var reduced = typeof window.matchMedia === 'function'
                    && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
                var drv = window.driver.js.driver({
                    animate: !reduced,
                    showProgress: false,
                    allowClose: true,
                    overlayOpacity: 0.45,
                    stagePadding: 6,
                    stageRadius: 10,
                    popoverClass: 'irm-feedback-popover',
                    doneBtnText: 'Đã hiểu — Bắt đầu dùng thử',
                    closeBtnText: 'Đóng',
                    onDestroyStarted: function () {
                        window.irmInterop.dismissFeedbackSpotlight();
                        if (window.irmInterop._feedbackDriver) {
                            var instance = window.irmInterop._feedbackDriver;
                            window.irmInterop._feedbackDriver = null;
                            if (typeof instance.destroy === 'function') {
                                instance.destroy();
                            }
                        }
                    },
                    steps: [
                        {
                            element: '#btnCustomerFeedback',
                            popover: {
                                title: '📍 Phiếu Khảo Sát & Góp Ý Nghiệm Thu (R6)',
                                description: 'Nhấn nút "Khảo sát & Góp ý" trên thanh tiêu đề bất kỳ lúc nào để đánh giá từng chức năng, giao diện & định dạng Excel (mở bản HTML trực tiếp hoặc tải file Excel).',
                                side: 'bottom',
                                align: 'end'
                            }
                        }
                    ]
                });
                window.irmInterop._feedbackDriver = drv;
                drv.drive(0);
            } catch (err) {
                // Keep CSS pulse highlight even if Driver.js fails
            }
        }
        return true;
    },
    dismissFeedbackSpotlight: function () {
        try {
            localStorage.setItem('irm-r6-feedback-seen', '1');
        } catch (e) {}
        var btn = document.getElementById('btnCustomerFeedback');
        if (btn && btn.classList && typeof btn.classList.remove === 'function') {
            btn.classList.remove('irm-feedback-highlight');
        }
    },

    // ── Global Shortcut Listener (Ctrl+K / Cmd+K) ──
    _dotNetRef: null,
    _shortcutsInitialized: false,
    registerShortcutHandler: function (dotNetHelper) {
        window.irmInterop._dotNetRef = dotNetHelper;
    },
    initGlobalShortcuts: function () {
        if (window.irmInterop._shortcutsInitialized) {
            window.irmInterop.syncTableLabels();
            return;
        }
        window.irmInterop._shortcutsInitialized = true;
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
                if (window.irmInterop._labelSyncPending) return;
                window.irmInterop._labelSyncPending = true;
                var schedule = typeof requestAnimationFrame === 'function'
                    ? requestAnimationFrame
                    : function (fn) { setTimeout(fn, 16); };
                schedule(function () {
                    window.irmInterop._labelSyncPending = false;
                    window.irmInterop.syncTableLabels();
                });
            });
            observer.observe(document.body, { childList: true, subtree: true });
        }
    }
};

// Initialize shortcuts immediately if DOM is already interactive/complete, otherwise wait for DOMContentLoaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function () {
        window.irmInterop.initGlobalShortcuts();
    });
} else {
    window.irmInterop.initGlobalShortcuts();
}
