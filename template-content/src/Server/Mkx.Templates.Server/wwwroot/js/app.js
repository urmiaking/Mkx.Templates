window.Mkx = window.Mkx || {};
window.Mkx.removeSplash = () => document.getElementById('app-loading')?.remove();
window.Mkx.setAppearance = (dark) => {
    document.documentElement.style.colorScheme = dark ? 'dark' : 'light';
    document.querySelector('meta[name="theme-color"]')?.setAttribute('content', dark ? '#111827' : '#f8fafc');
};
// Apply preferences before the first interactive render. Corrupt/disabled storage falls back safely.
try { Mkx.setAppearance(localStorage.getItem('Theme.IsDarkMode')?.toLowerCase() === 'true'); } catch { }
