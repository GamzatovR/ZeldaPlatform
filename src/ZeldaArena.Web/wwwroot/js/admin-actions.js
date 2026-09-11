// Быстрые действия в строке таблицы админки без перезагрузки (docs/SPEC.md §9.1,
// §10.1 сценарий 12): одобрить команду, заблокировать пользователя, сменить статус.
//
// Разметка: <form method="post" action="/admin/…" data-admin-action="/api/admin/…">.
// Без JavaScript это обычная форма: POST на страницу админки, редирект, сообщение
// через TempData (PRG). С ним та же форма уходит в API как FormData — поля и
// антифоржери-токен те же, — ответ { message } показывается тостом, а таблица
// перерисовывается событием list:refresh (ajax-list.js) с тем же фильтром,
// сортировкой и страницей.

import { request, reportError } from './http.js';
import { showToast } from './toast.js';

export function initAdminActions() {
  document.addEventListener('submit', async (event) => {
    const form = event.target.closest('form[data-admin-action]');

    if (!form || event.defaultPrevented) {
      return;
    }

    event.preventDefault();

    const buttons = [...form.querySelectorAll('button')];
    buttons.forEach((button) => { button.disabled = true; });

    try {
      const response = await request(form.dataset.adminAction, {
        method: form.dataset.method ?? 'POST',
        body: new FormData(form),
      });
      const result = await response.json();

      showToast(result.message, { kind: 'success' });
      form.dispatchEvent(new CustomEvent('admin-action:done', { bubbles: true, detail: result }));
      document.dispatchEvent(new CustomEvent('list:refresh'));
    } catch (error) {
      reportError(error);
    } finally {
      buttons.forEach((button) => { button.disabled = false; });
    }
  });
}
