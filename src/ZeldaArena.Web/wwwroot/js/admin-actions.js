// Быстрые действия в строке таблицы админки без перезагрузки.

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
