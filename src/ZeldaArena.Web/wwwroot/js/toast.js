// Тосты (docs/SPEC.md §10.1: «обработка 401/403/429 понятным тостом», §9.1: aria-live).
//
// Регион тостов свёрстан в _Layout с aria-live="polite", поэтому скринридер
// зачитывает сообщение сам, а тексты по кодам ответа лежат там же в data-атрибутах:
// они переводятся на сервере вместе с остальными ресурсами (§9.5).

const DISPLAY_MS = 6000;

function region() {
  return document.querySelector('[data-toasts]');
}

/** Локализованный текст из data-атрибута региона тостов: msgUnauthorized, msgTooMany и т. д. */
export function toastText(key) {
  return region()?.dataset[key] ?? '';
}

/**
 * Показывает сообщение. kind — info | success | error; link — необязательная
 * ссылка-действие ({ href, text }), например «Войти» на 401.
 */
export function showToast(message, { kind = 'info', link } = {}) {
  const host = region();

  if (!host || !message) {
    return;
  }

  const toast = document.createElement('div');
  toast.className = `toast-token toast-token--${kind}`;

  const text = document.createElement('p');
  text.className = 'toast-token__text';
  text.textContent = message;
  toast.append(text);

  if (link) {
    const action = document.createElement('a');
    action.className = 'toast-token__action';
    action.href = link.href;
    action.textContent = link.text;
    toast.append(action);
  }

  const close = document.createElement('button');
  close.type = 'button';
  close.className = 'toast-token__close';
  close.setAttribute('aria-label', host.dataset.closeLabel ?? '');
  close.textContent = '×';
  close.addEventListener('click', () => toast.remove());
  toast.append(close);

  host.append(toast);

  // Ошибку не убираем сама: человек должен успеть её прочесть и, возможно, нажать ссылку.
  if (kind !== 'error') {
    setTimeout(() => toast.remove(), DISPLAY_MS);
  }
}
