(() => {
  const normalize = value => [...String(value || '')].map(ch =>
    ch >= '۰' && ch <= '۹' ? String(ch.charCodeAt(0) - 1776) :
    ch >= '٠' && ch <= '٩' ? String(ch.charCodeAt(0) - 1632) : ch)
    .join('').replace(/[^0-9]/g, '').slice(0, 6);

  let pendingOtp;
  const render = () => {
    document.querySelectorAll('[data-otp-input]').forEach(group => {
      const input = group.querySelector('[data-otp-real]');
      if (!input || group.dataset.initialized === 'true') return;
      group.dataset.initialized = 'true';
      const boxes = [...group.querySelectorAll('.sms-otp-digit')];
      const submitter = input.form?.querySelector('[data-otp-submit]');
      let sent = false;
      const refresh = (autoSubmit = false) => {
        input.value = normalize(input.value);
        if (input.value.length < 6) sent = false;
        boxes.forEach((box, i) => {
          box.textContent = input.value[i] || '';
          box.classList.toggle('filled', i < input.value.length);
          box.classList.toggle('next', i === input.value.length && document.activeElement === input);
        });
        if (autoSubmit && group.dataset.otpAutoSubmit === 'true' && input.value.length === 6 && !sent && submitter && input.form?.requestSubmit) {
          sent = true;
          // Keep the submitter enabled so its Input.Action=verify name/value
          // is included in the POST and Razor can distinguish it from resend.
          input.form.requestSubmit(submitter);
        }
      };
      input.addEventListener('input', () => refresh(true));
      input.addEventListener('focus', () => refresh());
      input.addEventListener('blur', () => refresh());
      group.addEventListener('click', () => input.focus());
      input.addEventListener('paste', e => {
        const pasted = e.clipboardData?.getData('text');
        if (!pasted) return;
        e.preventDefault();
        input.value = normalize(pasted);
        input.dispatchEvent(new Event('input', {bubbles:true}));
      });
      input.form?.addEventListener('submit', e => {
        input.value = normalize(input.value);
        pendingOtp?.abort();
        const action = e.submitter?.value;
        if (action === 'verify') {
          if (sent && e.submitter?.dataset?.otpSubmitInFlight === 'true') {
            e.preventDefault();
            return;
          }
          if (e.submitter?.dataset) e.submitter.dataset.otpSubmitInFlight = 'true';
          sent = true;
        }
      });
      refresh();
      if (group.dataset.otpSms !== 'false' && window.isSecureContext && 'OTPCredential' in window && navigator.credentials?.get) {
        pendingOtp?.abort();
        pendingOtp = new AbortController();
        navigator.credentials.get({otp: {transport: ['sms']}, signal: pendingOtp.signal})
          .then(result => {
            if (!result?.code || !input.isConnected) return;
            input.value = normalize(result.code);
            input.dispatchEvent(new Event('input', {bubbles: true}));
          }).catch(() => {});
      }
    });

    const requestButton = document.querySelector('[data-otp-request]');
    if (requestButton?.form?.addEventListener && requestButton.dataset?.otpRequestInitialized !== 'true') {
      requestButton.dataset.otpRequestInitialized = 'true';
      const form = requestButton.form;
      const label = requestButton.querySelector('[data-otp-request-label]');
      const originalLabel = label?.textContent || 'ارسال کد ورود';
      let nativeSubmitReady = false;
      let pendingTimer;
      const reset = () => {
        if (pendingTimer) clearTimeout(pendingTimer);
        pendingTimer = undefined;
        nativeSubmitReady = false;
        delete form.dataset.otpRequestPending;
        form.removeAttribute('aria-busy');
        requestButton.removeAttribute?.('aria-disabled');
        requestButton.disabled = false;
        requestButton.classList.remove('is-pending');
        if (label) label.textContent = originalLabel;
      };

      form.addEventListener('submit', event => {
        if (event.submitter && event.submitter !== requestButton) return;
        if (nativeSubmitReady) {
          // The second, real native submission includes the enabled submitter's
          // Input.Action=request; disabling it before serialization drops that value.
          nativeSubmitReady = false;
          setTimeout(() => {
            if (requestButton.isConnected) requestButton.disabled = true;
          }, 0);
          return;
        }
        if (form.dataset.otpRequestPending === 'true') {
          event.preventDefault();
          return;
        }
        if (typeof form.requestSubmit !== 'function') return; // non-JS/legacy fallback
        // Validate via the browser first, then paint the loading state before
        // triggering the same native, antiforgery-protected form submission.
        event.preventDefault();
        event.stopImmediatePropagation?.();
        form.dataset.otpRequestPending = 'true';
        form.setAttribute('aria-busy', 'true');
        requestButton.setAttribute?.('aria-disabled', 'true');
        requestButton.classList.add('is-pending');
        if (label) label.textContent = 'در حال ارسال کد…';
        pendingTimer = setTimeout(() => {
          pendingTimer = undefined;
          if (form.isConnected === false || requestButton.isConnected === false) {
            reset();
            return;
          }
          nativeSubmitReady = true;
          try {
            form.requestSubmit(requestButton);
          } catch {
            reset();
          }
        }, 160);
      }, true);

      // Back/forward cache can restore the page with its DOM state intact.
      window.addEventListener?.('pageshow', event => {
        if (event.persisted) reset();
      });
    }

    const resend = document.querySelector('[data-otp-resend-seconds]');
    if (resend && resend.dataset.initialized !== 'true') {
      let countdown;
      resend.dataset.initialized = 'true';
      const seconds = Math.max(0, Number.parseInt(resend.dataset.otpResendSeconds || '0', 10) || 0);
      const end = Date.now() + seconds * 1000;
      const counter = resend.querySelector('[data-otp-resend-count]');
      const tick = () => {
        if (!resend.isConnected) {
          if (countdown) clearInterval(countdown);
          countdown = undefined;
          return;
        }
        const left = Math.max(0, Math.ceil((end - Date.now()) / 1000));
        resend.disabled = left > 0;
        if (counter) counter.textContent = left > 0
          ? String(Math.floor(left / 60)).padStart(2, '0') + ':' + String(left % 60).padStart(2, '0')
          : '';
        if (left === 0 && countdown) {
          clearInterval(countdown);
          countdown = undefined;
        }
      };
      tick();
      if (seconds > 0 && typeof setInterval === 'function') countdown = setInterval(tick, 250);
    }
  };
  window.Mkx = window.Mkx || {};
  window.Mkx.otp = { initialize: render };
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', render);
  else render();
  if (typeof Blazor !== 'undefined') Blazor.addEventListener('enhancedload', render);
})();
