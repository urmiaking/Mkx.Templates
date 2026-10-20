window.Mkx = window.Mkx || {};
Mkx.removeSplash = () => document.getElementById('app-loading')?.remove();
Mkx.systemDark = () => matchMedia('(prefers-color-scheme: dark)').matches;
Mkx.setMobilePickerHidden = hidden => { document.documentElement.dataset.mobilePickerHidden = String(hidden === true); };
Mkx.setPalette = async (palette, animate = true, source = null) => {
    const root = document.documentElement;
    const valid = ['jade', 'ocean', 'iris', 'amber'].includes(palette) ? palette : 'jade';
    if (root.dataset.palette === valid) return;
    await Mkx.revealAppearance(() => {
        root.dataset.palette = valid;
        Mkx.updateThemeColor();
    }, animate ? source : null, false, 'palette');
};
Mkx.updateThemeColor = () => {
    const root = document.documentElement;
    const backgrounds = { jade: ['#f4f8fb', '#10191e'], ocean: ['#f3f7fd', '#101725'],
        iris: ['#f7f5fc', '#191422'], amber: ['#fbf7f0', '#1c1711'] };
    const colors = backgrounds[root.dataset.palette] || backgrounds.jade;
    document.querySelector('meta[name="theme-color"]')?.setAttribute('content', colors[root.dataset.theme === 'dark' ? 1 : 0]);
};
Mkx.applyAppearance = (dark) => {
    document.documentElement.dataset.theme = dark ? 'dark' : 'light';
    document.documentElement.style.colorScheme = dark ? 'dark' : 'light';
    Mkx.updateThemeColor();
};
Mkx.setAppearance = async (dark, source) => {
    if (document.documentElement.dataset.theme === (dark ? 'dark' : 'light')) return;
    await Mkx.revealAppearance(() => Mkx.applyAppearance(dark), source, !dark, 'radial');
};
// One snapshot transition owns both mode and palette changes; colors swap atomically beneath it.
Mkx.revealAppearance = async (apply, source, reverse, kind) => {
    const root = document.documentElement;
    const request = Mkx.appearanceRequest = (Mkx.appearanceRequest || 0) + 1;
    Mkx.appearanceTransition?.skipTransition();
    if (!source?.getBoundingClientRect || !document.startViewTransition ||
        matchMedia('(prefers-reduced-motion: reduce)').matches) {
        Mkx.appearanceTransition = null;
        delete root.dataset.themeTransition;
        apply();
        return;
    }
    const bounds = source.getBoundingClientRect();
    const x = bounds.left + bounds.width / 2;
    const y = bounds.top + bounds.height / 2;
    const radius = Math.ceil(Math.hypot(Math.max(x, innerWidth - x), Math.max(y, innerHeight - y)));
    root.dataset.themeTransition = kind;
    const transition = document.startViewTransition(() => {
        if (request === Mkx.appearanceRequest) apply();
    });
    Mkx.appearanceTransition = transition;
    let reveal;
    try {
        await transition.ready;
        if (request !== Mkx.appearanceRequest) {
            transition.skipTransition();
            return;
        }
        const clips = [`circle(0px at ${x}px ${y}px)`, `circle(${radius}px at ${x}px ${y}px)`];
        reveal = root.animate({ clipPath: reverse ? clips.reverse() : clips }, {
            duration: 800,
            easing: reverse ? 'cubic-bezier(.64,0,.78,0)' : 'cubic-bezier(.22,1,.36,1)',
            pseudoElement: reverse ? '::view-transition-old(root)' : '::view-transition-new(root)',
            // Retain the final clip until the snapshot tree disappears, including interrupted reveals.
            fill: 'both',
        });
        await reveal.finished;
    } catch {
        transition.skipTransition();
        if (request === Mkx.appearanceRequest) apply();
    } finally {
        await transition.finished.catch(() => {});
        reveal?.cancel();
        if (Mkx.appearanceTransition === transition) {
            Mkx.appearanceTransition = null;
            delete root.dataset.themeTransition;
        }
    }
};
Mkx.restoreAppearance = () => {
    try { Mkx.setMobilePickerHidden(localStorage.getItem('Theme.MobilePickerHidden')?.toLowerCase() === 'true'); }
    catch { Mkx.setMobilePickerHidden(false); }
    try { Mkx.setPalette(localStorage.getItem('Theme.Palette'), false); }
    catch { Mkx.setPalette('jade', false); }
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
