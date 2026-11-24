(() => {
    document.addEventListener('click', async event => {
        const button = event.target.closest?.('[data-login-captcha-refresh]');
        if (!button || button.disabled) return;
        const panel = button.closest('.login-captcha');
        const form = button.closest('form');
        const id = panel?.querySelector('[name="Input.CaptchaId"]');
        const image = panel?.querySelector('img');
        const answer = panel?.querySelector('[name="Input.CaptchaAnswer"]');
        const error = panel?.querySelector('[data-login-captcha-error]');
        const token = form?.querySelector('[name="__RequestVerificationToken"]')?.value;
        if (!id || !image || !answer || !error || !token) return;

        button.disabled = true;
        error.textContent = '';
        error.hidden = true;
        try {
            const response = await fetch(button.dataset.loginCaptchaRefresh, {
                method: 'POST', credentials: 'same-origin', cache: 'no-store',
                headers: { 'X-CSRF-TOKEN': token, 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams({ id: id.value })
            });
            if (!response.ok) throw new Error('Captcha refresh failed');
            const challenge = await response.json();
            if (!challenge.id || !challenge.image?.startsWith('data:image/png;base64,')) throw new Error('Invalid challenge');
            if (!button.isConnected) return;
            image.src = challenge.image;
            id.value = challenge.id;
            answer.value = '';
            answer.focus();
        } catch {
            if (!button.isConnected) return;
            error.textContent = 'تازه‌سازی تصویر انجام نشد. دوباره تلاش کنید.';
            error.hidden = false;
        } finally {
            button.disabled = false;
        }
    });

    document.addEventListener('submit', event => {
        if (event.target.querySelector?.('[data-login-captcha-refresh]:disabled')) {
            event.preventDefault();
            event.stopImmediatePropagation();
        }
    }, true);
})();
