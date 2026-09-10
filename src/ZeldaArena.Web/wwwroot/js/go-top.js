// Кнопка «наверх». Появляется после прокрутки на высоту экрана и до этого
// скрыта атрибутом hidden — так её нет ни в раскладке, ни в обходе клавиатурой.

const SHOW_AFTER_RATIO = 1;

export function initGoTop() {
  const button = document.querySelector('[data-go-top]');

  if (!button) {
    return;
  }

  const sync = () => {
    button.hidden = window.scrollY < window.innerHeight * SHOW_AFTER_RATIO;
  };

  button.addEventListener('click', () => {
    // Пользователю, попросившему меньше движения, плавную прокрутку не навязываем.
    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    window.scrollTo({ top: 0, behavior: reduced ? 'auto' : 'smooth' });
  });

  window.addEventListener('scroll', sync, { passive: true });
  sync();
}
