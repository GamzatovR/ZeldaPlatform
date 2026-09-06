# Журнал работ ZeldaArena

Обновляется в конце каждой фазы. План фаз — `docs/SPEC.md` §18.

## Состояние фаз

| Фаза | Содержание | Ветка | Статус |
|---|---|---|---|
| 0 | Фундамент: решение, проекты, CPM, архитектурные тесты, docker-compose, CI | `feature/phase-0-foundation` | **Готово** |
| 1 | Домен и БД: сущности, VO, события, `AppDbContext`, миграция, сид, `erd.md` | `feature/phase-1-domain` | Следующая |
| 2 | Каркас Application: порты, `Result`, `PagedResult`, MediatR + behaviors, репозитории | `feature/phase-2-application` | — |
| 3 | Identity и безопасность: роли, 2FA, письма, lockout, rate limiting | `feature/phase-3-identity` | — |
| 4 | Подписки, фича-гейт, мнимая оплата | `feature/phase-4-billing` | — |
| 5 | Дизайн-система и каркас UI | `feature/phase-5-design` | — |
| 6 | Киберспортивный модуль | `feature/phase-6-esports` | — |
| 7 | Магазин | `feature/phase-7-shop` | — |
| 8 | AJAX, фильтры, пагинация | `feature/phase-8-ajax` | — |
| 9 | Админ-панель | `feature/phase-9-admin` | — |
| 10 | SignalR, MongoDB, логирование | `feature/phase-10-realtime` | — |
| 11 | Локализация, ошибки, middleware, деплой, документация | `feature/phase-11-final` | — |

---

## Фаза 0 — Фундамент

### Сделано

**Решение и проекты.** `ZeldaArena.sln`, четыре проекта в `src/` и два в `tests/`, ссылки
расставлены по правилу зависимостей `docs/SPEC.md` §5.2. `Web` создан по шаблону `mvc`:
приложение запускается с первого дня. Шаблонные `wwwroot/lib` (bootstrap, jquery) остаются
временно и будут заменены ресурсами макета в Фазе 5.

**Конфигурация сборки.** `global.json` фиксирует SDK 10.0.400 с `rollForward: latestFeature`.
`Directory.Build.props` задаёт `net10.0`, C# 14, nullable, `EnforceCodeStyleInBuild` и
`InvariantGlobalization=false` (иначе не заработает локализация ru/en из §9.5).
`TreatWarningsAsErrors` включён только в `Domain` и `Application`, как требует `CLAUDE.md`.
`Directory.Packages.props` вводит Central Package Management и содержит версии **всего** стека
из §2, а не только используемого сейчас: дальше фазы добавляют `PackageReference` без версии.

**Архитектурные тесты — 8 тестов, все пять правил §5.2:**

| Тест | Правило |
|---|---|
| `Domain_should_not_depend_on_other_layers` | 1, по типам |
| `Domain_assembly_should_not_reference_forbidden_packages` | 1, по ссылкам сборки |
| `Application_should_not_depend_on_infrastructure_web_or_frameworks` | 2, по типам |
| `Application_assembly_should_not_reference_forbidden_packages` | 2, по ссылкам сборки |
| `Infrastructure_should_not_depend_on_web` | дополнительно |
| `Web_should_not_use_infrastructure_types_outside_composition_root` | 3 |
| `Controllers_should_only_inject_sender_localizer_and_logger` | 4 |
| `Domain_types_should_not_expose_public_setters` | 5 |

**Проверка тестов на «не вакуумность».** Правила проверены внесением реальных нарушений
с последующим откатом (в коммиты не попало):

| Проба | Результат |
|---|---|
| `PackageReference` на EF Core в `Domain`, тип не используется | проверка по типам молчит, **проверка по ссылкам сборки краснеет** — ради этого она и добавлена |
| Тип `Domain`, использующий `DbContext`, со свойством `{ get; set; }` | 3 красных теста: правило 1 по типам, правило 1 по ссылкам, правило 5 |
| Контроллер с `HttpClient` в конструкторе | красное правило 4 |
| Контроллер, использующий тип `Infrastructure` | красное правило 3 |

**Инфраструктура разработки.** `deploy/docker-compose.yml`: postgres 17, mongo 8, mailhog,
у первых двух healthcheck и именованные тома. Параметры вынесены в переменные, образец —
`deploy/.env.example`; сам `.env` в `.gitignore`.

**CI.** `.github/workflows/ci.yml`: restore → build → test → `dotnet format --verify-no-changes`
на push в `main` и на каждый PR.

**Документация.** `README.md` (быстрый старт, структура, порты), `docs/adr/ADR-0001`
(Clean Architecture и проверка правила тестами), `docs/adr/ADR-0002` (Shouldly, MediatR 12.x,
Mapster — выбор по лицензии), индекс ADR.

### Проверено

| Проверка | Результат |
|---|---|
| `dotnet build -c Release` | успешно, 0 предупреждений, 0 ошибок |
| `dotnet test` | 9 тестов пройдено (8 архитектурных + smoke), 0 упало |
| `dotnet format --verify-no-changes` | чисто |
| Негативные пробы на все пять правил | каждая даёт красный тест, см. таблицу выше |
| `docker compose config` | синтаксис валиден |
| `docker compose up -d` | **не проверено: демон Docker не был запущен во время фазы** |
| `dotnet run --project src/ZeldaArena.Web` | см. раздел «Открытые вопросы» |

### Принятые решения

**MediatR 12.5.0, а не 14.x** — последняя версия под Apache-2.0; с 13-й лицензия коммерческая.
Обоснование в ADR-0002.

**PostgreSQL наружу портом 5433.** Порт 5432 на машине разработки занят локально
установленным сервером. Внутри compose-сети всё по-прежнему `postgres:5432`, наружу — 5433;
строка подключения в `appsettings.Development.json` это учитывает.

**`GenerateDocumentationFile=true` для всех проектов.** Иначе анализатор IDE0005 («лишний
using») не работает при сборке. `CS1591` (нет XML-комментария у публичного члена) подавлен —
комментировать требуется по смыслу, а не поголовно.

**`.sln`, а не `.slnx`.** Шаблон .NET 10 по умолчанию создаёт XML-формат `slnx`; выбран
классический `.sln` ради совместимости с привычным инструментарием.

### Входные условия Фазы 1

- Сущности, value objects, enum'ы и доменные события из `docs/SPEC.md` §5.3 и §6.
- `AppDbContext`, конфигурации через `IEntityTypeConfiguration<T>`, первая миграция,
  идемпотентный сид по §6.
- `docs/erd.md`, unit-тесты инвариантов домена.
- **Sentinel-тест.** По §18 он утверждает, что сборки `Domain` и `Application` содержат типы.
  Формулировать его надо аккуратно: в обеих сборках уже лежат якоря
  `DomainAssemblyReference` и `ApplicationAssemblyReference`, поэтому проверка «есть хоть
  один тип» пройдёт вхолостую. Тест должен требовать наличие наследников `BaseEntity`
  в `Domain` и реализаций `IRequest<>` / хендлеров в `Application`.

### Открытые вопросы

- `docker compose up -d` и MailHog на `localhost:8025` не проверены вживую: во время фазы
  демон Docker не отвечал. Проверить при первом запуске Фазы 1, когда понадобится база.
- Запуск `dotnet run` вживую не выполнялся; сборка `Web` и вызов `AddInfrastructure()`
  проверены компиляцией. Проверить при первом запуске.
