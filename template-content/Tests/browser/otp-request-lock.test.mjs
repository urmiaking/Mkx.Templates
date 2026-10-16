import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';

const root = new URL('../../', import.meta.url);
const source = path => readFileSync(new URL(path, root), 'utf8');

test('OTP request paints pending state before one actual native POST, preserving submitter', () => {
    const script = source('src/Server/Mkx.Templates.Server/wwwroot/js/login-otp.js');
    const formEvents = new Map(), windowEvents = new Map(), deferred = [];
    const classes = new Set(), attributes = new Map(), buttonAttributes = new Map();
    const label = { textContent: 'ارسال کد ورود' };
    let nativeSubmits = 0;
    let prevented = 0;
    const form = {
        isConnected: true, dataset: {},
        addEventListener: (name, handler) => formEvents.set(name, handler),
        setAttribute: (name, value) => attributes.set(name, value),
        removeAttribute: name => attributes.delete(name),
        requestSubmit: submitter => {
            assert.equal(submitter, button, 'native POST must include Input.Action=request');
            assert.equal(submitter.disabled, false, 'submitter stays enabled during serialization');
            nativeSubmits++;
            submit();
        }
    };
    const button = {
        form, dataset: {}, disabled: false, isConnected: true,
        setAttribute: (name, value) => buttonAttributes.set(name, value),
        removeAttribute: name => buttonAttributes.delete(name),
        querySelector: selector => selector === '[data-otp-request-label]' ? label : null,
        classList: {
            add: name => classes.add(name),
            remove: name => classes.delete(name)
        }
    };
    const document = {
        readyState: 'complete',
        querySelectorAll: () => [],
        querySelector: selector => selector === '[data-otp-request]' ? button : null
    };
    const submit = () => formEvents.get('submit')({
        submitter: button,
        preventDefault() { prevented++; },
        stopImmediatePropagation() {}
    });
    vm.runInNewContext(script, {
        document, window: { addEventListener: (name, fn) => windowEvents.set(name, fn) },
        navigator: {}, setTimeout: fn => deferred.push(fn), clearTimeout() {}
    });

    submit();
    assert.equal(prevented, 1, 'First submission is delayed to allow the spinner to paint');
    assert.equal(nativeSubmits, 0);
    assert.equal(form.dataset.otpRequestPending, 'true');
    assert.equal(attributes.get('aria-busy'), 'true');
    assert.equal(buttonAttributes.get('aria-disabled'), 'true');
    assert.equal(label.textContent, 'در حال ارسال کد…');
    assert.equal(classes.has('is-pending'), true);
    assert.equal(button.disabled, false, 'Input.Action must remain enabled until real submission');
    submit();
    assert.equal(prevented, 2, 'Repeated click must be suppressed');
    deferred.shift()();
    assert.equal(nativeSubmits, 1, 'Exactly one native POST resumes after painting');
    assert.equal(prevented, 2, 'Resumed native POST must not be prevented');
    deferred.shift()();
    assert.equal(button.disabled, true);
    windowEvents.get('pageshow')({ persisted: true });
    assert.equal(button.disabled, false);
    assert.equal(buttonAttributes.has('aria-disabled'), false);
    assert.equal(label.textContent, 'ارسال کد ورود');
    assert.equal(form.dataset.otpRequestPending, undefined);
    submit();
    assert.equal(prevented, 3, 'Form is usable again after browser back navigation');
});

test('OTP request button has proper labels, spinner and native fallback', () => {
    const page = source('src/Server/Mkx.Templates.Server/Pages/ForgotPassword.razor');
    const app = source('src/Server/Mkx.Templates.Server/Components/App.razor');
    const css = source('src/Server/Mkx.Templates.Server/wwwroot/css/login-otp.css');
    assert.match(page, /name="Input.Action" value="request"\s+data-otp-request/);
    assert.match(page, /data-otp-request-label>ارسال کد بازیابی/);
    assert.match(page, /class="sms-request-spinner" aria-hidden="true"/);
    assert.match(css, /\.sms-login-primary\.is-pending/);
    assert.match(css, /@keyframes sms-request-spin/);
    assert.match(app, /<script src="@Assets\["js\/login-otp\.js"\]"><\/script>/,
        'global script must survive enhanced navigation to the OTP page');
    assert.doesNotMatch(page, /src="\/js\/login-otp\.js"/,
        'page must not load a duplicate script after enhanced navigation');
});

test('OTP paste normalizes localized digits, fills six decorative cells and submits once', () => {
    const listeners = new Map();
    const boxes = Array.from({length: 6}, () => ({textContent: '', classList: {toggle() {}}}));
    let posts = 0, preventedPaste = false;
    const submitter = {value: 'verify', dataset: {}};
    const form = {
        querySelector: () => submitter,
        addEventListener() {},
        requestSubmit(button) { assert.equal(button, submitter); posts++; }
    };
    const input = {
        value: '', form,
        addEventListener: (event, handler) => listeners.set(event, handler),
        dispatchEvent: event => listeners.get(event.type)?.(event),
        focus() {}
    };
    const group = {
        dataset: {otpAutoSubmit: 'true', otpSms: 'false'},
        querySelector: () => input,
        querySelectorAll: () => boxes,
        addEventListener() {}
    };
    const window = {};
    vm.runInNewContext(source('src/Server/Mkx.Templates.Server/wwwroot/js/login-otp.js'), {
        window, navigator: {}, Event,
        document: {readyState: 'complete', querySelectorAll: () => [group], querySelector: () => null}
    });
    window.Mkx.otp.initialize(); // Enhanced navigation must not attach handlers twice.
    listeners.get('paste')({
        clipboardData: {getData: () => '۰١ ۲-٣۴٥'},
        preventDefault() { preventedPaste = true; }
    });
    assert(preventedPaste);
    assert.equal(input.value, '012345');
    assert.equal(boxes.map(box => box.textContent).join(''), '012345');
    input.dispatchEvent(new Event('input'));
    assert.equal(posts, 1);
    group.dataset.otpAutoSubmit = 'false';
    input.value = '6'; input.dispatchEvent(new Event('input'));
    input.value = '654321'; input.dispatchEvent(new Event('input'));
    assert.equal(posts, 1, 'Interactive fields own their verification action');
});
