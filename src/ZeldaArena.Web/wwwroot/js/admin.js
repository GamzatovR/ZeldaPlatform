// Точка входа скриптов админки.

import { initNav } from './nav.js';
import { initFilterChips } from './filter-chips.js';
import { initAjaxList } from './ajax-list.js';
import { initAdminActions } from './admin-actions.js';
import { initMatchConsole } from './admin-console.js';

// Сворачивать меню на узком экране можно, только когда есть кому его раскрыть:
// без JavaScript оно остаётся раскрытым (layout/_admin-shell.scss).
document.documentElement.classList.add('has-js');

initNav();
initFilterChips();
initAjaxList();
initAdminActions();
initMatchConsole();
