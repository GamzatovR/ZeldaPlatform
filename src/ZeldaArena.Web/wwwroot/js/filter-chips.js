// Чипсы активных фильтров (docs/SPEC.md §10.2): «Статус: Идёт ×».
//
// Строятся из самой формы, а не из отдельного описания фильтров: подпись — <label>
// поля, значение — текст выбранного <option>. Поэтому модуль один на все списки
// и не требует ничего от сервера. Сортировка и размер страницы — не фильтры, они
// помечены data-no-chip. Без JavaScript чипсов нет, остаётся «Сбросить всё».

function labelOf(form, field) {
  const label = field.id ? form.querySelector(`label[for="${CSS.escape(field.id)}"]`) : null;

  return label?.textContent.trim() || field.name;
}

function valueOf(field) {
  if (field.type === 'checkbox') {
    return '';
  }

  if (field.tagName === 'SELECT') {
    return field.selectedOptions[0]?.textContent.trim() ?? '';
  }

  return field.value.trim();
}

function isActive(field) {
  if (!field.name || field.type === 'hidden' || field.hasAttribute('data-no-chip')) {
    return false;
  }

  if (field.type === 'checkbox' || field.type === 'radio') {
    return field.checked;
  }

  return ['INPUT', 'SELECT'].includes(field.tagName) && field.value.trim() !== '';
}

function clear(field) {
  if (field.type === 'checkbox' || field.type === 'radio') {
    field.checked = false;
  } else {
    field.value = '';
  }
}

export function renderChips(form) {
  const host = form?.querySelector('[data-filter-chips]');

  if (!host) {
    return;
  }

  host.replaceChildren();

  for (const field of form.elements) {
    if (!isActive(field)) {
      continue;
    }

    const value = valueOf(field);
    const text = value ? `${labelOf(form, field)}: ${value}` : labelOf(form, field);

    const chip = document.createElement('button');
    chip.type = 'button';
    chip.className = 'chip-token';
    chip.setAttribute('aria-label', `${host.dataset.removeLabel ?? ''} ${text}`.trim());

    const caption = document.createElement('span');
    caption.textContent = text;

    const cross = document.createElement('span');
    cross.className = 'chip-token__remove';
    cross.setAttribute('aria-hidden', 'true');
    cross.textContent = '×';

    chip.append(caption, cross);
    chip.addEventListener('click', () => {
      clear(field);
      // Удалённый чипс уносит с собой фокус — возвращаем его на панель чипсов.
      host.focus();
      form.requestSubmit();
    });

    host.append(chip);
  }

  host.hidden = host.childElementCount === 0;
}

export function initFilterChips() {
  document.querySelectorAll('[data-filter-form]').forEach(renderChips);
}
