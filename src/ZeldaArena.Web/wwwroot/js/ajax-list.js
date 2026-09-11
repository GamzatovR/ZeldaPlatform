// Фильтры, пагинация и вкладки без перезагрузки (docs/SPEC.md §9.1, §10.1–10.3).
//
// Источник истины — адрес страницы. Любое действие со списком сначала меняет
// query-string через history.pushState, а потом запрашивает у Areas/Api тот же
// partial, которым страница рисует список, с теми же параметрами. Поэтому F5,
// пересланная ссылка и «Назад» (popstate) восстанавливают ровно то же состояние,
// а без JavaScript всё работает обычной GET-формой и обычными ссылками.
//
// Разметка-контракт:
//   [data-list][data-api="/api/…"]  — контейнер списка и адрес его эндпоинта;
//   [data-filter-form]              — GET-форма фильтра (необязательна);
//   [data-filter-reset]             — «Сбросить всё»;
//   [data-pagination] a, a[data-tab] — ссылки, которые перехватываются;
//   select[data-default]            — значение по умолчанию, в адрес не пишется.

import { request, isAbort, reportError } from './http.js';
import { renderChips } from './filter-chips.js';

const SKELETON_DELAY_MS = 150;
const TYPING_DELAY_MS = 400;
const TEXT_TYPES = ['search', 'text', 'number'];

function pathAndQuery(url) {
  return url.pathname + url.search;
}

/** Адрес из формы: пустые поля и значения по умолчанию в него не попадают. */
function urlFromForm(form) {
  const url = new URL(form.getAttribute('action') || location.pathname, location.href);
  const params = new URLSearchParams();

  for (const field of form.elements) {
    if (!field.name || field.disabled || field.type === 'submit' || field.name.startsWith('__')) {
      continue;
    }

    if ((field.type === 'checkbox' || field.type === 'radio') && !field.checked) {
      continue;
    }

    const value = field.value.trim();

    if (value !== '' && value !== field.dataset.default) {
      params.append(field.name, value);
    }
  }

  url.search = params.toString();

  return url;
}

/** Значения формы из адреса — для «Назад» и «Сбросить всё». */
function syncForm(form, params) {
  for (const field of form.elements) {
    if (!field.name || field.type === 'hidden' || field.type === 'submit') {
      continue;
    }

    const values = params.getAll(field.name);

    if (field.type === 'checkbox' || field.type === 'radio') {
      field.checked = values.includes(field.value);
    } else if (field.tagName === 'SELECT') {
      // Параметры адреса регистронезависимы (status=ongoing из примера §10.2).
      const wanted = (values[0] ?? field.dataset.default ?? '').toLowerCase();
      const option = [...field.options].find((item) => item.value.toLowerCase() === wanted);
      field.value = option ? option.value : '';
    } else {
      field.value = values[0] ?? '';
    }
  }
}

/** Ключ состояния вкладки: параметры адреса без номера страницы. */
function tabKey(url) {
  const params = new URLSearchParams(url.search);
  params.delete('page');
  params.sort();

  return params.toString();
}

function markTabs(url) {
  document.querySelectorAll('a[data-tab]').forEach((tab) => {
    if (tabKey(new URL(tab.href)) === tabKey(url)) {
      tab.setAttribute('aria-current', 'page');
    } else {
      tab.removeAttribute('aria-current');
    }
  });
}

function showSkeleton(container) {
  const template = document.getElementById('list-skeleton');

  if (template) {
    container.replaceChildren(template.content.cloneNode(true));
  }
}

function prefersReducedMotion() {
  return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
}

export function initAjaxList() {
  const container = document.querySelector('[data-list][data-api]');

  if (!container) {
    return;
  }

  const form = document.querySelector('[data-filter-form]');
  let inFlight = null;
  let current = pathAndQuery(location);

  async function load(url, { push = true, scroll = false } = {}) {
    // Тот же адрес — тот же список: change после input на числовом поле
    // и hash-переходы не должны гонять лишний запрос.
    if (pathAndQuery(url) === current) {
      return;
    }

    // Устаревший запрос отменяется: иначе медленный ответ на прошлый фильтр
    // мог бы прийти последним и перерисовать список не тем состоянием.
    inFlight?.abort();
    const controller = new AbortController();
    inFlight = controller;

    const before = current;

    if (push) {
      history.pushState({ ajaxList: true }, '', pathAndQuery(url));
    }

    current = pathAndQuery(url);

    const previous = [...container.childNodes];
    const skeletonTimer = setTimeout(() => showSkeleton(container), SKELETON_DELAY_MS);
    container.setAttribute('aria-busy', 'true');

    try {
      const response = await request(container.dataset.api + url.search, {
        signal: controller.signal,
        accept: 'text/html',
      });
      const html = await response.text();

      clearTimeout(skeletonTimer);
      container.innerHTML = html;
      markTabs(url);

      if (scroll && container.getBoundingClientRect().top < 0) {
        container.scrollIntoView({ block: 'start', behavior: prefersReducedMotion() ? 'auto' : 'smooth' });
      }
    } catch (error) {
      clearTimeout(skeletonTimer);

      if (!isAbort(error)) {
        // Список остался прежним — значит, и адрес должен остаться прежним:
        // иначе F5 показал бы не то, что сейчас на экране.
        container.replaceChildren(...previous);
        current = before;
        history.replaceState({ ajaxList: true }, '', before);
        markTabs(new URL(before, location.href));
        if (form) {
          syncForm(form, new URL(before, location.href).searchParams);
        }
        reportError(error);
      }
    } finally {
      if (inFlight === controller) {
        inFlight = null;
        container.removeAttribute('aria-busy');
        renderChips(form);
      }
    }
  }

  if (form) {
    let typingTimer = 0;

    form.addEventListener('submit', (event) => {
      event.preventDefault();
      clearTimeout(typingTimer);
      load(urlFromForm(form));
    });

    form.addEventListener('input', (event) => {
      if (!TEXT_TYPES.includes(event.target.type)) {
        return;
      }

      clearTimeout(typingTimer);
      typingTimer = setTimeout(() => load(urlFromForm(form)), TYPING_DELAY_MS);
    });

    form.addEventListener('change', (event) => {
      if (!TEXT_TYPES.includes(event.target.type)) {
        load(urlFromForm(form));
      }
    });

    form.querySelector('[data-filter-reset]')?.addEventListener('click', (event) => {
      event.preventDefault();
      syncForm(form, new URLSearchParams());
      load(new URL(event.currentTarget.href));
    });
  }

  document.addEventListener('click', (event) => {
    if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
      return;
    }

    const link = event.target.closest('a[data-tab], [data-pagination] a[href]');

    if (!link || (!link.matches('[data-tab]') && !container.contains(link))) {
      return;
    }

    event.preventDefault();
    load(new URL(link.href), { scroll: !link.matches('[data-tab]') });
  });

  window.addEventListener('popstate', () => {
    if (form) {
      syncForm(form, new URLSearchParams(location.search));
    }

    load(new URL(location.href), { push: false });
  });
}
