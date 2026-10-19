import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';

const script = readFileSync(new URL('../../src/Server/Mkx.Templates.Server/wwwroot/js/app.js', import.meta.url), 'utf8');
const button = { getBoundingClientRect: () => ({ left: 100, top: 20, width: 40, height: 40 }) };

function browser({ reduced = false, supported = true, saved = null, palette = null, storageBlocked = false, rejectSnapshot = false } = {}) {
    const animations = [];
    let snapshots = 0;
    const root = {
        dataset: {}, style: {},
        animate: (frames, options) => {
            const animation = { frames, options, cancelled: false };
            animations.push(animation);
            return { finished: Promise.resolve(), cancel: () => { animation.cancelled = true; } };
        },
    };
    const document = { documentElement: root, querySelector: () => null };
    if (supported) document.startViewTransition = update => {
        snapshots++;
        update();
        return { ready: rejectSnapshot ? Promise.reject(new Error('hidden document')) : Promise.resolve(),
            finished: Promise.resolve(), skipTransition() {} };
    };
    const sandbox = { document, innerWidth: 1440, innerHeight: 1000, setTimeout, clearTimeout,
        localStorage: { getItem(key) { if (storageBlocked) throw new Error('blocked'); return key === 'Theme.Palette' ? palette : saved; } },
        matchMedia: query => ({ matches: query.includes('reduced-motion') && reduced }),
    };
    sandbox.window = sandbox;
    vm.runInNewContext(script, sandbox);
    return { app: sandbox.Mkx, root, animations, document, snapshots: () => snapshots };
}

test('dark expands from the button and light reverses the dark snapshot back into it', async () => {
    const state = browser();
    for (const dark of [true, false]) {
        await state.app.setAppearance(dark, button);
        assert.equal(state.root.dataset.theme, dark ? 'dark' : 'light');
        assert.equal(state.root.style.colorScheme, dark ? 'dark' : 'light');
        assert.equal(state.root.dataset.themeTransition, undefined);
    }
    assert.equal(state.snapshots(), 2);
    const radius = Math.ceil(Math.hypot(1440 - 120, 1000 - 40));
    const [dark, light] = state.animations;
    assert.equal(dark.frames.clipPath[0], 'circle(0px at 120px 40px)');
    assert.equal(dark.frames.clipPath[1], `circle(${radius}px at 120px 40px)`);
    assert.deepEqual([...light.frames.clipPath], [...dark.frames.clipPath].reverse());
    assert.equal(dark.options.pseudoElement, '::view-transition-new(root)');
    assert.equal(light.options.pseudoElement, '::view-transition-old(root)');
    assert.equal(dark.options.duration, 800);
    assert.equal(light.options.duration, 800);
    assert.equal(light.options.easing, 'cubic-bezier(.64,0,.78,0)');
});

test('reduced motion, unsupported browsers and system changes apply without snapshots', async () => {
    for (const options of [{ reduced: true }, { supported: false }, {}]) {
        const state = browser(options);
        await state.app.setAppearance(true, Object.keys(options).length ? button : null);
        assert.equal(state.root.dataset.theme, 'dark');
        assert.equal(state.snapshots(), 0);
        assert.equal(state.animations.length, 0);
    }
});

test('the final dark clip is retained through delayed snapshot removal and then released', async () => {
    const state = browser({ saved: 'True' });
    let removeSnapshots;
    state.document.startViewTransition = update => {
        update();
        return { ready: Promise.resolve(), finished: new Promise(resolve => { removeSnapshots = resolve; }), skipTransition() {} };
    };
    const pending = state.app.setAppearance(false, button);
    await new Promise(setImmediate);
    const animation = state.animations[0];
    assert.equal(animation.options.fill, 'both');
    assert.equal(animation.cancelled, false, 'The clip must survive after the reveal animation finishes');
    assert.equal(state.root.dataset.themeTransition, 'radial');
    removeSnapshots();
    await pending;
    assert.equal(animation.cancelled, true, 'Release the retained animation after removing its snapshot');
    assert.equal(state.root.dataset.themeTransition, undefined);
});

test('saved appearance restores without animation and blocked storage falls back to the system', () => {
    assert.equal(browser({ saved: 'True' }).root.dataset.theme, 'dark');
    assert.equal(browser({ storageBlocked: true }).root.dataset.theme, 'light');
});

test('palette restoration validates storage and remains independent of light/dark preference', async () => {
    const state = browser({ saved: 'True', palette: 'iris' });
    assert.equal(state.root.dataset.palette, 'iris');
    assert.equal(state.root.dataset.theme, 'dark');
    assert.equal(state.root.dataset.paletteTransition, undefined);
    await state.app.setAppearance(false);
    assert.equal(state.root.dataset.palette, 'iris');
    assert.equal(browser({ palette: 'invalid' }).root.dataset.palette, 'jade');
    assert.equal(browser({ storageBlocked: true }).root.dataset.palette, 'jade');
});

test('palette changes reveal the new snapshot in both modes and reduced motion applies immediately', async () => {
    for (const saved of ['True', 'False']) {
        const state = browser({ saved });
        await state.app.setPalette('ocean', true, button);
        assert.equal(state.root.dataset.palette, 'ocean');
        assert.equal(state.animations[0].options.pseudoElement, '::view-transition-new(root)');
        assert.equal(state.animations[0].frames.clipPath[0], 'circle(0px at 120px 40px)');
        assert.equal(state.animations[0].options.fill, 'both');
        assert.equal(state.root.dataset.themeTransition, undefined);
    }
    for (const options of [{ reduced: true }, { supported: false }, { rejectSnapshot: true }]) {
        const state = browser(options);
        await state.app.setPalette('iris', true, button);
        assert.equal(state.root.dataset.palette, 'iris');
        assert.equal(state.root.dataset.themeTransition, undefined);
    }
});

test('a newer palette interrupts an older reveal without reverting the latest selection', async () => {
    const state = browser();
    let cancel;
    state.root.animate = () => ({ finished: new Promise((_, reject) => { cancel = reject; }), cancel() {} });
    state.document.startViewTransition = update => {
        update();
        return { ready: Promise.resolve(), finished: Promise.resolve(), skipTransition: () => cancel?.(new Error('interrupted')) };
    };
    const pending = state.app.setPalette('ocean', true, button);
    await Promise.resolve();
    await state.app.setPalette('amber', false);
    await pending;
    assert.equal(state.root.dataset.palette, 'amber');
    assert.equal(state.root.dataset.themeTransition, undefined);
});

test('a rejected snapshot still applies the theme and clears transition state', async () => {
    const state = browser({ rejectSnapshot: true });
    await state.app.setAppearance(true, button);
    assert.equal(state.root.dataset.theme, 'dark');
    assert.equal(state.root.dataset.themeTransition, undefined);
});

test('interrupting an active reveal cannot restore an older theme', async () => {
    const state = browser();
    let cancel;
    state.root.animate = () => ({ finished: new Promise((_, reject) => { cancel = reject; }), cancel() {} });
    state.document.startViewTransition = update => {
        update();
        return { ready: Promise.resolve(), finished: Promise.resolve(), skipTransition: () => cancel?.(new Error('interrupted')) };
    };
    const pending = state.app.setAppearance(true, button);
    await Promise.resolve();
    await state.app.setAppearance(false);
    await pending;
    assert.equal(state.root.dataset.theme, 'light');
    assert.equal(state.root.dataset.themeTransition, undefined);
});
