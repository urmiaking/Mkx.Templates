(() => {
    const root = document.documentElement;
    const percent = new Intl.NumberFormat('fa-IR', { style: 'percent' });
    let splash;
    const observer = new MutationObserver(updateProgress);

    function renderProgress(value) {
        const progress = splash?.querySelector('#splash-progress');
        if (!progress) return;
        progress.value = Math.min(100, Math.max(0, value));
        splash.querySelector('#splash-percentage').textContent = percent.format(Math.floor(progress.value) / 100);
        if (progress.value === 100) {
            splash.querySelector('#splash-phase').textContent = 'در حال آماده‌سازی برنامه';
        }
    }

    function updateProgress() {
        // Blazor reports completed boot resources on the document root.
        const value = parseFloat(root.style.getPropertyValue('--blazor-load-percentage'));
        if (Number.isFinite(value)) renderProgress(value);
    }

    function observeSplash() {
        const current = document.getElementById('app-loading');
        if (current === splash) return;
        observer.disconnect();
        splash = current;
        if (!splash) return;
        updateProgress();
        observer.observe(root, { attributes: true, attributeFilter: ['style'] });
    }

    window.Mkx = window.Mkx || {};
    window.Mkx.removeSplash = () => {
        const current = document.getElementById('app-loading');
        if (!current || current.classList.contains('fade-out')) return;
        observer.disconnect();
        // The interactive layout has rendered; startup is genuinely complete.
        splash = current;
        renderProgress(100);
        if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
            current.remove();
            splash = null;
            return;
        }
        current.classList.add('fade-out');
        window.setTimeout(() => {
            current.remove();
            if (splash === current) splash = null;
        }, 220);
    };

    observeSplash();
    window.Blazor?.addEventListener('enhancedload', observeSplash);
})();
