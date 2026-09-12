// Пульт счёта матча без перезагрузки (docs/SPEC.md §9.4 п. 3, §11).
//
// Разметка: [data-console][data-api][data-status-api][data-state], внутри — обычные
// формы [data-console-form] (счёт) и [data-console-status-form] (старт, финиш, отмена).
// Без JavaScript они отправляются на страницу и возвращают её же; здесь та же форма
// уходит в /api/admin/matches, а ответ перерисовывает табло.
//
// Состояние матча приходит от сервера — и в data-state при отрисовке страницы,
// и в каждом ответе. Клиент ничего не досчитывает сам, поэтому кнопки не могут
// разойтись с действительным состоянием матча.
//
// В скрытых полях уходит счёт, который сейчас на экране. Если его успели изменить
// в другом окне, сервер отвечает 409 — правка не затирает чужую (§15).

import { request, reportError, HttpError, problemMessage } from './http.js';
import { showToast } from './toast.js';

const RELOAD_DELAY_MS = 1500;

function render(root, state) {
  root.querySelector('[data-console-score-a]').textContent = String(state.scoreA);
  root.querySelector('[data-console-score-b]').textContent = String(state.scoreB);

  root.querySelectorAll('[data-console-form]').forEach((form) => {
    const button = form.querySelector('[data-console-side]');
    const delta = Number(button.dataset.consoleDelta);
    const scoreA = button.dataset.consoleSide === 'a' ? state.scoreA + delta : state.scoreA;
    const scoreB = button.dataset.consoleSide === 'b' ? state.scoreB + delta : state.scoreB;

    form.elements.scoreA.value = String(scoreA);
    form.elements.scoreB.value = String(scoreB);
    form.elements.expectedScoreA.value = String(state.scoreA);
    form.elements.expectedScoreB.value = String(state.scoreB);
    button.disabled = scoreA < 0 || scoreB < 0;
  });

  const finish = root.querySelector('[data-console-finish]');

  if (finish) {
    finish.disabled = !state.canFinish;
  }
}

export function initMatchConsole() {
  const root = document.querySelector('[data-console][data-state]');

  if (!root) {
    return;
  }

  let state = JSON.parse(root.dataset.state);

  async function send(form, url) {
    const buttons = [...root.querySelectorAll('button')];
    buttons.forEach((button) => { button.disabled = true; });

    try {
      const response = await request(url, { method: 'POST', body: new FormData(form) });
      const result = await response.json();

      // Смена статуса меняет и набор кнопок, и заголовок — их рисует сервер.
      if (result.match.status !== state.status) {
        location.reload();
        return;
      }

      state = result.match;
      showToast(result.message, { kind: 'success' });
    } catch (error) {
      if (error instanceof HttpError && error.status === 409) {
        // Счёт успели изменить в другом окне: показываем чужую правку, а не свою.
        showToast(problemMessage(error.problem), { kind: 'error' });
        setTimeout(() => location.reload(), RELOAD_DELAY_MS);
        return;
      }

      reportError(error);
    }

    render(root, state);
  }

  root.addEventListener('submit', (event) => {
    const scoreForm = event.target.closest('[data-console-form]');
    const statusForm = event.target.closest('[data-console-status-form]');

    if (!scoreForm && !statusForm) {
      return;
    }

    event.preventDefault();
    send(scoreForm ?? statusForm, scoreForm ? root.dataset.api : root.dataset.statusApi);
  });
}
