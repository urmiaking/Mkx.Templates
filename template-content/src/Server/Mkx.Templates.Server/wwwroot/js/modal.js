// Modal dialogs enter the browser top layer via showModal(): stacking contexts
// created by page transitions must never clip the scrim or expose the AppBar.
const activeDialogs = new WeakMap();
let activeCount = 0;

export function activate(panel, dialog) {
    if (!panel || !dialog || activeDialogs.has(panel)) return;
    const previous = document.activeElement;
    if (!dialog.open) dialog.showModal();

    // The browser's native Escape action must not bypass Blazor's Busy/OnClose
    // policy. UiModal handles Escape through its @onkeydown callback instead.
    const preventNativeCancel = event => event.preventDefault();
    dialog.addEventListener('cancel', preventNativeCancel);

    const focusable = () => [...panel.querySelectorAll(
        'button:not([disabled]),input:not([disabled]),select:not([disabled]),textarea:not([disabled]),a[href],[tabindex]:not([tabindex="-1"])'
    )].filter(x => x.getClientRects().length);

    panel.setAttribute('tabindex', '-1');
    (focusable()[0] ?? panel).focus({preventScroll:true});

    const trap = event => {
        if (event.key !== 'Tab') return;
        const all = focusable();
        if (!all.length) { event.preventDefault(); panel.focus(); return; }
        const first = all[0], last = all[all.length - 1];
        if (event.shiftKey && (document.activeElement === first || document.activeElement === panel)) {
            event.preventDefault();
            last.focus();
        } else if (!event.shiftKey && document.activeElement === last) {
            event.preventDefault();
            first.focus();
        }
    };

    panel.addEventListener('keydown', trap);
    activeDialogs.set(panel, { previous, trap, preventNativeCancel });
    activeCount += 1;
    document.documentElement.classList.add('ui-modal-open');
}

export function deactivate(panel, dialog) {
    const state = activeDialogs.get(panel);
    if (!state) return;

    panel.removeEventListener('keydown', state.trap);
    dialog?.removeEventListener('cancel', state.preventNativeCancel);
    if (dialog?.open) dialog.close();

    activeDialogs.delete(panel);
    activeCount = Math.max(0, activeCount - 1);
    if (activeCount === 0) document.documentElement.classList.remove('ui-modal-open');
    if (state.previous?.isConnected) state.previous.focus({preventScroll:true});
}
