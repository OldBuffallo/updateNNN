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
    }
};
