// Optional real-browser UI regression suite. API fixtures are isolated from the database.
// Run from the solution root with a local development server already running.
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { readdirSync, mkdirSync } from 'node:fs';
import path from 'node:path';

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.UI_PLAYWRIGHT_MODULE || 'playwright');
const base = process.env.UI_BASE_URL || 'http://localhost:7007';
const output = process.env.UI_ARTIFACTS;
if (output) mkdirSync(output, { recursive: true });
const client = readdirSync('src/Client').find(name => name.endsWith('.Client'));
const prefix = client.replace(/\.Client$/, '').split('.').at(-1);
const browser = await chromium.launch({
    headless: true,
    ...(process.env.UI_BROWSER_CHANNEL ? { channel: process.env.UI_BROWSER_CHANNEL } : {}),
});

try {
    const context = await browser.newContext({ viewport: { width: 1440, height: 1000 } });
    // This suite checks credential-management UI, not platform authenticator hardware.
    await context.addInitScript(() => Object.defineProperty(window, 'PublicKeyCredential', { value: undefined }));
    const page = await context.newPage();
    page.setDefaultTimeout(10000);
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => {
        if (message.type() === 'error' && !message.text().includes('500')) errors.push(message.text());
    });
    const id = '0195d5ee-0961-7000-9000-000000000001';
    const version = '0195d5ee-0961-7000-9000-000000001001';
    let items = Array.from({ length: 30 }, (_, i) => ({
        id: `0195d5ee-0961-7000-9000-${String(i + 1).padStart(12, '0')}`,
        name: `نمونه ${i + 1}`, description: 'توضیحات نمونه', version,
    }));
    let failSave = false;
    let writes = 0;
    let otpEnables = 0;
    const claim = (type, value) => ({ type, value });
    await context.route('**/api/**', async route => {
        const request = route.request();
        const url = new URL(request.url());
        const pathname = url.pathname;
        const method = request.method();
        let body;
        let status = 200;
        if (pathname.endsWith('/auth-state')) {
            body = { userClaims: [
                claim('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier', id),
                claim('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name', 'ui-review'),
                claim('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname', 'کاربر آزمایشی'),
                claim('http://schemas.microsoft.com/ws/2008/06/identity/claims/role', 'Administrators'),
                ...['Users-View', 'Users-Manage', 'Users-ManageClaims', 'Tests-View', 'Tests-Manage']
                    .map(policy => claim(`${prefix}-${policy}`, '')),
            ] };
        } else if (pathname.endsWith('/antiforgery')) {
            body = { requestToken: 'ui-fixture-token' };
        } else if (pathname === '/api/tests' && method === 'GET') {
            const get = name => url.searchParams.get(name) || url.searchParams.get(name[0].toUpperCase() + name.slice(1));
            const skip = Number(get('skip') || 0), take = Number(get('take') || 25);
            const filtered = items.filter(item => item.name.includes(get('search') || ''));
            body = { data: filtered.slice(skip, skip + take), total: filtered.length, skip, take };
        } else if (pathname.startsWith('/api/tests') && method !== 'GET') {
            writes++;
            if (failSave) {
                status = 500;
                body = { title: 'Fixture save failure', status };
            } else if (method === 'POST') {
                body = { ...request.postDataJSON(), id: '0195d5ee-0961-7000-9000-999999999999', version };
                items.unshift(body);
            } else if (method === 'PUT') {
                body = { ...request.postDataJSON(), id: pathname.split('/').at(-1) };
                items = items.map(item => item.id === body.id ? body : item);
            } else {
                items = items.filter(item => item.id !== pathname.split('/').at(-1));
                status = 204;
            }
        } else if (pathname.endsWith('/send-verification-token')) {
            status = 204;
        } else if (pathname.endsWith('/2fa/enable')) {
            otpEnables++;
            assert.equal(request.postDataJSON().token, '012345');
            body = { recoveryCodes: ['ABCD-1234', 'EFGH-5678'] };
        } else if (pathname.endsWith('/enabled')) {
            body = true;
        } else if (pathname.endsWith('/user-management/users')) {
            body = { data: [{ id, name: 'کاربر نمونه', userName: 'sample', email: 'sample@example.test',
                roles: ['Users'], directClaimsCount: 1 }], total: 1 };
        } else if (pathname.endsWith('/current-user')) {
            body = { id, username: 'ui-review', fullName: 'کاربر آزمایشی', phoneNumber: '09123456789',
                email: 'review@example.test', role: 'Administrators', isActive: true };
        } else if (pathname.endsWith('/2fa')) {
            body = { enabled: false, machineRemembered: false, recoveryCodesLeft: 0 };
        } else if (pathname.endsWith('/external-providers')) {
            body = { currentLogins: [], otherProviders: [], canRemoveLogins: false };
        } else if (pathname.endsWith('/passkeys')) {
            body = [];
        } else if (pathname.endsWith('/roles')) {
            body = [{ roleName: 'Users', memberCount: 1, claimsCount: 0 }];
        } else if (pathname.endsWith('/authenticator-key')) {
            body = { key: 'JBSWY3DPEHPK3PXP' };
        } else if (pathname.endsWith('/claims')) {
            body = [{ name: 'Category_Users', description: 'کاربران', isGranted: false,
                children: [{ name: `${prefix}-Users-View`, description: 'نمایش کاربران', isGranted: false, children: [] }] }];
        } else if (pathname.endsWith('/user-accounts/list')) {
            body = { data: [], total: 0 };
        } else {
            throw new Error(`Unexpected UI fixture request: ${method} ${pathname}`);
        }
        await route.fulfill({ status, contentType: status >= 400 ? 'application/problem+json' : 'application/json',
            body: status === 204 ? '' : JSON.stringify(body) });
    });

    const screenshot = async name => {
        if (output) {
            await page.waitForTimeout(350); // Capture the settled reference transitions.
            await page.screenshot({ path: path.join(output, `${name}.png`) });
        }
    };
    const navigate = async pathname => {
        if (page.url() === 'about:blank') await page.goto(base);
        else {
            // Client routing keeps the fixture auth separate from real server authentication.
            await page.locator('body').evaluate((body, href) => {
                const link = document.createElement('a');
                link.href = href; body.append(link); link.click(); link.remove();
            }, pathname);
        }
        await page.locator('#main-content').waitFor();
        await page.waitForTimeout(400);
        assert.equal(await page.locator('#blazor-error-ui').isVisible(), false, pathname);
    };

    await navigate('/');
    assert.equal(await page.locator('.workspace-sidebar').evaluate(node => node.getBoundingClientRect().width), 238);
    assert(await page.locator('.workspace-sidebar').getByRole('button', { name: 'خروج از حساب', exact: true }).isEnabled());
    assert.equal(await page.getByRole('button', { name: 'باز کردن منوی اصلی' }).isVisible(), false);
    await screenshot('home-desktop');
    await page.getByRole('button', { name: 'اعلان‌ها', exact: true }).click();
    const notifications = page.getByRole('dialog', { name: 'اعلان‌ها' });
    await notifications.waitFor();
    assert(await notifications.evaluate(node => node.matches(':modal')));
    const notificationBounds = await notifications.boundingBox();
    assert.equal(notificationBounds.width, 420);
    assert.equal(notificationBounds.height, 1000);
    assert.equal(notificationBounds.x, 0);
    await screenshot('notifications-desktop');
    await page.keyboard.press('Escape');
    await notifications.waitFor({ state: 'detached' });
    await page.locator('.workspace-create').click();
    await page.getByRole('dialog', { name: 'افزودن تست' }).waitFor();
    await page.getByRole('button', { name: 'انصراف', exact: true }).click();
    await page.getByRole('dialog').waitFor({ state: 'detached' });

    await navigate('/tests');
    await page.getByRole('button', { name: 'افزودن', exact: true }).click();
    const editor = page.getByRole('dialog', { name: 'افزودن تست' });
    await editor.waitFor();
    assert.equal(await editor.getByLabel('نام', { exact: false }).first().inputValue(), '');
    await editor.getByRole('button', { name: 'ذخیره', exact: true }).click();
    await editor.locator('[role=alert]').first().waitFor();
    assert.equal(writes, 0);
    await editor.getByLabel('نام', { exact: false }).first().fill('نمونه جدید');
    await editor.getByRole('button', { name: 'انصراف', exact: true }).click();
    await page.getByRole('dialog', { name: 'تغییرات ذخیره نشده' }).waitFor();
    await page.getByRole('button', { name: 'ادامه ویرایش' }).click();
    assert.equal(await page.getByRole('dialog').count(), 1);
    failSave = true;
    await editor.getByRole('button', { name: 'ذخیره', exact: true }).click();
    await editor.getByText('خطایی در سرور رخ داده است. دوباره تلاش کنید.').waitFor();
    assert.equal(writes, 1);
    assert.equal(await editor.getByLabel('نام', { exact: false }).first().inputValue(), 'نمونه جدید');
    failSave = false;
    await editor.getByRole('button', { name: 'ذخیره', exact: true }).click();
    await editor.waitFor({ state: 'detached' });
    assert.equal(writes, 2);
    await page.getByRole('button', { name: 'بعدی', exact: true }).click();
    await page.getByText('صفحه 2', { exact: false }).waitFor();
    await page.getByLabel('جستجوی نام یا توضیحات').fill('نمونه جدید');
    await page.getByText('صفحه 1', { exact: false }).waitFor();
    await page.getByText('نمونه جدید', { exact: true }).waitFor();

    await navigate('/users');
    await page.getByRole('button', { name: 'افزودن کاربر', exact: true }).click();
    const userEditor = page.getByRole('dialog', { name: 'افزودن کاربر جدید' });
    await userEditor.getByRole('button', { name: 'ذخیره', exact: true }).click();
    await userEditor.locator('[role=alert]').first().waitFor();
    await userEditor.getByLabel('نام و نام خانوادگی').fill('نام آزمایشی');
    await userEditor.getByRole('button', { name: 'انصراف', exact: true }).click();
    await page.getByRole('button', { name: 'کنار گذاشتن', exact: true }).click();
    await userEditor.waitFor({ state: 'detached' });

    await navigate('/user-accounts/list');
    await page.getByRole('button', { name: 'کاربر جدید', exact: true }).click();
    const accountEditor = page.getByRole('dialog', { name: 'حساب کاربری جدید' });
    await accountEditor.getByRole('button', { name: 'ذخیره', exact: true }).click();
    await accountEditor.locator('[role=alert]').first().waitFor();
    await accountEditor.getByRole('combobox').selectOption('Users');
    await accountEditor.getByRole('button', { name: 'انصراف', exact: true }).click();
    await accountEditor.waitFor({ state: 'detached' });
    await page.getByRole('button', { name: 'بستن پیام', exact: true }).evaluateAll(buttons => buttons.forEach(button => button.click()));

    for (const pathname of ['/users', '/users/role-claims', '/user-accounts',
        '/user-accounts/two-factor-authentication', '/user-accounts/enable-authenticator',
        '/user-accounts/passkeys', '/user-accounts/external-logins', '/user-accounts/list']) {
        await navigate(pathname);
        await screenshot(pathname.replaceAll('/', '-'));
    }
    await navigate('/user-accounts');
    await page.getByRole('button', { name: 'تغییر شماره همراه' }).click();
    const phoneEditor = page.getByRole('dialog', { name: 'ویرایش شماره تلفن' });
    await phoneEditor.waitFor();
    await phoneEditor.getByLabel('شماره همراه جدید').fill('09121112222');
    await phoneEditor.getByRole('button', { name: 'دریافت کد تأیید' }).click();
    const phoneOtp = phoneEditor.locator('[data-otp-real]');
    await phoneOtp.waitFor();
    await phoneOtp.fill('۱۲۳۴۵۶');
    await page.waitForFunction(() => document.querySelector('[data-otp-real]')?.value === '123456');
    assert.equal(await phoneEditor.locator('.sms-otp-digit.filled').count(), 6);
    assert(await phoneEditor.getByRole('button', { name: 'ذخیره شماره' }).isEnabled());
    assert(await phoneEditor.getByRole('button', { name: /ارسال مجدد در/ }).isDisabled());
    await screenshot('phone-otp-desktop');
    await phoneEditor.getByRole('button', { name: 'انصراف', exact: true }).click();
    await phoneEditor.waitFor({ state: 'detached' });
    await navigate('/user-accounts/enable-authenticator');
    const authenticatorOtp = page.locator('[data-otp-real]');
    await authenticatorOtp.waitFor({ state: 'visible' });
    await authenticatorOtp.fill('٠١٢٣٤٥');
    await page.getByRole('dialog', { name: 'کدهای بازیابی' }).waitFor();
    assert.equal(otpEnables, 1, 'Completing an OTP enables the authenticator once');
    await screenshot('authenticator-recovery');
    await page.getByRole('dialog').getByRole('button', { name: /تایید|تأیید|بستن|ذخیره|متوجه/ }).click();
    await page.waitForURL('**/user-accounts/two-factor-authentication');
    await navigate('/');
    await page.emulateMedia({ colorScheme: 'dark' });
    await page.waitForFunction(() => document.documentElement.dataset.theme === 'dark');
    await page.emulateMedia({ colorScheme: 'light' });
    await page.waitForFunction(() => document.documentElement.dataset.theme === 'light');
    await page.getByRole('button', { name: 'فعال‌سازی حالت تاریک' }).click();
    assert.equal(await page.locator('html').getAttribute('data-theme'), 'dark');
    await page.mouse.move(700, 80);
    await page.waitForTimeout(350);
    assert.equal(await page.locator('.workspace-create').evaluate(node => getComputedStyle(node).backgroundColor), 'rgb(17, 116, 98)');
    assert.equal(await page.locator('.workspace-topbar-actions > button').first().evaluate(node => getComputedStyle(node).backgroundColor), 'rgb(25, 39, 46)');
    await screenshot('home-dark');
    await page.setViewportSize({ width: 390, height: 844 });
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await page.goto(base);
    await page.locator('h1').waitFor();
    assert.equal(await page.locator('.workspace-main').evaluate(node => getComputedStyle(node).marginInlineStart), '0px');
    assert(await page.locator('.workspace-bottom-nav').isVisible());
    assert.equal(await page.locator('.workspace-main > section').evaluate(node => getComputedStyle(node).animationName), 'none');
    assert.equal(await page.evaluate(() => document.documentElement.scrollWidth > innerWidth), false);
    await screenshot('home-mobile-dark');
    await page.getByRole('button', { name: 'باز کردن منوی اصلی' }).click();
    await page.locator('.workspace-sidebar.mobile-open').waitFor();
    assert(await page.locator('.workspace-main').evaluate(node => node.inert));
    await page.keyboard.press('Escape');
    await page.waitForFunction(() => !document.querySelector('.workspace-main').inert);

    assert.equal(await page.locator('.workspace-create-circle').evaluate(node => Math.round(node.getBoundingClientRect().width)), 62);
    await page.getByRole('button', { name: 'اعلان‌ها', exact: true }).click();
    await notifications.waitFor();
    assert.equal((await notifications.boundingBox()).width, 390);
    assert.equal(Math.round((await notifications.boundingBox()).x), 0);
    assert.equal(await notifications.evaluate(node => node.scrollWidth > node.clientWidth), false);
    await screenshot('notifications-mobile');
    await page.keyboard.press('Escape');
    await notifications.waitFor({ state: 'detached' });
    await navigate('/user-accounts/enable-authenticator');
    await screenshot('authenticator-mobile');
    assert.equal(await page.evaluate(() => document.documentElement.scrollWidth > innerWidth), false);

    // Static SSR pages use real server-rendered form markup; no sign-in is submitted.
    await page.goto(`${base}/Account/Login`);
    assert.equal(await page.getByLabel('نام کاربری / شماره تلفن همراه').getAttribute('name'), 'Input.Username');
    assert.equal(await page.getByLabel('رمز عبور', { exact: true }).getAttribute('name'), 'Input.Password');
    assert.equal(await page.locator('input[name="Input.RememberMe"][type=checkbox]').count(), 1);
    await screenshot('login-mobile');
    await page.goto(`${base}/Account/ForgotPassword`);
    assert.equal(await page.getByLabel('شماره تلفن همراه').getAttribute('name'), 'Input.PhoneNumber');
    assert.deepEqual(errors, []);
    console.log('Native UI browser checks passed (API fixture data; no database mutations).');
} finally {
    await browser.close();
}
