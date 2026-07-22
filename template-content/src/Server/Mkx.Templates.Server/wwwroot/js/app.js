window.Mkx = window.Mkx || {};
window.Mkx.removeSplash = function () {
    var el = document.getElementById('app-loading');
    if (el) {
        el.classList.add('fade-out');
        setTimeout(function () { el.remove(); }, 1000);
    }
};