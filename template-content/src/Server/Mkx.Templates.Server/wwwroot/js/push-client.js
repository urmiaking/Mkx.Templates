// Web Push uses the existing root-scope PWA service worker; never registers a second worker.
(() => {
    const supported = () => window.isSecureContext && 'serviceWorker' in navigator &&
        'PushManager' in window && 'Notification' in window;
    const optOutKey = 'mkx-push-opt-out';
    const optedOut = () => { try { return localStorage.getItem(optOutKey) === '1'; } catch { return false; } };
    const setOptOut = value => { try {
        if (value) localStorage.setItem(optOutKey, '1');
        else localStorage.removeItem(optOutKey);
    } catch {} };
    const registration = async () => {
        if (!supported()) return null;
        // May be absent in Development: register on explicit user action or granted restore.
        return navigator.serviceWorker.getRegistration('/').then(existing =>
            existing || navigator.serviceWorker.register('/service-worker.js', { updateViaCache: 'none' }));
    };
    const current = async () => {
        if (!supported()) return null;
        const existing = await navigator.serviceWorker.getRegistration('/');
        return existing ? existing.pushManager.getSubscription() : null;
    };
    const encodeKey = key => {
        const str = atob((key + '='.repeat((4 - key.length % 4) % 4)).replace(/-/g, '+').replace(/_/g, '/'));
        return Uint8Array.from(str, ch => ch.charCodeAt(0));
    };
    const state = async () => {
        if (!supported()) return { supported: false, permission: 'unsupported', subscribed: false, endpoint: null };
        const sub = await current();
        return { supported: true, permission: Notification.permission, subscribed: !!sub, endpoint: sub?.endpoint ?? null };
    };
    const subscribe = async (key, prompt) => {
        if (!supported() || (!prompt && (Notification.permission !== 'granted' || optedOut())))
            return { supported: supported(), permission: supported() ? Notification.permission : 'unsupported', subscribed: false };
        let permission = Notification.permission;
        if (prompt && permission === 'default') permission = await Notification.requestPermission();
        if (permission !== 'granted') return { supported: true, permission, subscribed: false };
        const reg = await registration();
        let sub = await reg.pushManager.getSubscription();
        if (sub) {
            const old = sub.options?.applicationServerKey;
            if (old && encodeKey(key).toString() !== new Uint8Array(old).toString()) {
                await sub.unsubscribe(); sub = null;
            }
        }
        sub ||= await reg.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: encodeKey(key) });
        setOptOut(false);
        const json = sub.toJSON();
        return {
            supported: true, permission: 'granted', subscribed: true,
            endpoint: sub.endpoint, p256dh: json.keys?.p256dh, auth: json.keys?.auth,
            deviceName: navigator.userAgentData?.platform ?? navigator.platform ?? 'Browser'
        };
    };
    window.mkxPush = {
        state,
        subscribe: key => subscribe(key, true),
        subscribeGranted: key => subscribe(key, false),
        unsubscribe: async (optOut = true) => {
            if (optOut) setOptOut(true);
            const sub = await current();
            return sub ? sub.unsubscribe() : false;
        }
    };
})();
