import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import vm from 'node:vm';

const source = readFileSync(new URL('../../src/Server/Mkx.Templates.Server/wwwroot/js/app-startup.js', import.meta.url), 'utf8');

function setup({ initial = '', reduced = false, hasSplash = true } = {}) {
    let reported = initial;
    let observing = false;
    let update;
    let enhanced;
    let timer;
    const progress = { value: 0 };
    const percentage = { textContent: '۰٪' };
    const phase = { textContent: 'در حال بارگیری فایل‌های برنامه' };
    const classes = new Set();
    const element = {
        querySelector: selector => ({ '#splash-progress': progress, '#splash-percentage': percentage, '#splash-phase': phase })[selector],
        classList: { contains: c => classes.has(c), add: c => classes.add(c) },
        remove: () => { splash = null; }
    };
    let splash = hasSplash ? element : null;
    const root = { style: { getPropertyValue: key => { assert.equal(key, '--blazor-load-percentage'); return reported; } } };
    const window = {
        Blazor: { addEventListener: (name, fn) => { assert.equal(name, 'enhancedload'); enhanced = fn; } },
        matchMedia: () => ({ matches: reduced }),
        setTimeout: fn => { timer = fn; }
    };
    vm.runInNewContext(source, {
        window, document: { documentElement: root, getElementById: () => splash },
        MutationObserver: class { constructor(fn) { update = fn; } observe() { observing = true; } disconnect() { observing = false; } }
    });
    return { progress, percentage, phase, classes, window,
        set: value => { reported = value; if (observing) update(); },
        isObserving: () => observing, current: () => splash,
        finish: () => timer?.(), insert: () => { splash = element; enhanced(); }
    };
}

test('tracks actual Blazor resource progress and preparation at 100%', () => {
    const s = setup();
    assert.equal(s.progress.value, 0);
    s.set('37.8%');
    assert.equal(s.progress.value, 37.8);
    assert.equal(s.percentage.textContent, new Intl.NumberFormat('fa-IR', { style: 'percent' }).format(.37));
    s.set('invalid');
    assert.equal(s.progress.value, 37.8);
    s.set('100%');
    assert.equal(s.progress.value, 100);
    assert.match(s.phase.textContent, /آماده/);
    assert.ok(s.current());
    s.window.Mkx.removeSplash();
    assert.equal(s.isObserving(), false);
    assert.ok(s.classes.has('fade-out'));
    s.finish();
    assert.equal(s.current(), null);
});

test('handles already reported progress, SSR navigation and reduced motion', () => {
    const s = setup({ initial: '65%', reduced: true, hasSplash: false });
    assert.equal(s.isObserving(), false);
    s.window.Mkx.removeSplash();
    s.insert();
    assert.equal(s.progress.value, 65);
    s.window.Mkx.removeSplash();
    assert.equal(s.progress.value, 100);
    assert.equal(s.current(), null);
    assert.equal(s.isObserving(), false);
});
