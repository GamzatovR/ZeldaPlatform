// Точка входа скриптов админки (docs/SPEC.md §9.4). Подключается только
// _AdminLayout, поэтому публичные страницы админских модулей не загружают.
//
// Модули общие с сайтом: раскрытие меню, фильтры и пагинация без перезагрузки,
// тосты. Каждый сам ищет свою разметку и молча ничего не делает, если её нет.

import { initNav } from './nav.js';
import { initFilterChips } from './filter-chips.js';
import { initAjaxList } from './ajax-list.js';
import { initAdminActions } from './admin-actions.js';

// Сворачивать меню на узком экране можно, только когда есть кому его раскрыть:
// без JavaScript оно остаётся раскрытым (layout/_admin-shell.scss).
document.documentElement.classList.add('has-js');

initNav();
initFilterChips();
initAjaxList();
initAdminActions();
