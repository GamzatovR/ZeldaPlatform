// Единая обёртка над fetch для Areas/Api (docs/SPEC.md §10.1).
//
// - Антифоржери-токен из <meta> уходит заголовком RequestVerificationToken
//   в каждом изменяющем запросе (§15).
// - 401/403/429/5xx и обрыв сети объясняются тостом здесь, в одном месте.
//   400/404/409 — дело вызывающего: только он знает, куда положить ошибку поля.
// - Отмена через AbortController не считается ошибкой и не показывает тост.

import { showToast, toastText } from './toast.js';

const TOKEN_HEADER = 'RequestVerificationToken';

/** Ответ API с кодом ошибки; problem — разобранный ProblemDetails или null. */
export class HttpError extends Error {
  constructor(status, problem) {
    super(problem?.detail ?? problem?.title ?? `HTTP ${status}`);
    this.name = 'HttpError';
    this.status = status;
    this.problem = problem;
  }
}

export function isAbort(error) {
  return error?.name === 'AbortError';
}

function antiforgeryToken() {
  return document.querySelector('meta[name="request-verification-token"]')?.content ?? '';
}

async function readProblem(response) {
  const type = response.headers.get('Content-Type') ?? '';

  if (!type.includes('json')) {
    return null;
  }

  try {
    return await response.json();
  } catch {
    return null;
  }
}

function retryMinutes(response) {
  const seconds = Number.parseInt(response.headers.get('Retry-After') ?? '', 10);

  return Number.isFinite(seconds) && seconds > 0 ? Math.ceil(seconds / 60) : 1;
}

function explain(response) {
  switch (response.status) {
    case 401: {
      // Вернуться после входа туда же, где был пользователь.
      const returnUrl = encodeURIComponent(location.pathname + location.search);
      showToast(toastText('msgUnauthorized'), {
        kind: 'error',
        link: { href: `/Identity/Account/Login?returnUrl=${returnUrl}`, text: toastText('msgLogin') },
      });
      return true;
    }
    case 403:
      showToast(toastText('msgForbidden'), { kind: 'error' });
      return true;
    case 429:
      showToast(toastText('msgTooMany').replace('{0}', String(retryMinutes(response))), { kind: 'error' });
      return true;
    default:
      if (response.status >= 500) {
        showToast(toastText('msgServer'), { kind: 'error' });
        return true;
      }
      return false;
  }
}

/**
 * Запрос к API. json — тело, которое уйдёт как application/json; body — FormData
 * или строка как есть. Возвращает Response при 2xx, иначе бросает HttpError.
 */
export async function request(url, { method = 'GET', body, json, signal, accept = 'application/json' } = {}) {
  const headers = { Accept: accept, 'X-Requested-With': 'fetch' };
  let payload = body;

  if (method !== 'GET') {
    headers[TOKEN_HEADER] = antiforgeryToken();
  }

  if (json !== undefined) {
    headers['Content-Type'] = 'application/json';
    payload = JSON.stringify(json);
  }

  let response;

  try {
    response = await fetch(url, { method, headers, body: payload, signal, credentials: 'same-origin' });
  } catch (error) {
    if (!isAbort(error)) {
      showToast(toastText('msgNetwork'), { kind: 'error' });
    }
    throw error;
  }

  if (response.ok) {
    return response;
  }

  const problem = await readProblem(response);
  const error = new HttpError(response.status, problem);
  error.explained = explain(response);

  throw error;
}

/**
 * Первое сообщение ProblemDetails: detail отказа сценария или первая ошибка поля.
 * title не берётся — это служебная английская фраза фреймворка.
 */
export function problemMessage(problem) {
  if (problem?.detail) {
    return problem.detail;
  }

  const fieldErrors = Object.values(problem?.errors ?? {}).flat();

  return fieldErrors[0] ?? '';
}

/** Ошибка, которую вызывающий не обработал сам: показать хотя бы текст сервера. */
export function reportError(error) {
  if (isAbort(error) || error?.explained) {
    return;
  }

  showToast(problemMessage(error?.problem) || toastText('msgServer'), { kind: 'error' });
}
