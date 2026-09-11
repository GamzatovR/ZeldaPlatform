// Точка входа клиентских скриптов. Подключается как ES-модуль, поэтому
// выполняется после разбора разметки — DOMContentLoaded здесь не нужен.
//
// Из 14 скриптов, подключённых страницами шаблона, в макете лежит только main.js
// (docs/design/design-system.md §1). Слайдеры, параллакс, лайтбоксы и подменённый
// курсор в проект не переносятся: они не дают ничего по критериям и мешают
// требованию §16 по скорости загрузки.
//
// Каждый модуль сам ищет свою разметку и молча ничего не делает, если её
// на странице нет, — поэтому все подключены здесь, а не страницами по отдельности.

import { initNav } from './nav.js';
import { initGoTop } from './go-top.js';
import { initFilterChips } from './filter-chips.js';
import { initAjaxList } from './ajax-list.js';
import { initCart } from './cart.js';

initNav();
initGoTop();
initFilterChips();
initAjaxList();
initCart();
