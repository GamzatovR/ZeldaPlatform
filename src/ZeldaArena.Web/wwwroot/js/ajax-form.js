// Отправка форм оплаты, оформления заказа и ввода кода без перезагрузки
// (docs/SPEC.md §7.6, §10.1, сценарии 7 и 8).
//
// Разметка: <form data-ajax-form="/api/…">. Форма уходит как есть — FormData
// с антифоржери-токеном и теми же именами полей, поэтому сервер биндит ту же
// ViewModel, а его ошибки ложатся в те же span[data-valmsg-for], что рисует
// unobtrusive-валидация. Успех — JSON { redirectUrl }, переход решает сервер.
//
// Сначала работает клиентская валидация: при ошибке jquery.validate останавливает
// событие submit раньше, и сюда оно не доходит. Сервер при этом проверяет всё
// заново (§15, двухуровневая валидация).

import { request, reportError } from './http.js';
import { showToast } from './toast.js';

const FIELD_ERROR = 'field-validation-error';
const FIELD_OK = 'field-validation-valid';

function fieldSelector(attribute, name) {
  return `[${attribute}="${CSS.escape(name)}" i]`;
}

function clearErrors(form) {
  form.querySelectorAll(`.${FIELD_ERROR}`).forEach((span) => {
    span.textContent = '';
    span.classList.replace(FIELD_ERROR, FIELD_OK);
  });
  form.querySelectorAll('.input-validation-error').forEach((input) => input.classList.remove('input-validation-error'));

  const summary = form.querySelector('[data-valmsg-summary]');
  summary?.classList.replace('validation-summary-errors', 'validation-summary-valid');
  summary?.querySelector('ul')?.replaceChildren();
}

function showSummary(form, messages) {
  const summary = form.querySelector('[data-valmsg-summary]');

  if (!summary) {
    showToast(messages.join(' '), { kind: 'error' });
    return;
  }

  let list = summary.querySelector('ul');

  if (!list) {
    list = document.createElement('ul');
    summary.append(list);
  }

  list.replaceChildren(...messages.map((text) => {
    const item = document.createElement('li');
    item.textContent = text;
    return item;
  }));
  summary.classList.replace('validation-summary-valid', 'validation-summary-errors');
}

/** Ошибки полей — к полям, остальное (отказ сценария) — в сводку формы. */
function showErrors(form, problem) {
  const loose = [];
  let firstInvalid = null;

  for (const [name, messages] of Object.entries(problem?.errors ?? {})) {
    const span = form.querySelector(fieldSelector('data-valmsg-for', name));
    const input = form.querySelector(fieldSelector('name', name));

    if (!span) {
      loose.push(...messages);
      continue;
    }

    span.textContent = messages[0];
    span.classList.replace(FIELD_OK, FIELD_ERROR);
    input?.classList.add('input-validation-error');
    firstInvalid ??= input;
  }

  if (problem?.detail) {
    loose.push(problem.detail);
  }

  if (loose.length > 0) {
    showSummary(form, loose);
  }

  firstInvalid?.focus();
}

function setBusy(form, busy) {
  form.querySelectorAll('button[type="submit"], button:not([type])').forEach((button) => {
    button.disabled = busy;
  });
}

async function onSubmit(event) {
  const form = event.target;

  if (!form.matches('form[data-ajax-form]') || event.defaultPrevented) {
    return;
  }

  event.preventDefault();

  // Повторное нажатие, пока идёт запрос, не создаёт второй платёж. Ключ
  // идемпотентности защищает и на сервере, но лишнее письмо с кодом ни к чему.
  if (form.dataset.busy) {
    return;
  }

  form.dataset.busy = 'true';
  setBusy(form, true);
  clearErrors(form);

  try {
    const response = await request(form.dataset.ajaxForm, { method: 'POST', body: new FormData(form) });
    const { redirectUrl } = await response.json();

    // Кнопки остаются заблокированными до перехода.
    location.assign(redirectUrl);
    return;
  } catch (error) {
    if ([400, 404, 409].includes(error?.status)) {
      showErrors(form, error.problem);

      const attempts = error.problem?.attemptsLeft;
      const counter = document.querySelector('[data-attempts-left]');

      if (counter && Number.isInteger(attempts)) {
        counter.textContent = String(attempts);
      }
    } else {
      reportError(error);
    }
  }

  delete form.dataset.busy;
  setBusy(form, false);
}

export function initAjaxForms() {
  document.addEventListener('submit', onSubmit);
}
