// Корзина без перезагрузки.

import { request, reportError } from './http.js';
import { showToast } from './toast.js';

function updateMiniCart({ itemCount, miniCartLabel }) {
  const count = document.querySelector('[data-mini-cart-count]');
  const label = document.querySelector('[data-mini-cart-label]');

  if (count) {
    count.textContent = String(itemCount);
    count.hidden = itemCount === 0;
  }

  if (label) {
    label.textContent = miniCartLabel;
  }
}

async function run(form, send) {
  const buttons = [...form.querySelectorAll('button')];
  buttons.forEach((button) => { button.disabled = true; });

  try {
    const response = await send();
    const data = await response.json();

    updateMiniCart(data);
    showToast(data.message, { kind: 'success' });
  } catch (error) {
    reportError(error);
  } finally {
    buttons.forEach((button) => { button.disabled = false; });
  }
}

/**
 * Что было в фокусе до отправки. Снимается заранее: на время запроса кнопки
 * блокируются, и заблокированная кнопка фокус теряет.
 */
function focusKey(element) {
  return {
    line: element?.closest('[data-cart-line]')?.dataset.cartLine,
    label: element?.getAttribute('aria-label'),
    id: element?.id,
  };
}

/**
 * Перерисовка таблицы. Фокус возвращается на тот же элемент управления той же строки:
 * иначе после «+» с клавиатуры фокус падал бы в начало страницы.
 */
async function refreshCart(cart, { line, label, id }) {
  try {
    const response = await request(cart.dataset.api, { accept: 'text/html' });
    cart.innerHTML = await response.text();
  } catch (error) {
    reportError(error);
    return;
  }

  const target = (id && document.getElementById(id))
    || (line && label && cart.querySelector(`[data-cart-line="${line}"] [aria-label="${CSS.escape(label)}"]`));

  if (target && !target.disabled) {
    target.focus();
  }
}

function onSubmit(event) {
  const form = event.target;

  if (form.matches('form[data-add-to-cart]')) {
    event.preventDefault();
    run(form, () => request(form.dataset.addToCart, { method: 'POST', body: new FormData(form) }));
    return;
  }

  const cart = form.closest('[data-cart][data-api]');
  const { cartQuantity, cartRemove } = form.dataset;

  if (!cart || (!cartQuantity && !cartRemove)) {
    return;
  }

  event.preventDefault();

  const focused = focusKey(event.submitter ?? document.activeElement);
  const itemUrl = `${cart.dataset.api}/items/${cartQuantity ?? cartRemove}`;
  const send = cartQuantity
    ? () => request(itemUrl, { method: 'PATCH', json: { quantity: Number(new FormData(form).get('Quantity')) } })
    : () => request(itemUrl, { method: 'DELETE' });

  // Таблица перерисовывается и после отказа: «на складе осталось 3» значит,
  // что строка на экране уже не соответствует серверу.
  run(form, send).then(() => refreshCart(cart, focused));
}

export function initCart() {
  document.addEventListener('submit', onSubmit);

  // Поле количества применяется сразу по change — отдельная кнопка «Применить»
  // нужна только без JavaScript. requestSubmit проверит min/max самим браузером.
  document.addEventListener('change', (event) => {
    const input = event.target;

    if (input.name === 'Quantity' && input.form?.dataset.cartQuantity) {
      input.form.requestSubmit();
    }
  });
}
