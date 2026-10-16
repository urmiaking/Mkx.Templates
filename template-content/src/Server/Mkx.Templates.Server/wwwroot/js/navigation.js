// Keep the mobile overlay keyboard-contained without affecting the permanent desktop navigation.
export function bind(panel) {
    const mobile = matchMedia('(max-width: 959.98px)');
    let previous = null;
    let expanded = false;
    const blocked = () => [...document.querySelectorAll('.workspace-main,.workspace-bottom-nav,.workspace-topbar')];
    const controls = () => [...panel.querySelectorAll('a[href],button:not([disabled])')].filter(x => x.getClientRects().length);
    const sync = () => {
        const modal = expanded && mobile.matches;
        document.documentElement.classList.toggle('workspace-menu-open', modal);
        blocked().forEach(element => { element.inert = modal; });
        panel.setAttribute('role', modal ? 'dialog' : 'complementary');
        if (modal) panel.setAttribute('aria-modal', 'true'); else panel.removeAttribute('aria-modal');
        if (modal) (controls()[0] ?? panel).focus({preventScroll:true});
    };
    const keydown = event => {
        if (!expanded || !mobile.matches) return;
        if (event.key !== 'Tab') return;
        const all = controls(), first = all[0], last = all.at(-1);
        if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last?.focus(); }
        else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first?.focus(); }
    };
    panel.addEventListener('keydown', keydown);
    mobile.addEventListener('change', sync);
    return {
        update(open) {
            if (open === expanded) return;
            if (open) previous = document.activeElement;
            expanded = open;
            sync();
            if (!open && previous?.isConnected && mobile.matches) previous.focus({preventScroll:true});
        },
        dispose() {
            document.documentElement.classList.remove('workspace-menu-open');
            blocked().forEach(element => { element.inert = false; });
            panel.removeEventListener('keydown', keydown);
            mobile.removeEventListener('change', sync);
        }
    };
}
