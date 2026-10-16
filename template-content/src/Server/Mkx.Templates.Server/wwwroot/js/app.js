window.Mkx = window.Mkx || {};
Mkx.removeSplash = () => document.getElementById('app-loading')?.remove();
Mkx.systemDark = () => matchMedia('(prefers-color-scheme: dark)').matches;
Mkx.setAppearance = (dark) => {
    document.documentElement.dataset.theme = dark ? 'dark' : 'light';
    document.documentElement.style.colorScheme = dark ? 'dark' : 'light';
    document.querySelector('meta[name="theme-color"]')?.setAttribute('content', dark ? '#111a20' : '#f4f8fb');
};
Mkx.restoreAppearance = () => {
    try { const saved = localStorage.getItem('Theme.IsDarkMode'); Mkx.setAppearance(saved === null ? Mkx.systemDark() : saved.toLowerCase() === 'true'); }
    catch { Mkx.setAppearance(Mkx.systemDark()); }
};
Mkx.restoreAppearance();
Mkx.watchSystemAppearance = receiver => {
    Mkx.unwatchSystemAppearance();
    Mkx.appearanceListener = () => {
        try {
            if (localStorage.getItem('Theme.IsDarkMode') === null)
                receiver.invokeMethodAsync('SystemAppearanceChanged', Mkx.systemDark()).catch(() => {});
        } catch { Mkx.restoreAppearance(); }
    };
    matchMedia('(prefers-color-scheme: dark)').addEventListener('change', Mkx.appearanceListener);
};
Mkx.unwatchSystemAppearance = () => {
    if (Mkx.appearanceListener) matchMedia('(prefers-color-scheme: dark)').removeEventListener('change', Mkx.appearanceListener);
    Mkx.appearanceListener = null;
};
