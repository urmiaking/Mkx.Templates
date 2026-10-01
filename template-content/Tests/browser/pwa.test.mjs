import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';
const source = readFileSync(new URL('../../src/Server/Mkx.Templates.Server/wwwroot/service-worker.template.js', import.meta.url), 'utf8').replace('__BUILD_ID__', 'test-build');
function worker() {
    const handlers = new Map(), stores = new Map();
    let skipped = 0, offline = false;
    const key = request => typeof request === 'string' ? new URL(request, 'https://example.test').href : request.url;
    const cache = name => {
        if (!stores.has(name)) stores.set(name, new Map());
        const data = stores.get(name);
        return { addAll: async urls => urls.forEach(url => data.set(key(url), new Response('offline-page'))),
            match: async request => data.get(key(request))?.clone(), put: async (request,response) => data.set(key(request), response) };
    };
    const self = { location: { origin: 'https://example.test' }, clients: { claim: async () => {} },
        addEventListener: (name,handler) => handlers.set(name, handler), skipWaiting: async () => { skipped++; } };
    vm.runInNewContext(source, { self, URL, Response, caches: { open: async name => cache(name), keys: async () => [...stores.keys()], delete: async name => stores.delete(name) },
        fetch: async request => { if (offline) throw new TypeError('offline'); const response = new Response('network', { headers: { 'Content-Type': request.url.endsWith('.html') ? 'text/html' : 'application/octet-stream', 'Cache-Control': request.private ? 'private' : 'public' } }); Object.defineProperty(response,'type',{value:'basic'}); return response; } });
    async function dispatch(name, properties = {}) {
        let response;
        const pending = [];
        handlers.get(name)({ ...properties, waitUntil: promise => pending.push(promise), respondWith: promise => { response = promise; } });
        await Promise.all(pending);
        return response ? await response : undefined;
    }
    return { stores, cache, dispatch, get skipped() { return skipped; }, goOffline: () => { offline = true; } };
}
const request = (path, extra = {}) => ({ method: 'GET', mode: 'cors', url: new URL(path, 'https://example.test').href, ...extra });
test('installation never activates a waiting worker automatically', async () => { const app = worker(); await app.dispatch('install'); assert.equal(app.skipped,0); await app.dispatch('message',{data:{action:'ignored'}}); assert.equal(app.skipped,0); await app.dispatch('message',{data:{action:'activateUpdate'}}); assert.equal(app.skipped,1); });
test('activation deletes only obsolete application caches', async () => { const app = worker(); app.cache('unrelated-app'); app.cache('Mkx.Templates-static-old'); await app.dispatch('install'); await app.dispatch('activate'); assert.equal(app.stores.has('unrelated-app'),true); assert.equal(app.stores.has('Mkx.Templates-static-old'),false); assert.equal(app.stores.has('Mkx.Templates-static-test-build'),true); });
test('API, account, writes, logs and cross-origin requests bypass caching', async () => {
    const app = worker();
    for (const value of [request('/api/tests'),request('/api/Account/auth-state'),request('/Account/Login'),request('/logs'),request('/_framework/app.wasm',{method:'POST'}),request('https://elsewhere.test/app.wasm')]) assert.equal(await app.dispatch('fetch',{request:value}),undefined);
    assert.equal(app.stores.size,0);
});
test('only public non-HTML static responses are cached and writes are awaited', async () => {
    const app = worker(); await app.dispatch('fetch',{request:request('/_framework/app.wasm')});
    const stored = app.stores.get('Mkx.Templates-static-test-build'); assert.equal(stored.size,1);
    await app.dispatch('fetch',{request:request('/assets/private.json',{private:true})});
    await app.dispatch('fetch',{request:request('/assets/user.html')});
    assert.equal(stored.size,1);
    assert.equal(await app.dispatch('fetch',{request:request('/assets/file.js?token=secret')}),undefined);
});
test('offline navigation uses public fallback and never a saved authenticated shell', async () => {
    const app = worker(); await app.dispatch('install'); await app.dispatch('fetch',{request:request('/users',{mode:'navigate'})});
    assert.equal(app.stores.get('Mkx.Templates-static-test-build').has('https://example.test/users'),false);
    app.goOffline(); const response = await app.dispatch('fetch',{request:request('/users',{mode:'navigate'})}); assert.equal(await response.text(),'offline-page');
});
test('helper waits for approval, protects dirty forms, and reloads only once', async () => {
    const helper = readFileSync(new URL('../../src/Server/Mkx.Templates.Server/wwwroot/js/pwa-helper.js',import.meta.url),'utf8');
    const handlers = new Map(), loadHandlers = [], panels = [], messages = [];
    let reloads = 0, dirty = true, confirmed = false;
    const waiting = { postMessage: message => messages.push(message) };
    const createElement = () => ({ children: [], append(...children) { this.children.push(...children); }, setAttribute() {}, remove() { this.removed = true; } });
    const document = { addEventListener() {}, querySelector: () => dirty ? {} : null,
        getElementById: id => panels.find(panel => panel.id === id && !panel.removed), createElement,
        body: { append(panel) { panels.push(panel); } } };
    vm.runInNewContext(helper,{ document, confirm: () => confirmed,
        navigator:{userAgent:'test',serviceWorker:{addEventListener:(name,action)=>handlers.set(name,action),register:async()=>({waiting,addEventListener(){}})}},
        window:{addEventListener:(name,action)=>{if(name==='load')loadHandlers.push(action);}}, location:{reload:()=>reloads++},
        sessionStorage:{getItem:()=>null,setItem(){}}, matchMedia:()=>({matches:false}) });
    handlers.get('controllerchange')(); assert.equal(reloads,0);
    await loadHandlers[0]();
    const accept = panels[0].children[1];
    await accept.onclick(); assert.equal(messages.length,0); assert.equal(panels[0].removed,undefined);
    confirmed = true; await accept.onclick(); assert.equal(messages[0].action,'activateUpdate');
    handlers.get('controllerchange')(); handlers.get('controllerchange')(); assert.equal(reloads,1);
});
