/* Only public static assets are cached. HTML, credentials and API data are never cached. */
const CACHE_PREFIX = 'Mkx.Templates-static-';
const CACHE_NAME = CACHE_PREFIX + '__BUILD_ID__';
const OFFLINE_URL = '/offline.html';
const PRECACHE = [OFFLINE_URL, '/icon-192.png', '/icon-512.png', '/favicon.png'];
self.addEventListener('install', event => event.waitUntil(caches.open(CACHE_NAME).then(cache => cache.addAll(PRECACHE))));
self.addEventListener('activate', event => event.waitUntil((async () => {
    const names = await caches.keys();
    await Promise.all(names.filter(name => name.startsWith(CACHE_PREFIX) && name !== CACHE_NAME).map(name => caches.delete(name)));
    await self.clients.claim();
})()));
self.addEventListener('fetch', event => {
    const request = event.request;
    const url = new URL(request.url);
    if (request.method !== 'GET' || url.origin !== self.location.origin) return;
    const path = url.pathname.toLowerCase();
    if (path.startsWith('/api/') || path === '/api' || path.startsWith('/account') || path.startsWith('/health/') || path.startsWith('/logs')) return;
    if (request.mode === 'navigate') {
        event.respondWith(fetch(request).catch(async () => (await caches.open(CACHE_NAME)).match(OFFLINE_URL)));
        return;
    }
    const isPublicAsset = /^\/(?:_framework|_content|css|js|fonts|assets)\//.test(path) || PRECACHE.includes(url.pathname);
    if (!isPublicAsset || url.search) return;
    // Start cache work while the event is active, then await it via waitUntil.
    const network = fetch(request);
    event.waitUntil(network.then(async response => {
        if (response.ok && response.type === 'basic' && !/no-store|private/i.test(response.headers.get('Cache-Control') || '') && !response.headers.get('Content-Type')?.includes('text/html'))
            await (await caches.open(CACHE_NAME)).put(request, response.clone());
    }).catch(() => {}));
    event.respondWith(network.catch(async () => (await (await caches.open(CACHE_NAME)).match(request)) || Response.error()));
});
// Waiting workers activate only after a deliberate user action.
self.addEventListener('message', event => {
    if (event.data?.action === 'activateUpdate') event.waitUntil(self.skipWaiting());
});
// Web Push event handling uses the SAME service worker as PWA updates/offline support.
const localNotificationUrl = value => {
    try {
        const url = new URL(typeof value === 'string' ? value : '/', self.location.origin);
        return url.origin === self.location.origin ? url.pathname + url.search + url.hash : '/';
    } catch { return '/'; }
};
self.addEventListener('push', event => {
    let data = {};
    try { data = event.data?.json() ?? {}; } catch {}
    const title = typeof data.title === 'string' && data.title.trim() ? data.title : 'اعلان جدید';
    const body = typeof data.body === 'string' ? data.body : '';
    const tag = typeof data.tag === 'string' ? data.tag : undefined;
    event.waitUntil(self.registration.showNotification(title, {
        body, icon: '/icon-192.png', badge: '/icon-192.png', tag,
        data: { url: localNotificationUrl(data.url) }
    }));
});
self.addEventListener('notificationclick', event => {
    event.notification.close();
    const path = localNotificationUrl(event.notification.data?.url);
    event.waitUntil((async () => {
        const clients = await self.clients.matchAll({ type: 'window', includeUncontrolled: true });
        for (const client of clients) {
            if ('navigate' in client) await client.navigate(path);
            if ('focus' in client) return client.focus();
        }
        return self.clients.openWindow(path);
    })());
});
