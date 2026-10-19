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
    await page.clock.install();
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
    let authRequests = 0, holdAuthRefresh = false, releaseAuthRefresh;
    let failRoleSave = false, roleWrites = 0;
    let savedRoleClaims = [`${prefix}-Users-View`];
    const claim = (type, value) => ({ type, value });
    await context.route('**/api/**', async route => {
        const request = route.request();
        const url = new URL(request.url());
        const pathname = url.pathname;
        const method = request.method();
        let body;
        let status = 200;
        if (pathname.endsWith('/auth-state')) {
            authRequests++;
            if (holdAuthRefresh && authRequests > 1) await new Promise(resolve => { releaseAuthRefresh = resolve; });
            body = { userClaims: [
                claim('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier', id),
                claim('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name', 'ui-review'),
                claim('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname', 'کاربر آزمایشی'),
                claim('http://schemas.microsoft.com/ws/2008/06/identity/claims/role', 'Administrators'),
                ...['Users-View', 'Users-Manage', 'Users-ManageClaims', 'SampleItems-View', 'SampleItems-Manage']
                    .map(policy => claim(`${prefix}-${policy}`, '')),
            ] };
        } else if (pathname.endsWith('/antiforgery')) {
            body = { requestToken: 'ui-fixture-token' };
        } else if (pathname === '/api/sample-items' && method === 'GET') {
            const get = name => url.searchParams.get(name) || url.searchParams.get(name[0].toUpperCase() + name.slice(1));
            const skip = Number(get('skip') || 0), take = Number(get('take') || 25);
            const filtered = items.filter(item => item.name.includes(get('search') || ''));
            body = { data: filtered.slice(skip, skip + take), total: filtered.length, skip, take };
        } else if (pathname.startsWith('/api/sample-items') && method !== 'GET') {
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
            body = [{ roleName: 'Administrators', memberCount: 2, claimsCount: 3 },
                { roleName: 'Users', memberCount: 12, claimsCount: savedRoleClaims.length }];
        } else if (pathname.endsWith('/authenticator-key')) {
            body = { key: 'JBSWY3DPEHPK3PXP' };
        } else if (pathname.endsWith('/claims') && method === 'PUT') {
            roleWrites++;
            status = failRoleSave ? 500 : 200;
            if (!failRoleSave) savedRoleClaims = request.postDataJSON();
            body = failRoleSave ? { title: 'Fixture role save failure', status } : true;
        } else if (pathname.endsWith('/claims')) {
            const permission = (suffix, description, children = []) => ({ name: `${prefix}-Users-${suffix}`,
                description, isGranted: savedRoleClaims.includes(`${prefix}-Users-${suffix}`), children });
            body = [{ name: 'Category_Users', description: 'کاربران', isGranted: false,
                children: [permission('View', 'نمایش کاربران', [permission('Manage', 'مدیریت کاربران'),
                    permission('ManageClaims', 'تنظیم دسترسی‌ها')])] }];
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
    await page.locator('.getting-started summary').click();
    await page.locator('.home-layout').evaluate(node => { node.dataset.refreshMarker = 'preserved'; });
    holdAuthRefresh = true;
    const refreshRequest = page.waitForRequest(request => request.url().endsWith('/auth-state'));
    await page.clock.fastForward(305000);
    await refreshRequest;
    await page.waitForTimeout(100);
    assert.equal(await page.locator('.home-layout').getAttribute('data-refresh-marker'), 'preserved', 'Polling must not unmount Home while the auth request is pending');
    assert(await page.locator('.getting-started details').evaluate(node => node.open));
    const refreshResponse = page.waitForResponse(response => response.url().endsWith('/auth-state'));
    holdAuthRefresh = false;
    releaseAuthRefresh();
    await refreshResponse;
    await page.waitForTimeout(100);
    assert.equal(await page.locator('.home-layout').getAttribute('data-refresh-marker'), 'preserved', 'Unchanged auth must not recreate Home');
    assert(await page.locator('.getting-started details').evaluate(node => node.open));
    await page.locator('.getting-started summary').click();
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
    const includesSample = await page.locator('.workspace-create').count() > 0;
    if (!includesSample) assert.equal(await page.locator('a[href^="/sample-items"]').count(), 0);
    if (includesSample) {
        await page.locator('.workspace-create').click();
        await page.getByRole('dialog', { name: 'افزودن نمونه' }).waitFor();
        await page.getByRole('button', { name: 'انصراف', exact: true }).click();
        await page.getByRole('dialog').waitFor({ state: 'detached' });

        await navigate('/sample-items');
        await page.getByRole('button', { name: 'افزودن', exact: true }).click();
        const editor = page.getByRole('dialog', { name: 'افزودن نمونه' });
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
    }

    await navigate('/users');
    const bodyMotion = await page.locator('.account-body').evaluate(node => ({
        animation: getComputedStyle(node).animationName,
        tableIncluded: !!node.querySelector('.ui-data-panel'),
    }));
    assert.equal(bodyMotion.animation, 'section-enter', 'Tables enter with the shared page body');
    assert(bodyMotion.tableIncluded);
    await page.evaluate(() => document.fonts.ready);
    assert.equal(await page.locator('.ui-pagination').evaluate(node => getComputedStyle(node).fontFeatureSettings), '"ss01"');
    await page.getByRole('button', { name: 'افزودن کاربر', exact: true }).click();
    const userEditor = page.getByRole('dialog', { name: 'افزودن کاربر جدید' });
    await userEditor.getByRole('button', { name: 'ذخیره', exact: true }).click();
    await userEditor.locator('[role=alert]').first().waitFor();
    await userEditor.getByLabel('نام و نام خانوادگی').fill('نام آزمایشی');
    await userEditor.getByRole('button', { name: 'انصراف', exact: true }).click();
    await page.getByRole('button', { name: 'کنار گذاشتن', exact: true }).click();
    await userEditor.waitFor({ state: 'detached' });

    await page.getByRole('button', { name: 'بستن پیام', exact: true }).evaluateAll(buttons => buttons.forEach(button => button.click()));
    for (const viewport of [{ width: 1440, height: 1000 }, { width: 375, height: 844 }]) {
        await page.setViewportSize(viewport);
        await navigate('/users/role-claims');
        await page.getByRole('button', { name: 'بستن پیام', exact: true }).evaluateAll(buttons => buttons.forEach(button => button.click()));
        await page.locator('.role-row').first().waitFor();
        assert.equal(await page.locator('.role-row').count(), 2);
        assert.equal(await page.locator('.roles-list .ui-panel, .roles-list .ui-data-panel').count(), 0);
        assert.equal(await page.evaluate(() => document.documentElement.scrollWidth > innerWidth), false);
        await screenshot(`roles-${viewport.width}`);
        await page.locator('.role-row').filter({ hasText: 'Users' }).getByRole('button').click();
        const roleDialog = page.getByRole('dialog', { name: 'مدیریت دسترسی‌های نقش: کاربر' });
        await roleDialog.waitFor();
        await roleDialog.evaluate(node => Promise.all(node.getAnimations({ subtree: true }).map(animation => animation.finished.catch(() => {}))));
        const search = roleDialog.getByLabel('جستجوی دسترسی‌ها');
        const searchBounds = await search.boundingBox();
        for (const label of ['انتخاب همه', 'لغو همه']) {
            const buttonBounds = await roleDialog.getByRole('button', { name: label, exact: true }).boundingBox();
            assert(Math.abs(searchBounds.y + searchBounds.height / 2 - buttonBounds.y - buttonBounds.height / 2) < 2, `${viewport.width}px: aligned ${label}`);
        }
        assert.equal(await roleDialog.locator('.ui-panel').count(), 0);
        assert.equal(await roleDialog.evaluate(node => node.scrollWidth > node.clientWidth), false);
        assert(await roleDialog.locator('.claim-choice').evaluateAll(nodes => nodes.every(node => node.getBoundingClientRect().height <= 48)));
        await search.fill('تنظیم دسترسی‌ها');
        await roleDialog.getByRole('checkbox', { name: 'تنظیم دسترسی‌ها', exact: true }).waitFor();
        await roleDialog.getByRole('checkbox', { name: 'مدیریت کاربران', exact: true }).waitFor({ state: 'detached' });
        assert.equal(await roleDialog.getByRole('checkbox', { name: 'مدیریت کاربران', exact: true }).count(), 0);
        await roleDialog.getByRole('button', { name: 'انتخاب همه', exact: true }).click();
        await search.fill('');
        await roleDialog.getByRole('checkbox', { name: 'مدیریت کاربران', exact: true }).waitFor();
        assert.equal(await roleDialog.locator('input[type=checkbox]:checked').count(), 4);
        await roleDialog.getByRole('button', { name: 'لغو همه', exact: true }).click();
        assert.equal(await roleDialog.locator('input[type=checkbox]:checked').count(), 0);
        await roleDialog.getByRole('checkbox', { name: 'تنظیم دسترسی‌ها', exact: true }).check();
        assert.equal(await roleDialog.locator('input[type=checkbox]:checked').count(), 1, 'Individual permissions remain directly editable');
        assert(await roleDialog.locator('.claims-children').count() > 0, 'Nested permissions retain a semantic child list');
        await roleDialog.getByRole('button', { name: 'انتخاب همه', exact: true }).click();
        await screenshot(`role-permissions-${viewport.width}`);
        if (viewport.width === 1440) {
            failRoleSave = true;
            await roleDialog.getByRole('button', { name: 'ذخیره دسترسی‌ها', exact: true }).click();
            await roleDialog.locator('[role=alert]').waitFor();
            assert.equal(await roleDialog.locator('input[type=checkbox]:checked').count(), 4, 'Failed save retains all selections');
            failRoleSave = false;
            await roleDialog.getByRole('button', { name: 'ذخیره دسترسی‌ها', exact: true }).click();
            await roleDialog.waitFor({ state: 'detached' });
            assert.equal(roleWrites, 2);
            assert.equal(savedRoleClaims.length, 3);
            assert(savedRoleClaims.every(name => !name.startsWith('Category_')));
        } else {
            await roleDialog.getByRole('button', { name: 'انصراف', exact: true }).click();
            await roleDialog.waitFor({ state: 'detached' });
        }
    }
    await page.setViewportSize({ width: 1440, height: 1000 });

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
    await page.waitForFunction(() => document.documentElement.dataset.themeTransition === 'radial');
    await page.waitForFunction(() => document.getAnimations().some(animation =>
        animation.effect?.getKeyframes().some(frame => frame.clipPath?.startsWith('circle('))));
    const reveal = await page.evaluate(() => document.getAnimations().map(animation => ({
        pseudo: animation.effect?.pseudoElement,
        frames: animation.effect?.getKeyframes().map(frame => frame.clipPath),
    })).find(animation => animation.pseudo === '::view-transition-new(root)'));
    assert(reveal, 'The theme uses a real circular View Transition');
    assert(reveal.frames[0].startsWith('circle(0px at '));
    await page.waitForFunction(() => !document.documentElement.dataset.themeTransition);
    assert.equal(await page.locator('html').getAttribute('data-theme'), 'dark');
    await page.mouse.move(700, 80);
    await page.waitForTimeout(350);
    if (includesSample) assert.equal(await page.locator('.workspace-create').evaluate(node => getComputedStyle(node).backgroundColor), 'rgb(17, 116, 98)');
    assert.equal(await page.locator('.workspace-topbar-actions > button').first().evaluate(node => getComputedStyle(node).backgroundColor), 'rgb(25, 39, 46)');
    await screenshot('home-dark');
    const lightButton = page.getByRole('button', { name: 'فعال‌سازی حالت روشن' });
    await lightButton.waitFor();
    await lightButton.focus();
    await page.keyboard.press('Enter');
    await page.waitForFunction(() => document.getAnimations().some(animation =>
        animation.effect?.pseudoElement === '::view-transition-old(root)' &&
        animation.effect.getKeyframes().at(-1).clipPath?.startsWith('circle(0px at ')));
    assert.equal(await page.evaluate(() => getComputedStyle(document.documentElement, '::view-transition-old(root)').zIndex), '2');
    const reverseEnd = await page.evaluate(async () => {
        const root = document.documentElement;
        const reveal = document.getAnimations().find(animation => animation.effect?.pseudoElement === '::view-transition-old(root)');
        // Keep the snapshot tree alive beyond the circle, exposing even a one-frame unclipping flash.
        const hold = root.animate({ opacity: [1, 1] }, { duration: 1200, pseudoElement: '::view-transition-group(root)' });
        await reveal.finished;
        const clip = getComputedStyle(root, '::view-transition-old(root)').clipPath;
        await hold.finished;
        return clip;
    });
    assert(reverseEnd.startsWith('circle(0px at '), 'The dark snapshot stays clipped until the transition tree disappears');
    await page.waitForFunction(() => document.documentElement.dataset.theme === 'light' && !document.documentElement.dataset.themeTransition);
    await page.getByRole('button', { name: 'فعال‌سازی حالت تاریک' }).click();
    await page.waitForFunction(() => document.documentElement.dataset.theme === 'dark' && !document.documentElement.dataset.themeTransition);
    await page.emulateMedia({ reducedMotion: 'no-preference' });
    // The edge picker drags with real mouse/touch input and remembers a viewport-relative position.
    await page.getByRole('button', { name: 'بستن پیام', exact: true }).evaluateAll(buttons => buttons.forEach(button => button.click()));
    await page.setViewportSize({ width: 1440, height: 1000 });
    await navigate('/user-accounts');
    const edgePalette = page.getByRole('button', { name: 'انتخاب رنگ از کنار صفحه', exact: true });
    const gallery = page.getByRole('dialog', { name: 'رنگ فضای کار', exact: true });
    assert.equal(await page.locator('.palette-picker-toolbar').count(), 0);
    const original = await edgePalette.boundingBox();
    await page.mouse.move(original.x + 15, original.y + 25);
    await page.mouse.down();
    await page.mouse.move(original.x + 15, original.y - 135, { steps: 12 });
    await page.mouse.up();
    const dragged = await edgePalette.boundingBox();
    assert(Math.abs(dragged.y - original.y + 160) < 2);
    assert.equal(await gallery.isVisible(), false, 'Dragging must not accidentally open the picker');
    assert(Number(await page.evaluate(() => localStorage.getItem('Theme.PickerPosition'))) < .5);
    await page.goto(base);
    await edgePalette.waitFor();
    await page.waitForFunction(y => Math.abs(document.querySelector('.palette-edge-tab').getBoundingClientRect().top - y) < 2, dragged.y);
    await page.setViewportSize({ width: 375, height: 844 });
    const touchStart = await edgePalette.boundingBox();
    const cdp = await context.newCDPSession(page);
    await cdp.send('Emulation.setTouchEmulationEnabled', { enabled: true, maxTouchPoints: 1 });
    await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [{ x: 16, y: touchStart.y + 25 }] });
    for (let distance = 20; distance <= 180; distance += 20) {
        await cdp.send('Input.dispatchTouchEvent', { type: 'touchMove', touchPoints: [{ x: 16, y: touchStart.y + 25 + distance }] });
    }
    await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });
    const touchEnd = await edgePalette.boundingBox();
    assert(touchEnd.y > touchStart.y + 150, 'Touch dragging moves the edge control');
    assert.equal(await gallery.isVisible(), false);
    await cdp.send('Emulation.setTouchEmulationEnabled', { enabled: false });
    await cdp.detach();
    await page.goto(base);
    await edgePalette.waitFor();
    await page.waitForFunction(y => Math.abs(document.querySelector('.palette-edge-tab').getBoundingClientRect().top - y) < 2, touchEnd.y);
    await edgePalette.focus();
    await page.keyboard.press('Home');
    const minimum = await edgePalette.boundingBox();
    assert(minimum.y >= 70 && minimum.y < 110);
    await page.keyboard.press('End');
    const maximum = await edgePalette.boundingBox();
    const bottomNavigation = await page.locator('.workspace-bottom-nav').boundingBox();
    assert(maximum.y + maximum.height <= bottomNavigation.y - 11, 'The tab stays above mobile navigation');
    await page.keyboard.press('ArrowUp');
    assert(Math.abs((await edgePalette.boundingBox()).y - maximum.y + 24) < 2);
    await page.setViewportSize({ width: 1440, height: 1000 });
    await navigate('/user-accounts');
    await page.locator('.account-page').evaluate(node => { node.dataset.paletteMarker = 'retained'; });
    const paletteNames = { jade: 'یشم', ocean: 'اقیانوس', iris: 'زنبق', amber: 'کهربا' };
    for (const mode of ['light', 'dark']) {
        if (await page.locator('html').getAttribute('data-theme') !== mode) {
            await page.getByRole('button', { name: mode === 'light' ? 'فعال‌سازی حالت روشن' : 'فعال‌سازی حالت تاریک', exact: true }).click();
            await page.waitForFunction(mode => document.documentElement.dataset.theme === mode && !document.documentElement.dataset.themeTransition, mode);
        }
        await edgePalette.focus();
        await page.keyboard.press('Enter');
        await gallery.waitFor();
        await gallery.evaluate(node => Promise.all(node.getAnimations().map(animation => animation.finished.catch(() => {}))));
        for (const [palette, name] of Object.entries(paletteNames)) {
            const previous = await page.locator('html').getAttribute('data-palette');
            await gallery.getByRole('button', { name, exact: true }).click();
            if (previous !== palette) {
                await page.waitForFunction(() => document.documentElement.dataset.themeTransition === 'palette');
                await page.waitForFunction(() => document.getAnimations().some(animation => animation.effect?.pseudoElement === '::view-transition-new(root)' && animation.effect.getKeyframes().some(frame => frame.clipPath?.startsWith('circle('))));
                const wave = await page.evaluate(() => document.getAnimations().find(animation => animation.effect?.pseudoElement === '::view-transition-new(root)')?.effect.getKeyframes().map(frame => frame.clipPath));
                assert(wave?.[0].startsWith('circle(0px at '), 'A palette reveals through a circular snapshot in light and dark');
                assert.equal(await page.locator('.workspace-top-avatar').evaluate(node => getComputedStyle(node).transitionDuration), '0s');
                if (palette === 'ocean' && output) {
                    await page.evaluate(() => {
                        const wave = document.getAnimations().find(animation => animation.effect?.pseudoElement === '::view-transition-new(root)');
                        wave.pause(); wave.currentTime = 120;
                    });
                    await page.screenshot({ path: path.join(output, `palette-wave-${mode}.png`) });
                    await page.evaluate(() => document.getAnimations().find(animation => animation.effect?.pseudoElement === '::view-transition-new(root)').play());
                }
            }
            await page.waitForFunction(palette => document.documentElement.dataset.palette === palette && !document.documentElement.dataset.themeTransition, palette);
            await gallery.locator(`[data-palette-choice="${palette}"][aria-pressed=true]:not(:disabled)`).waitFor();
            assert.equal(await gallery.getByRole('button', { name, exact: true }).getAttribute('aria-pressed'), 'true');
            const savedPalette = await page.evaluate(() => localStorage.getItem('Theme.Palette'));
            assert(savedPalette === palette || (palette === 'jade' && savedPalette === null));
            const contrast = await page.evaluate(() => {
                const style = getComputedStyle(document.documentElement);
                const color = name => { let hex = style.getPropertyValue(`--ui-${name}`).trim().slice(1); if (hex.length === 3) hex = [...hex].map(c => c + c).join(''); return hex.match(/../g).map(c => parseInt(c, 16)); };
                const luminance = rgb => rgb.map(value => { const v = value / 255; return v <= .04045 ? v / 12.92 : ((v + .055) / 1.055) ** 2.4; })
                    .reduce((sum, v, i) => sum + v * [.2126, .7152, .0722][i], 0);
                const ratio = (a, b) => { const values = [luminance(a), luminance(b)].sort((a, b) => b - a); return (values[0] + .05) / (values[1] + .05); };
                return [ratio(color('text-primary'), color('surface')), ratio(color('text-secondary'), color('surface')),
                    ratio(color('link'), color('surface')), ratio([255, 255, 255], color('primary'))];
            });
            assert(contrast.every(value => value >= 4.5), `${palette}/${mode}: readable text and actions (${contrast.join(', ')})`);
            await screenshot(`palette-${palette}-${mode}-desktop`);
        }
        await gallery.getByRole('button', { name: 'بستن انتخاب رنگ', exact: true }).click();
        const collapse = await gallery.evaluate(node => node.getAnimations().find(animation => animation.transitionProperty === 'transform')?.effect.getKeyframes().map(frame => frame.transform));
        assert(collapse?.length >= 2, 'Closing reverses the sheet back into the tab');
        await gallery.waitFor({ state: 'hidden' });
    }
    assert.equal(await page.locator('.account-page').getAttribute('data-palette-marker'), 'retained');
    for (const width of [375, 320]) {
        await page.setViewportSize({ width, height: 844 });
        const bounds = await page.locator('.workspace-topbar-actions').evaluate(node => [...node.children].map(child => child.getBoundingClientRect()).filter(rect => rect.width > 0)
            .map(rect => ({ x: rect.x, width: rect.width, height: rect.height })).sort((a, b) => a.x - b.x));
        assert.equal(bounds.length, 3);
        assert(bounds.every(rect => rect.width === 44 && rect.height === 44));
        assert(bounds.slice(1).every((rect, i) => Math.abs(rect.x - bounds[i].x - bounds[i].width - 8) < 1));
        await edgePalette.click();
        await gallery.waitFor();
        await gallery.evaluate(node => Promise.all(node.getAnimations().map(animation => animation.finished.catch(() => {}))));
        const rect = await gallery.boundingBox();
        const clientWidth = await page.evaluate(() => document.documentElement.clientWidth);
        assert(Math.abs(rect.x) < 1 && rect.x + rect.width <= clientWidth - 11, 'The sheet remains attached to the left edge');
        assert(rect.y >= 11 && rect.y + rect.height <= 833);
        assert.equal(await gallery.evaluate(node => node.scrollWidth > node.clientWidth), false);
        assert(await gallery.locator('.palette-sheet-shape path').getAttribute('d'));
        await screenshot(`palette-sheet-${width}`);
        await page.keyboard.press('Escape');
        await gallery.waitFor({ state: 'hidden' });
        assert(await edgePalette.evaluate(node => document.activeElement === node));
        assert.equal(await page.evaluate(() => document.documentElement.scrollWidth > innerWidth), false);
    }
    await page.goto(base);
    await page.locator('h1').waitFor();
    assert.equal(await page.locator('html').getAttribute('data-palette'), 'amber');
    await edgePalette.click();
    await gallery.waitFor();
    assert.equal(await gallery.getByRole('button', { name: 'کهربا', exact: true }).getAttribute('aria-pressed'), 'true');
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await gallery.getByRole('button', { name: 'زنبق', exact: true }).click();
    await page.waitForFunction(() => document.documentElement.dataset.palette === 'iris');
    assert.equal(await page.locator('html').getAttribute('data-theme-transition'), null);
    assert.equal(await gallery.evaluate(node => getComputedStyle(node).transitionDuration), '0s');
    await page.locator('h1').click();
    await gallery.waitFor({ state: 'hidden' });
    await page.emulateMedia({ reducedMotion: 'no-preference' });

    for (const viewport of [{ width: 375, height: 844 }, { width: 844, height: 390 },
        { width: 768, height: 1024 }, { width: 960, height: 800 }]) {
        await page.setViewportSize(viewport);
        for (const pathname of ['/user-accounts', '/users']) {
            await navigate(pathname);
            const motion = await page.locator('.account-page').evaluate(node => ({
                page: getComputedStyle(node).animationName,
                header: getComputedStyle(node.querySelector('.account-hero')).animationName,
                body: getComputedStyle(node.querySelector('.account-body')).animationName,
                bodyDuration: getComputedStyle(node.querySelector('.account-body')).animationDuration,
                tableIncluded: !!node.querySelector('.account-body .ui-data-panel'),
            }));
            assert.equal(motion.page, 'page-enter', `${pathname}: ${viewport.width}px page entry`);
            assert.equal(motion.header, 'section-enter', `${pathname}: ${viewport.width}px header entry`);
            assert.equal(motion.body, 'section-enter', `${pathname}: ${viewport.width}px body entry`);
            assert.equal(motion.bodyDuration, '0.5s');
            if (pathname === '/users') assert(motion.tableIncluded);
            assert.equal(await page.evaluate(() => document.documentElement.scrollWidth > innerWidth), false);
        }
    }
    await page.setViewportSize({ width: 390, height: 844 });
    await navigate('/');
    assert.equal(await page.locator('.home-layout').evaluate(node => getComputedStyle(node).animationName), 'section-enter');
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await page.goto(base);
    await page.locator('h1').waitFor();
    assert.equal(await page.locator('html').getAttribute('data-theme'), 'dark', 'Explicit appearance survives reload');
    assert.equal(await page.locator('.workspace-main').evaluate(node => getComputedStyle(node).marginInlineStart), '0px');
    assert(await page.locator('.workspace-bottom-nav').isVisible());
    assert.equal(await page.locator('.workspace-main > section').evaluate(node => getComputedStyle(node).animationName), 'none');
    await navigate('/user-accounts');
    for (const selector of ['.account-page', '.account-hero', '.account-body']) {
        assert.equal(await page.locator(selector).evaluate(node => getComputedStyle(node).animationName), 'none');
    }
    await navigate('/');
    await page.getByRole('button', { name: 'فعال‌سازی حالت روشن' }).click();
    await page.waitForFunction(() => document.documentElement.dataset.theme === 'light');
    assert.equal(await page.locator('html').getAttribute('data-theme-transition'), null);
    await page.getByRole('button', { name: 'فعال‌سازی حالت تاریک' }).click();
    assert.equal(await page.evaluate(() => document.documentElement.scrollWidth > innerWidth), false);
    await screenshot('home-mobile-dark');
    await page.getByRole('button', { name: 'باز کردن منوی اصلی' }).click();
    await page.locator('.workspace-sidebar.mobile-open').waitFor();
    assert(await page.locator('.workspace-main').evaluate(node => node.inert));
    assert(await page.locator('.palette-picker-floating').evaluate(node => node.inert));
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
    await page.emulateMedia({ reducedMotion: 'no-preference' });
    await page.goto(`${base}/Account/Login`);
    assert.equal(await page.locator('html').getAttribute('data-palette'), 'iris', 'Palette also restores on static SSR account surfaces');
    assert.equal(await page.locator('.identity-card').evaluate(node => getComputedStyle(node).animationName), 'section-enter');
    await page.emulateMedia({ reducedMotion: 'reduce' });
    assert.equal(await page.locator('.identity-card').evaluate(node => getComputedStyle(node).animationName), 'none');
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
