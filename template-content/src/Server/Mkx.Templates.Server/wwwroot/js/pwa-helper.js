(() => {
    let acceptedUpdate = false;
    let refreshing = false;
    let deferredPrompt;
    const changedForms = new Set();
    document.addEventListener('input', event => { if (event.target.closest('form')) changedForms.add(event.target.closest('form')); }, { passive: true });
    const hasChanges = () => [...changedForms].some(form => form.isConnected) || document.querySelector('[data-unsaved="true"]');
    function banner(id, text, label, action) {
        if (document.getElementById(id)) return;
        const panel = document.createElement('aside'); panel.id = id; panel.className = 'pwa-toast'; panel.setAttribute('aria-live', 'polite');
        const body = document.createElement('p'); body.textContent = text;
        const accept = document.createElement('button'); accept.className = 'pwa-btn'; accept.textContent = label;
        const later = document.createElement('button'); later.className = 'pwa-btn'; later.textContent = 'بعداً';
        accept.onclick = async () => { accept.disabled = true; try { if (await action() !== false) panel.remove(); } finally { accept.disabled = false; } };
        later.onclick = () => { panel.remove(); sessionStorage.setItem(id, 'dismissed'); };
        panel.append(body, accept, later); document.body.append(panel);
    }
    function offerUpdate(worker) {
        banner('pwa-update', 'نسخهٔ جدید آماده است. پس از ذخیرهٔ کارها آن را فعال کنید.', 'به‌روزرسانی', () => {
            if (hasChanges() && !confirm('تغییرات ذخیره نشده دارید. کنار گذاشته شوند و برنامه به‌روزرسانی شود؟')) return false;
            acceptedUpdate = true; worker.postMessage({ action: 'activateUpdate' });
        });
    }
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.addEventListener('controllerchange', () => {
            if (acceptedUpdate && !refreshing) { refreshing = true; location.reload(); }
        });
        window.addEventListener('load', async () => {
            try {
                const registration = await navigator.serviceWorker.register('/service-worker.js', { updateViaCache: 'none' });
                if (registration.waiting) offerUpdate(registration.waiting);
                registration.addEventListener('updatefound', () => {
                    const worker = registration.installing;
                    worker?.addEventListener('statechange', () => {
                        if (worker.state === 'installed' && navigator.serviceWorker.controller) offerUpdate(worker);
                    });
                });
            } catch { console.warn('Offline support is unavailable.'); }
        });
    }
    window.addEventListener('beforeinstallprompt', event => {
        event.preventDefault(); deferredPrompt = event;
        if (sessionStorage.getItem('pwa-install') !== 'dismissed') banner('pwa-install', 'برای دسترسی سریع‌تر برنامه را نصب کنید.', 'نصب', async () => {
            await deferredPrompt.prompt(); await deferredPrompt.userChoice; deferredPrompt = null;
        });
    });
    window.addEventListener('appinstalled', () => { document.getElementById('pwa-install')?.remove(); deferredPrompt = null; });
    window.addEventListener('load', () => {
        const ios = /iPad|iPhone|iPod/.test(navigator.userAgent);
        if (ios && !navigator.standalone && !matchMedia('(display-mode: standalone)').matches && sessionStorage.getItem('pwa-ios') !== 'dismissed')
            banner('pwa-ios', 'برای نصب در Safari از Share و سپس Add to Home Screen استفاده کنید.', 'متوجه شدم', () => { sessionStorage.setItem('pwa-ios', 'dismissed'); });
    });
})();
