import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import vm from 'node:vm';

const root = new URL('../../', import.meta.url);
const source = path => readFileSync(new URL(path, root), 'utf8');
const path = 'src/Server/Mkx.Templates.Server/';

test('refresh is not a submitter and sends only the challenge id', async () => {
    const handlers = {};
    const password = { value: 'typed password' };
    const username = { value: 'typed user' };
    const id = { value: 'old' };
    const image = { src: 'old image' };
    const answer = { value: '12345', focus() {} };
    const error = { hidden: true, textContent: '' };
    const panel = { querySelector: key => ({ '[name="Input.CaptchaId"]': id, img: image,
        '[name="Input.CaptchaAnswer"]': answer, '[data-login-captcha-error]': error })[key] };
    const form = { querySelector: key => ({ '[name="__RequestVerificationToken"]': { value: 'token' },
        '[name="Input.Username"]': username, '[name="Input.Password"]': password })[key] };
    const button = { disabled: false, isConnected: true, dataset: { loginCaptchaRefresh: '/refresh' },
        closest: key => key === 'form' ? form : panel };
    let reject = false;
    vm.runInNewContext(source(path + 'wwwroot/js/login-captcha.js'), {
        document: { addEventListener: (name, fn) => { handlers[name] = fn; } },
        URLSearchParams,
        fetch: async (url, options) => {
            assert.equal(url, '/refresh');
            assert.equal(options.method, 'POST');
            assert.equal(options.headers['X-CSRF-TOKEN'], 'token');
            assert.equal(options.body.toString(), 'id=' + id.value);
            return { ok: !reject, json: async () => ({ id: 'new', image: 'data:image/png;base64,abc' }) };
        }
    });
    await handlers.click({ target: { closest: () => button } });
    assert.equal(id.value, 'new');
    assert.equal(image.src, 'data:image/png;base64,abc');
    assert.equal(answer.value, '');
    assert.equal(password.value, 'typed password');
    assert.equal(username.value, 'typed user');
    reject = true;
    await handlers.click({ target: { closest: () => button } });
    assert.equal(error.hidden, false);
    assert.equal(id.value, 'new');
    assert.equal(button.disabled, false);
    assert.equal(password.value, 'typed password');
    const page = source(path + 'Pages/Login.razor');
    assert.match(page, /type="button" data-login-captcha-refresh=/);
    assert.doesNotMatch(page, /اعتبار ۵ دقیقه/);
    assert.match(page, /role="alert" data-login-error/);
});
