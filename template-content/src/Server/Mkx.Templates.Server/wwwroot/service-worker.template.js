const CACHE_NAME = 'Mkx.Templates-pwa-cache-v2';

const ASSETS_TO_CACHE = [
  '/',
  '/manifest.json',
  '/icon-192.png',
  '/icon-512.png',
  '/favicon.png',
  '/css/app.css',
  '/_content/MudBlazor/MudBlazor.min.css',
  '/_content/MudBlazor/MudBlazor.min.js',
  '/_framework/blazor.web.js'
];

// Install Event
self.addEventListener('install', event => {
  self.skipWaiting(); // Force the waiting service worker to become the active service worker immediately during initial install if possible
  event.waitUntil(
    caches.open(CACHE_NAME).then(cache => {
      return cache.addAll(ASSETS_TO_CACHE);
    })
  );
});

// Activate Event
self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames.map(cache => {
          if (cache !== CACHE_NAME) {
            return caches.delete(cache);
          }
        })
      );
    }).then(() => {
      return self.clients.claim();
    })
  );
});

// Fetch Event
self.addEventListener('fetch', event => {
  const url = new URL(event.request.url);

  // Bypass API calls completely and prevent caching
  if (url.pathname.toLowerCase().startsWith('/api')) {
    if (event.request.method === 'GET') {
      event.respondWith(fetch(event.request, { cache: 'no-store' }));
    }
    return;
  }

  // Bypass Account pages completely (static SSR auth pages)
  if (url.pathname.toLowerCase().startsWith('/account')) {
    return;
  }

  // Ignore non-GET requests
  if (event.request.method !== 'GET') {
    return;
  }

  // Online First: Network first, falling back to cache
  event.respondWith(
    fetch(event.request)
      .then(response => {
        // If response is valid, clone and update cache
        if (response && response.status === 200 && response.type === 'basic') {
          const responseToCache = response.clone();
          caches.open(CACHE_NAME).then(cache => {
            cache.put(event.request, responseToCache);
          });
        }
        return response;
      })
      .catch(() => {
        // Offline Fallback
        return caches.match(event.request).then(cachedResponse => {
          if (cachedResponse) {
            return cachedResponse;
          }
          // If navigation page requests fail, fall back to cached shell
          if (event.request.mode === 'navigate') {
            return caches.match('/');
          }
        });
      })
  );
});

// Skip Waiting
self.addEventListener('message', event => {
  if (event.data && event.data.action === 'skipWaiting') {
    self.skipWaiting();
  }
});
