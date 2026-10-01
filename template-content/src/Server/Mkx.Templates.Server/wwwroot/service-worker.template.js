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
