// Раскрытие мобильного меню и меню аккаунта.

function bindToggle(button, panel, openClass) {
  button.addEventListener('click', () => {
    const isOpen = panel.classList.toggle(openClass);
    button.setAttribute('aria-expanded', String(isOpen));
  });
}

function closeOnOutsideClick(root, panel, button, openClass) {
  document.addEventListener('click', (event) => {
    if (root.contains(event.target) || !panel.classList.contains(openClass)) {
      return;
    }

    panel.classList.remove(openClass);
    button.setAttribute('aria-expanded', 'false');
  });
}

export function initNav() {
  const navToggle = document.querySelector('[data-nav-toggle]');
  const nav = navToggle && document.getElementById(navToggle.getAttribute('aria-controls'));

  if (navToggle && nav) {
    bindToggle(navToggle, nav, 'is-open');
  }

  document.querySelectorAll('[data-menu]').forEach((menu) => {
    const button = menu.querySelector('[data-menu-toggle]');

    if (!button) {
      return;
    }

    bindToggle(button, menu, 'is-open');
    closeOnOutsideClick(menu, menu, button, 'is-open');

    // Escape возвращает фокус на кнопку: иначе после закрытия списка
    // клавиатурный фокус остаётся в невидимой панели.
    menu.addEventListener('keydown', (event) => {
      if (event.key !== 'Escape' || !menu.classList.contains('is-open')) {
        return;
      }

      menu.classList.remove('is-open');
      button.setAttribute('aria-expanded', 'false');
      button.focus();
    });
  });
}
