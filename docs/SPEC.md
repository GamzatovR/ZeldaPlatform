# Техническое задание: киберспортивная платформа «ZeldaArena» (финальная версия)

**Платформа:** .NET 10 (LTS, поддержка до ноября 2028) — фиксируется в `global.json`
**Стек:** ASP.NET Core MVC + Razor Pages · EF Core 10 · PostgreSQL · MongoDB · SignalR · Clean Architecture
**Версия:** 3.1 — финальная, объём оптимизирован под сжатые сроки при сохранении максимального балла по всем 16 критериям.

> **Как использовать.** Файл кладётся в репозиторий как `docs/SPEC.md`, `CLAUDE.md` — в корень, `.claude/settings.json` — в папку `.claude/`.
> Реализация идёт строго по плану работ (§18), одна фаза = одна сессия Claude Code = один Pull Request.
> Перед началом каждой фазы — режим Plan; код не пишется до утверждения плана.

---

## 1. Принцип экономии времени

Объём сознательно урезан. Всё, что не влияет на баллы, выброшено. Чего в проекте **нет** и не должно появиться:
Hangfire, Redis, кредитные балансы, тикеты поддержки, прогнозы на матчи, отзывы на товары, бренды, галереи изображений, иерархия категорий, разбивка матча по картам, адресная книга, теги новостей.

Что **сохранено обязательно**, потому что напрямую оценивается: Clean Architecture с отдельными проектами, система подписок на фичах, 3 роли и политики, ≥5 AJAX, фильтрация в URL, AJAX-пагинация, двухуровневая валидация, MongoDB под реальные задачи, SignalR, локализация ru/en, аудит действий, страницы ошибок, middleware, ≥6 таблиц.

**Правило при нехватке времени:** резать можно только из раздела «Приоритет B» (§17). Всё из «Приоритета A» обязательно.

---

## 2. Технологический стек

Версии фиксируются в `global.json` и `Directory.Packages.props` (Central Package Management).

| Слой | Технология |
|---|---|
| Платформа | .NET 10 (LTS), C# 14, `net10.0` во всех проектах |
| Веб | ASP.NET Core MVC (контроллеры + Views) **и** Razor Pages (область Identity/Account) |
| ORM | EF Core 10, Code First, миграции |
| Основная БД | PostgreSQL (Npgsql) |
| NoSQL | MongoDB (`MongoDB.Driver`) |
| Кэш | `IMemoryCache` |
| Identity | ASP.NET Core Identity + TOTP 2FA |
| CQRS | MediatR + pipeline behaviors |
| Валидация | FluentValidation (сервер) + DataAnnotations/Unobtrusive (клиент) |
| Маппинг | Mapster |
| Логи | Serilog: Console + Rolling File + MongoDB sink |
| Real-time | SignalR |
| Фон | `BackgroundService` (без Hangfire) |
| Санитизация | `Ganss.Xss` (HtmlSanitizer) |
| Бандлинг | `LigerShark.WebOptimizer.Core` |
| Фронтенд | Стили и скрипты выданного HTML-шаблона; SCSS с токенами; ванильный JS (ES-модули); jQuery только ради `jquery-validation-unobtrusive` |
| Тесты | xUnit, **Shouldly** (BSD), NSubstitute, NetArchTest.Rules |
| Деплой | Docker + docker-compose + Nginx |

**Про библиотеку утверждений:** используется **Shouldly**, не FluentAssertions. Начиная с версии 8 FluentAssertions распространяется по коммерческой лицензии Xceed, а последняя свободная ветка 7.x заморожена и обновляться не будет. Начинать новый LTS-проект с замороженной зависимости не следует.

**Запрещено:** бизнес-логика в контроллерах; `DbContext` в `Web`; `dynamic`; `ViewBag` для бизнес-данных; конкатенация SQL; секреты в репозитории.

## 3. Матрица соответствия критериям

| № | Критерий | Балл | Чем закрывается |
|---|---|---|---|
| 1 | Архитектура | **5** — Clean Architecture, отдельные проекты | 4 проекта + 2 тестовых, правило зависимостей, автотесты архитектуры (§5) |
| 2 | Дизайн и ТЗ | **5** — адаптив, макеты, SPA-подход | Токены из макета, mobile-first, partial-навигация + History API (§9.1) |
| 3 | AJAX | **5** — 5+ запросов | 10 сценариев (§10.1) |
| 4 | Админ-панель | **5** — 6+ страниц | 13 страниц (§9.4) |
| 5 | Авторизация и роли | **5** — Identity, 3 роли, политики | Admin / Moderator / User + политики + динамические фича-политики (§8) |
| 6 | Страницы | **5** — 6+, ≥3 динамических | 17 публичных, 13 динамических (§9.3) |
| 7 | Фильтрация | **5** — состояние в URL, переживает F5 | Единый механизм на 3 списках (§10.2) |
| 8 | Валидация | **5** — клиент + сервер | DataAnnotations + Unobtrusive + Remote / FluentValidation (§15) |
| 9 | Пагинация | **5** — AJAX с сохранением состояния | Единый tag helper, состояние в URL (§10.3) |
| 10 | Структура БД | **5** — 6+ таблиц, нормализовано | 30 таблиц, 3НФ (§6) |
| 11 | SignalR | **5** — полноценный чат/уведомления | Live-счёт матча + чат матча + персональные уведомления (§11) |
| 12 | Локализация | **5** — 2+ языка, всё локализовано | ru/en: UI, валидация, письма, ошибки, enum'ы + переводы контента в БД (§9.5) |
| 13 | MongoDB | **5** — реальные задачи | Лента событий матча, чат, аудит, логи + альтернативное хранилище новостей (§12) |
| 14 | Логирование | **5** — действия пользователя | Serilog + `AuditBehavior` + просмотр в админке (§13) |
| 15 | Маршруты/ошибки | **5** — свои страницы ошибок | 400/401/403/404/429/500, локализованные (§14.2) |
| 16 | Middleware | **5** — минимум один полезный | 4 собственных (§14.1) |

Дополнительно оценивается на защите: точки расширения, замена хранилища, добавление нового UI, SOLID, расширяемость подписок, безопасность ввода, 2FA, git и таск-трекер, деплой со сжатием и минификацией.

---

## 4. Предметная область и главный сценарий

**ZeldaArena** — портал по киберспортивной дисциплине Zelda: турниры, матчи с live-счётом, команды, игроки, статистика, расписание, новости и магазин игровой периферии.

**Ключевая платная функция:** создание собственной киберспортивной команды. Обычный пользователь только просматривает; оформивший подписку получает право создать команду, стать её капитаном, редактировать профиль команды и управлять составом. Право проверяется через фичу `team.create`.

Вторая платная фича — `stats.advanced` (расширенная статистика команд и игроков). Она нужна не ради функциональности, а чтобы система подписок была реально расширяемой: на защите демонстрируется, как эта фича вынимается из тарифа Pro в отдельный платный тариф **через админку, без единой строки кода** — это прямое требование задания.

---

## 5. Архитектура

### 5.1. Структура решения

```
ZeldaArena.sln
├── src/
│   ├── ZeldaArena.Domain/          # сущности, VO, enum'ы, доменные события. Ноль внешних зависимостей
│   ├── ZeldaArena.Application/     # CQRS-сценарии, DTO, ПОРТЫ (интерфейсы), валидаторы, behaviors
│   ├── ZeldaArena.Infrastructure/  # EF Core, Identity, Mongo, почта, файлы, SignalR-адаптер, фоновая служба
│   └── ZeldaArena.Web/             # MVC + Razor Pages: контроллеры, вьюхи, VM, middleware, хаб, ресурсы
├── tests/
│   ├── ZeldaArena.UnitTests/          # домен + Application
│   └── ZeldaArena.ArchitectureTests/  # проверка правила зависимостей
├── docs/  (SPEC.md, architecture.md, erd.md, extension-points.md, design/, adr/, PROGRESS.md)
├── deploy/ (Dockerfile, docker-compose.yml, nginx.conf)
├── .claude/settings.json
├── CLAUDE.md
└── README.md
```

### 5.2. Правило зависимостей

```
Domain  ←  Application  ←  Infrastructure
                ↑                ↑
                └──── Web ───────┘  (Infrastructure — только ради AddInfrastructure() в Program.cs)
```

Проверяется тестами `ZeldaArena.ArchitectureTests` (NetArchTest):
1. `Domain` не зависит от `Application` / `Infrastructure` / `Web` / EF Core;
2. `Application` не зависит от `Infrastructure` / `Web` / EF Core / ASP.NET Core;
3. типы `ZeldaArena.Infrastructure.*` не встречаются в `Web`, кроме `Program.cs`;
4. контроллеры принимают в конструктор только `ISender`, `IStringLocalizer`, `ILogger`;
5. доменные сущности не имеют публичных сеттеров.

Красный тест = красная сборка. Это доказательство 5 баллов по критерию 1.

Начиная с Фазы 1 к правилам добавляется **sentinel-тест**: сборки `Domain` и `Application` должны содержать типы. Без него архитектурные проверки могли бы молча проходить на пустой сборке, и нарушение осталось бы незамеченным.

### 5.3. Организация проектов

**Domain**
```
Common/       BaseEntity, IAuditableEntity, ISoftDeletable, DomainEvent, ValueObject, Result<T>
Esports/      Tournament, TournamentTeam, Team, Player, RosterEntry, Match, PlayerMatchStats,
              NewsArticle, Comment, Follow
Shop/         Product, ProductCategory, Cart, CartItem, Order, OrderItem
Billing/      Plan, Feature, PlanFeature, Subscription, Payment
Common/       ContentTranslation, AppSetting, Notification
ValueObjects/ Money, Slug, CountryCode
Enums/        MatchStatus, TournamentStatus, SubscriptionStatus, PaymentStatus, OrderStatus,
              PlayerRole, CommentTargetType, FollowTargetType, NotificationType
Events/       MatchScoreChangedEvent, MatchFinishedEvent, SubscriptionActivatedEvent,
              SubscriptionExpiredEvent, OrderPlacedEvent, PaymentConfirmedEvent
```
Инварианты — в сущностях, публичных сеттеров нет:
`Match.UpdateScore(a,b)` (нельзя менять завершённый матч, нельзя превысить `BestOf`), `Match.Finish()`, `Team.Create(ownerId,…)`, `RosterEntry` (игрок не может состоять в двух командах одновременно), `Cart.AddItem()` (проверка остатка), `Order.Cancel()`, `Subscription.IsActiveAt(date)`, `Payment.Confirm(code)`, `Money` (не отрицательна, валюты не смешиваются).

**Application** — вертикальные срезы:
```
Common/Interfaces/  IRepository<T>, IReadRepository<T>, INewsRepository, IUnitOfWork,
                    ICurrentUserService, IDateTimeProvider, IEmailSender, IEntitlementService,
                    IAuditLogWriter, IRealtimeNotifier, IMatchEventStore, IChatMessageStore,
                    IFileStorage, IPaymentGateway
Common/Behaviors/   ValidationBehavior, AuditBehavior, LoggingBehavior, TransactionBehavior
Common/Models/      PagedResult<T>, Result, Error, FilterBase
Features/           Tournaments/, Matches/, Teams/, Players/, News/, Shop/, Cart/, Orders/,
                    Subscriptions/, Payments/, Comments/, Follows/, Admin/
```

**Infrastructure**
```
Persistence/Ef/     AppDbContext, Configurations/, EfRepository<T>, EfNewsRepository,
                    Interceptors/, Migrations/, Seed/
Persistence/Mongo/  MongoContext, MongoAuditLogWriter, MongoChatMessageStore,
                    MongoMatchEventStore, MongoNewsRepository
Identity/           ApplicationUser, CurrentUserService, EntitlementService,
                    FeaturePolicyProvider, FeatureAuthorizationHandler, IdentitySeeder
Email/              SmtpEmailSender (MailHog в разработке), шаблоны писем
Payments/           FakePaymentGateway  ← мнимая оплата с кодом подтверждения
Files/              LocalFileStorage
Realtime/           SignalRNotifier : IRealtimeNotifier
BackgroundJobs/     SubscriptionExpirationService : BackgroundService
Logging/            SerilogConfiguration
DependencyInjection.cs
```

**Web**
```
Program.cs
Areas/Admin/     Controllers, Views
Areas/Identity/  Razor Pages (Login, Register, Manage/*, 2FA)
Areas/Api/       [ApiController] — только AJAX и задел под мобильный клиент
Controllers/     Home, Tournaments, Matches, Teams, Players, News, Shop, Cart, Checkout,
                 Orders, Account, Error
Views/, ViewComponents/, TagHelpers/, Middleware/, Hubs/LiveHub.cs, Resources/, wwwroot/
```

### 5.4. Точки расширения (описать в `docs/extension-points.md`)

| № | Точка | Механизм | Как расширяется |
|---|---|---|---|
| EP-1 | **Замена хранилища** | `INewsRepository` имеет **две рабочие реализации** — `EfNewsRepository` (PostgreSQL) и `MongoNewsRepository`, выбор через `Persistence:NewsProvider` в конфиге. Остальные сущности за `IRepository<T>` | Написать реализацию порта и зарегистрировать в `DependencyInjection.cs`. Ноль изменений в Domain, Application, Web. **Демонстрируется вживую переключением конфига** |
| EP-2 | **Новый UI (мобильный)** | Логика в Application, `Areas/Api` содержит только `mediator.Send(...)` | Создать проект `ZeldaArena.MobileApi` со ссылкой на Application + Infrastructure. Ядро не трогается. Порядок описать в `architecture.md` |
| EP-3 | **Новая платная функция** | Таблица `Features` + атрибут `[RequireFeature("code")]` + tag helper `<feature-gate>` | Добавить строку в БД через админку, повесить атрибут на действие. **Ноль изменений ядра** |
| EP-4 | **Выделение функции в отдельную услугу** | `PlanFeatures` — many-to-many, редактируется из админки | Снять `stats.advanced` с тарифа Pro, создать тариф «Analytics» с этой фичей. Без деплоя |
| EP-5 | **Параметризованная фича** | `PlanFeature.Value` (например, процент скидки или лимит команд) | Изменить значение в админке |
| EP-6 | **Новый платёжный провайдер** | `IPaymentGateway` + резолвер по строковому ключу | Класс + регистрация |
| EP-7 | **Новый язык** | `IStringLocalizer` + `.resx` + `SupportedCultures` + `ContentTranslations` | `.resx` и код культуры в конфиге |
| EP-8 | **Новое сквозное поведение** | MediatR pipeline behaviors | Дописать behavior и зарегистрировать |

### 5.5. SOLID — примеры для защиты (в `architecture.md`)

* **SRP** — хендлер `UpdateMatchScoreCommandHandler` только меняет счёт; рассылку в SignalR делает обработчик доменного события.
* **OCP** — новая платная фича добавляется строкой в БД, новый провайдер оплаты — новым классом.
* **LSP** — `EfNewsRepository` и `MongoNewsRepository` проходят один набор контрактных тестов.
* **ISP** — раздельные `IReadRepository<T>` и `IRepository<T>`; узкие `IRealtimeNotifier` и `IEmailSender`.
* **DIP** — порты в Application, реализации в Infrastructure, связывание только в composition root.

---

## 6. Модель данных (30 таблиц, PostgreSQL)

**Identity (7):** `AspNetUsers` (расширен: `DisplayName, AvatarPath, PreferredCulture, CountryCode, CreatedAt, LastLoginAt, IsBlocked`), `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`.

> **Версии платформы.** .NET 10 (LTS, до ноября 2028). .NET 8 и .NET 9 снимаются с поддержки 10 ноября 2026 года,
> поэтому новый проект на них начинать не следует. Версия SDK фиксируется в `global.json`;
> `TargetFramework` во всех проектах — `net10.0`; язык — C# 14.
> Перед Фазой 0 проверить `dotnet --list-sdks`: должен быть установлен SDK 10.x.

**Киберспорт (10)**

| Таблица | Поля |
|---|---|
| `Tournaments` | `Id, Slug(uniq), Name, Description, Tier, Region, PrizePool, Currency, StartsAt, EndsAt, Status, RulesHtml, LogoPath, BannerPath, IsFeatured, CreatedAt` |
| `TournamentTeams` | `TournamentId, TeamId, Seed, Placement?` — PK составной |
| `Teams` | `Id, Slug(uniq), Name, Tag, LogoPath, CountryCode, Region, FoundedAt, Rating, Description, OwnerUserId?, IsApproved, CreatedAt` — `OwnerUserId` заполняется, если команду создал подписчик |
| `Players` | `Id, Slug(uniq), Nickname, FirstName, LastName, CountryCode, BirthDate, Role, AvatarPath, Bio` |
| `RosterEntries` | `Id, TeamId, PlayerId, Role, JoinedAt, LeftAt?, IsActive` — историчный состав |
| `Matches` | `Id, TournamentId, TeamAId, TeamBId, ScheduledAt, StartedAt?, EndedAt?, Status, BestOf, ScoreA, ScoreB, WinnerTeamId?, StreamUrl?, RowVersion` |
| `PlayerMatchStats` | `Id, MatchId, PlayerId, TeamId, Kills, Deaths, Assists, Damage, Rating` |
| `NewsArticles` | `Id, Slug(uniq), Title, Summary, BodyHtml, CoverPath, AuthorId, PublishedAt, IsPublished, ViewCount` |
| `Comments` | `Id, UserId, TargetType, TargetId, Text, CreatedAt, IsApproved, IsDeleted` |
| `Follows` | `Id, UserId, TargetType, TargetId, CreatedAt` — uniq `(UserId, TargetType, TargetId)` |

**Магазин (6)**

| Таблица | Поля |
|---|---|
| `Products` | `Id, Slug(uniq), Sku(uniq), Name, Description, CategoryId, Price, Currency, StockQuantity, ImagePath, IsActive, CreatedAt, RowVersion` |
| `ProductCategories` | `Id, Slug, Name` — плоский справочник |
| `Carts` | `Id, UserId?, AnonymousId?, CreatedAt, UpdatedAt` |
| `CartItems` | `Id, CartId, ProductId, Quantity, PriceSnapshot` |
| `Orders` | `Id, Number(uniq), UserId, Status, Subtotal, DiscountAmount, Total, Currency, Recipient, Phone, Country, City, Street, PostalCode, PlacedAt, PaidAt?, CanceledAt?, RowVersion` — адрес хранится снапшотом в заказе |
| `OrderItems` | `Id, OrderId, ProductId, ProductNameSnapshot, UnitPrice, Quantity` |

**Подписки и оплата (5)**

| Таблица | Поля | Примечания |
|---|---|---|
| `Plans` | `Id, Code(uniq), Name, Description, Price, Currency, DurationDays, IsActive, SortOrder, RowVersion` | |
| `Features` | `Id, Code(uniq), Name, Description, IsActive` | Ключ всей системы доступа |
| `PlanFeatures` | `PlanId, FeatureId, Value?` | PK составной; `Value` — параметр фичи (EP-5) |
| `Subscriptions` | `Id, UserId, PlanId, StartsAt, EndsAt, Status, AutoRenew, PriceSnapshot, CanceledAt?, CreatedAt, RowVersion` | Индексы `(UserId, Status)`, `(EndsAt)` |
| `Payments` | `Id, UserId, Purpose(Subscription/Order), SubscriptionId?, OrderId?, Amount, Currency, Status, CardLast4, CardBrand, ConfirmationEmail, ConfirmationCodeHash, ConfirmationExpiresAt, ConfirmationAttemptsLeft, IdempotencyKey(uniq), CreatedAt, PaidAt?, FailureReason?` | **Номер карты и CVV не хранятся никогда** |

**Общее (2):** `ContentTranslations` (`Id, EntityType, EntityId, CultureCode, FieldName, Value` — uniq по всем пяти; универсальная таблица переводов для турниров, новостей, товаров), `AppSettings` (`Key, Value, UpdatedAt`).

**Требования к схеме:** явные FK, `Restrict` на удаление команд и турниров (история матчей не должна стираться каскадом); уникальные индексы на `Slug`, `Code`, `Sku`, `Number`; составные индексы под каждый фильтр; `RowVersion` на `Matches`, `Products`, `Orders`, `Subscriptions`, `Plans`; деньги — `numeric(18,2)`; даты — `timestamptz` / `DateTimeOffset` в UTC; soft delete через глобальный query filter; конфигурации только через `IEntityTypeConfiguration<T>`.

**Сид (идемпотентный):** 3 роли; администратор из переменных окружения; 3 тарифа; 2 фичи; 4 турнира (1 идёт, 1 прошёл, 2 предстоящих); 12 команд; 60 игроков с составами; 40 матчей во всех статусах, минимум 1 в статусе Live; 10 новостей; 3 категории и 24 товара; 3 демо-пользователя (с активной подпиской, с истёкшей, без). Данных должно хватать, чтобы фильтрация и пагинация были видны.

---

## 7. Подписки и разграничение доступа

### 8.1. Принцип

Доступ определяется **фичами**, не ролями и не тарифами. Тариф — просто набор фич. Код никогда не спрашивает «какой у пользователя тариф», только «есть ли фича `team.create`». Это и даёт требуемую расширяемость.

### 8.2. Фичи и тарифы

| Код фичи | Что открывает |
|---|---|
| `team.create` | Создание своей команды, управление её профилем и составом |
| `stats.advanced` | Расширенная статистика команд и игроков (графики формы, разбивка показателей) |

| Тариф | Цена | Срок | Фичи |
|---|---|---|---|
| `free` | 0 | — | нет |
| `pro-month` | 299 ₽ | 30 дней | `team.create`, `stats.advanced` |
| `pro-year` | 2 490 ₽ | 365 дней | `team.create`, `stats.advanced` |

Тарифы различаются сроком и стоимостью — требование задания выполнено. Добавление третьей фичи или четвёртого тарифа = записи в БД через админку.

### 7.3. Три уровня проверки

**(1) Сервис — единственный источник истины** (`Application/Common/Interfaces`):
```csharp
public interface IEntitlementService
{
    Task<bool> HasFeatureAsync(Guid userId, string featureCode, CancellationToken ct = default);
    Task<EntitlementSet> GetEntitlementsAsync(Guid userId, CancellationToken ct = default);
    Task<string?> GetFeatureValueAsync(Guid userId, string featureCode, CancellationToken ct = default);
}
```
Реализация: активные подписки (`Status == Active && EndsAt > now`) → объединение `PlanFeature` → кэш в `IMemoryCache` с TTL 5 минут, инвалидация по доменным событиям подписки и при правке тарифа/фичи в админке.

**(2) Декларативно в MVC.** `IAuthorizationPolicyProvider` собирает политику по имени `Feature:{code}` на лету:
```csharp
[RequireFeature(FeatureCodes.TeamCreate)]
public IActionResult Create() { ... }
```
Отказ → редирект на `/account/subscription?required=team.create` с объяснением, какой тариф нужен, а не голый 403.

**(3) В разметке** — tag helper `<feature-gate feature="stats.advanced">…</feature-gate>`, скрывающий блок или показывающий призыв оформить подписку. Сервер перепроверяет всегда.

### 7.4. Роль Premium

Права дают фичи. Роль `Premium` выдаётся и снимается автоматически обработчиками `SubscriptionActivatedEvent` / `SubscriptionExpiredEvent` и используется **только для отображения** (бейдж у ника, метка в чате). Для проверки доступа роль не используется никогда — зафиксировать в `docs/adr/ADR-0005` с обоснованием.

Итоговый набор ролей: `Admin`, `Moderator`, `User` + техническая `Premium`.

### 7.5. Жизненный цикл

1. `/account/subscription` → выбор тарифа → создаётся `Payment` (`Purpose = Subscription`, `Status = Pending`).
2. Оплата по схеме §7.6.
3. После подтверждения — в одной транзакции: `Payment.Succeeded` + создание/продление `Subscription` → `SubscriptionActivatedEvent` → выдача роли Premium, сброс кэша прав, уведомление через SignalR, письмо.
4. `SubscriptionExpirationService` (`BackgroundService`, раз в час) помечает истёкшие подписки, снимает роль, шлёт уведомление.
5. Отмена: `AutoRenew = false`, доступ сохраняется до `EndsAt`.

### 7.6. Мнимая оплата с подтверждением по коду

Единый механизм для подписок и заказов магазина, реализация — `FakePaymentGateway : IPaymentGateway`.

**Шаг 1. Ввод реквизитов.** Форма: номер карты, срок действия (месяц/год), CVV, email для чека. Валидация на клиенте и сервере: номер по алгоритму Луна, срок не в прошлом, CVV — 3 цифры, email — корректный формат.

**Шаг 2. `POST /api/payments` (AJAX).** Сервер:
* сохраняет `Payment` со статусом `Pending`, записывая **только** `CardLast4` и `CardBrand` (определяется по BIN: 4… → Visa, 5… → Mastercard, 2… → МИР);
* генерирует 6-значный код криптографическим ГСЧ, сохраняет **хеш** кода (`ConfirmationCodeHash`), `ConfirmationExpiresAt = now + 10 мин`, `ConfirmationAttemptsLeft = 5`;
* отправляет письмо с кодом на указанный email через `IEmailSender`;
* возвращает `{ paymentId, maskedEmail }`.

**Шаг 3.** Страница **без перезагрузки** показывает поле ввода кода, таймер до истечения и кнопку «Отправить код повторно» (не чаще раза в 60 секунд).

**Шаг 4. `POST /api/payments/{id}/confirm` (AJAX).** Сервер сравнивает хеш введённого кода:
* совпал и не истёк → `Payment.Confirm()` → активация подписки или оформление заказа → редирект на страницу успеха, письмо-«чек»;
* не совпал → `ConfirmationAttemptsLeft--`, сообщение с числом оставшихся попыток;
* попытки кончились или код истёк → `Payment.Failed`, предложение начать оплату заново.

**Безопасность (важно для защиты):** полный номер карты и CVV не сохраняются в БД, не пишутся в логи, не попадают в аудит и не возвращаются клиенту. Сам код хранится только в виде хеша. На эндпоинты оплаты и подтверждения навешан rate limiting. Операция идемпотентна по `IdempotencyKey`. В `docs/adr` зафиксировать: реальный приём платежей потребовал бы PCI DSS и токенизации на стороне провайдера — интерфейс `IPaymentGateway` для этого и предусмотрен (EP-6).

**Почта в разработке:** контейнер MailHog в `docker-compose`, веб-интерфейс на `localhost:8025` — письмо с кодом видно вживую, это хорошо смотрится на демонстрации. В `Development` код дополнительно пишется в лог.

---

## 8. Аутентификация и безопасность аккаунта

### 8.1. Роли и политики

| Роль | Права |
|---|---|
| `Admin` | Всё: пользователи, роли, тарифы, фичи, заказы, аудит, настройки |
| `Moderator` | Турниры, матчи (включая пульт счёта), команды, игроки, новости, модерация комментариев. **Нет** доступа к пользователям, тарифам, платежам, настройкам |
| `User` | Публичная часть, личный кабинет, заказы, подписка, своя команда при наличии фичи |
| `Premium` | Техническая, только для отображения |

Политики: `AdminOnly`, `ModeratorOrAdmin`, `CanManageCatalog`, `CanManageBilling`, `CanModerateComments`, `CanViewAuditLog`, `EmailConfirmed` + динамические `Feature:*`. Доступ в Area `Admin` — через соглашение на всю область.

### 8.2. Требования безопасности (прямое требование задания)

* **2FA** — TOTP (Google/Microsoft Authenticator), QR-код, 10 recovery-кодов, «запомнить устройство» на 30 дней. Для роли `Admin` 2FA **обязательна**: без неё вход в Area `Admin` заблокирован фильтром.
* **Смена пароля** — только с вводом текущего; после смены `UpdateSecurityStampAsync` (разлогин прочих сессий) + письмо-уведомление.
* **Смена email** — подтверждение по ссылке на новый адрес + уведомление на старый. До подтверждения email не меняется.
* **Восстановление пароля** — одноразовый токен, TTL 30 минут, ответ формы всегда одинаковый (нет user enumeration).
* **Пароль** — минимум 10 символов, требования Identity.
* **Lockout** — 5 неудачных попыток → 15 минут.
* **Rate limiting** — login, register, forgot-password, оплата, подтверждение кода, комментарии, чат.
* Подтверждение email обязательно перед покупкой подписки и оформлением заказа.
* `SecurityStampValidationInterval = 5 минут`; cookie `HttpOnly`, `Secure`, `SameSite=Lax`.
* В аудит пишутся: вход, выход, неудачный вход, регистрация, смена пароля и email, включение 2FA, изменение ролей, покупка подписки, оформление и отмена заказа, изменение счёта матча, создание команды.

---

## 9. Пользовательский интерфейс

### 10.1. Дизайн

* Макет выдан в виде готового HTML-шаблона и лежит в `docs/design/` (структура и правила чтения — в `CLAUDE.md`, раздел «Работа с макетами»). Перед вёрсткой изучить `docs/design/README.md`, файлы `pages/*.html` и авторские css, затем составить `docs/design/design-system.md`: палитра, шрифты и шкала размеров, отступы, радиусы, тени, стили кнопок/карточек/бейджей/форм, сетка. Вендорские и минифицированные файлы шаблона не читать. Токены переносятся в `wwwroot/scss/_tokens.scss` как CSS-переменные; компоненты используют только их, без хардкод-цветов. Из скриптов шаблона в проект переносится только то, что реально используется.
* **Mobile-first**, брейкпоинты 576/768/992/1200. Проверка на 360, 768, 1440 px.
* **SPA-подход:** вкладки матчей на странице турнира, фильтры и пагинация списков, корзина, вкладки статистики, таблицы админки обновляются через `fetch` + подстановку partial view + `history.pushState`. Полная перезагрузка только при смене раздела. Обязательны скелетон-лоадеры, обработка `popstate`, `AbortController` для отмены устаревших запросов.
* **Прогрессивное улучшение:** без JS фильтры работают как обычная GET-форма, корзина — как обычные POST-формы. Это нужно, чтобы фильтрация переживала перезагрузку.
* Доступность: семантика, `alt`, контраст ≥ 4.5:1, фокус-стили, `aria-live` для тостов и live-счёта.

### 10.2. Общие компоненты

ViewComponents: `Header` (поиск, корзина, колокольчик, бейдж подписки, переключатель языка), `LiveTicker` (бегущая строка live-матчей), `UpcomingMatches`, `NewsFeed`, `ProductCard`, `MiniCart`, `Footer`.
TagHelpers: `<feature-gate>`, `<pagination>`, `<sortable-header>`, `<active-route>`, `<match-score>`.

### 10.3. Публичные страницы (17, динамических 13)

| # | Маршрут | Содержимое | Дин. |
|---|---|---|---|
| 1 | `/` | Трейлер игры, live и ближайшие матчи, лента новостей, витрина 4–6 товаров, топ-команды, призыв оформить подписку | ✔ |
| 2 | `/schedule` | Расписание турниров и матчей: группировка по дням, фильтр по турниру и статусу, отметка live | ✔ |
| 3 | `/tournaments` | Список турниров: фильтры (статус, регион, даты, призовой), сортировка, AJAX-пагинация | ✔ |
| 4 | `/tournaments/{slug}` | Турнир: название, даты, призовой фонд, регламент; **вкладки матчей Все / Предстоящие / Прошедшие** (AJAX, состояние в URL); команды-участницы с рейтингом | ✔ |
| 5 | `/matches/{id}` | Матч: счёт в реальном времени, составы команд, статистика игроков, лента событий, чат матча, комментарии | ✔ |
| 6 | `/teams` | Список команд: фильтры (регион, рейтинг, поиск), пагинация | ✔ |
| 7 | `/teams/{slug}` | Команда: статистика, форма последних матчей, состав, история матчей, кнопка Follow. Блок расширенной статистики за фичей `stats.advanced` | ✔ |
| 8 | `/teams/create` + `/account/my-team` | **Создание и управление своей командой — только по подписке** (`team.create`): профиль, логотип, состав | ✔ |
| 9 | `/players` | Список игроков: фильтры (роль, страна, команда), поиск, пагинация | ✔ |
| 10 | `/players/{slug}` | Игрок: профиль, текущая и прошлые команды, статистика, последние матчи | ✔ |
| 11 | `/news/{slug}` | Новость с комментариями | ✔ |
| 12 | `/shop` | Магазин: фильтры (категория, цена от–до, наличие), сортировка, пагинация, добавление в корзину без перезагрузки | ✔ |
| 13 | `/cart` | Корзина: изменение количества, удаление, пересчёт итога без перезагрузки, проверка остатков | ✔ |
| 14 | `/checkout` | Оформление: адрес доставки, список выбранных товаров, платёжные реквизиты, подтверждение кодом из письма (§7.6) | ✔ |
| 15 | `/orders` | История заказов с фильтром по статусу и пагинацией; детали заказа | ✔ |
| 16 | `/account/login`, `/account/register` | **Вход и регистрация — макет есть в шаблоне.** Реализуются как Razor Pages в Area Identity, но верстаются строго по макету. Сюда же по тем же стилям: восстановление пароля, подтверждение email, ввод кода 2FA | ✔ |
| 17 | `/account`, `/account/subscription`, `/account/security` | Профиль; тарифы и оформление подписки; 2FA, смена пароля и email. **Макета нет** — собирается из токенов и компонентов страницы входа и внутренних блоков шаблона | ✔ |

Плюс страницы Identity (Razor Pages) и страницы ошибок.

### 9.4. Админ-панель — Area `Admin` (13 страниц, нужно ≥6)

**Приоритет A (обязательно):**
1. `/admin` — дашборд: число пользователей, активных подписок, выручка, ближайшие матчи, последние события аудита.
2. `/admin/tournaments` — CRUD турниров + состав участников.
3. `/admin/matches` — CRUD матчей + **пульт счёта**: изменение счёта, старт и завершение матча; каждое действие мгновенно уходит зрителям через SignalR.
4. `/admin/teams` — CRUD команд, одобрение команд, созданных пользователями, управление составами.
5. `/admin/players` — CRUD игроков.
6. `/admin/products` — CRUD товаров и категорий, остатки, цены.
7. `/admin/orders` — заказы: фильтры, смена статуса, детали.
8. `/admin/plans` — CRUD тарифов + привязка фич чекбоксами (AJAX).
9. `/admin/features` — CRUD фич. **Здесь происходит выделение функции в отдельную услугу (EP-3/EP-4).**
10. `/admin/users` — пользователи: фильтры, роли, блокировка.

**Приоритет B (если останется время):**
11. `/admin/news` — CRUD новостей (иначе новости только из сида).
12. `/admin/moderation` — очередь комментариев.
13. `/admin/audit` — просмотр аудит-лога из MongoDB с фильтрами и пагинацией. *(Рекомендуется не резать: это самая наглядная демонстрация критериев 13 и 14.)*

Единый `_AdminLayout` с боковым меню и тостами. Все таблицы: серверная фильтрация, сортировка по клику на заголовок, AJAX-пагинация, состояние в URL.

### 9.5. Локализация

* Языки **ru** (по умолчанию) и **en**.
* Провайдеры культуры: query (`?culture=en`) → cookie → `Accept-Language`.
* Ресурсы: `Resources/Views/…`, `Resources/Controllers/…`, `SharedResource.{ru,en}.resx`, `DataAnnotationLocalizerProvider` для сообщений валидации.
* Локализуется **всё**: меню, кнопки, заголовки, сообщения валидации (в том числе клиентские), письма (код оплаты, чек, уведомления), страницы ошибок, названия статусов матчей, заказов и подписок (enum'ы), тосты, уведомления SignalR (передаётся ключ + payload, перевод на клиенте).
* Контент из БД — через `ContentTranslations` с фолбэком на язык по умолчанию: названия и описания турниров, заголовки новостей, названия товаров.
* Даты, числа и валюта форматируются по текущей культуре; хранение — UTC.
* Переключатель языка сохраняет текущий URL и все параметры фильтра.

---

## 10. AJAX, фильтрация, пагинация

### 10.1. AJAX-сценарии (нужно 5+, делаем 10)

| # | Сценарий | Эндпоинт |
|---|---|---|
| 1 | Фильтр и поиск турниров / команд / игроков | `GET /api/tournaments`, `/api/teams`, `/api/players` |
| 2 | Вкладки матчей на странице турнира | `GET /api/tournaments/{id}/matches?state=upcoming` |
| 3 | Пагинация всех списков и админ-таблиц | `?page=` на тех же эндпоинтах |
| 4 | Фильтр магазина | `GET /api/shop/products` |
| 5 | Добавление товара в корзину + обновление мини-корзины | `POST /api/cart/items` |
| 6 | Изменение количества и удаление из корзины с пересчётом итога | `PATCH/DELETE /api/cart/items/{id}` |
| 7 | Создание платежа и отправка кода на почту | `POST /api/payments` |
| 8 | Подтверждение оплаты кодом | `POST /api/payments/{id}/confirm` |
| 9 | Follow/unfollow команды или игрока | `POST/DELETE /api/follows` |
| 10 | Комментарии: подгрузка и отправка | `GET/POST /api/comments` |
| 11 | Проверка занятости email при регистрации (Remote-валидация) | `GET /api/account/check-email` |
| 12 | Админ: изменение счёта матча (результат уходит всем через SignalR), смена роли, привязка фичи к тарифу | `POST /api/admin/...` |

Общие правила: антифоржери-токен в заголовке `RequestVerificationToken`; обработка 401/403/429 понятным тостом; `AbortController`; единый модуль `wwwroot/js/http.js` — обёртка над `fetch`. Для списков сервер возвращает partial HTML, для операций — JSON.

### 10.2. Фильтрация (критерий 7)

Три фильтруемых списка на одном механизме: турниры (и команды/игроки тем же кодом), магазин, заказы. Примеры URL:

```
/tournaments?status=ongoing&region=eu&from=2025-01-01&sort=prize_desc&page=2
/shop?category=keyboards&priceMin=3000&priceMax=15000&inStock=true&sort=price_asc&page=3&pageSize=24
```

* **Источник истины — URL.** Изменение фильтра обновляет query-string через `history.pushState` и запрашивает partial.
* Перезагрузка и переход по прямой ссылке полностью восстанавливают состояние — это ключ к 5 баллам.
* Кнопка «Назад» возвращает предыдущее состояние (`popstate`).
* Активные фильтры показаны «чипсами» с крестиком, есть «Сбросить всё».
* Сортировка — только по whitelist полей через словарь «строка → выражение».
* Пустой результат — отдельное состояние с подсказкой.

### 10.3. Пагинация (критерий 9)

Серверная (`Skip/Take`), `PagedResult<T> { Items, Page, PageSize, TotalCount, TotalPages }`; AJAX-подгрузка + `pushState`, поэтому ссылка на конкретную страницу работает и F5 не сбрасывает; `pageSize` из whitelist (12/24/48); единый tag helper `<pagination>` во всех списках.

---

## 11. SignalR — один хаб `LiveHub` (`/hubs/live`)

**Live-счёт матча.** Зритель страницы матча вступает в группу `match:{id}`. Модератор в пульте (`/admin/matches`) меняет счёт → `UpdateMatchScoreCommand` → доменное событие `MatchScoreChangedEvent` → `IRealtimeNotifier` → счёт мгновенно обновляется у всех зрителей, в бегущей строке на главной и в списке матчей турнира. **Это главная демонстрация критерия 11 — показывается в двух окнах браузера.**

**Чат матча.** Отправка и приём сообщений, бейджи ролей и Premium, индикатор «печатает…», удаление сообщения модератором в реальном времени, история из MongoDB, rate limiting, отправка только авторизованным.

**Лента событий матча.** Модератор добавляет событие (старт, гол/раунд, завершение) → запись в MongoDB + рассылка подписчикам группы.

**Персональные уведомления.** Группа `user:{id}`: подписка активирована, подписка истекает, оплата прошла, заказ оформлен, комментарий одобрен, матч избранной команды начинается. Клиент: колокольчик со счётчиком и тосты. Плюс широковещательное объявление от администратора.

Инфраструктурно: `IRealtimeNotifier` объявлен в Application, реализован в Infrastructure через `IHubContext` — Application ничего не знает о SignalR. Клиент подключается с `withAutomaticReconnect`.

---

## 12. MongoDB

| Коллекция | Назначение |
|---|---|
| `match_events` | Лента событий матчей: тип события, минута, команда, описание. Гибкая схема, высокая частота записи, индекс `(matchId, occurredAt)`. Источник ленты на странице матча |
| `chat_messages` | История чата матчей, индекс `(matchId, sentAt)` |
| `audit_logs` | Действия пользователей и админов: кто, что, над чем, до/после, IP, User-Agent, correlationId. Индексы по `userId`, `action`, `timestamp`. Просмотр в `/admin/audit` |
| `app_logs` | Технические логи Serilog (sink `Serilog.Sinks.MongoDB`) |
| `news` | **Альтернативное хранилище новостей** — используется, когда `Persistence:NewsProvider=Mongo`. Доказательство EP-1 |

Доступ только через порты `IMatchEventStore`, `IChatMessageStore`, `IAuditLogWriter`, `INewsRepository`, объявленные в Application. Драйвер Mongo не выходит за пределы Infrastructure.

---

## 13. Логирование и аудит

* **Serilog**: Console (Development), Rolling File (7 дней), MongoDB (`app_logs`).
* **Enrichers:** `CorrelationId`, `UserId`, `UserName`, `RemoteIp`, `RequestPath`, `Culture`. Correlation ID генерируется в middleware и возвращается заголовком `X-Correlation-Id`.
* **Аудит действий** — MediatR `AuditBehavior`: команда, помеченная `IAuditableRequest`, автоматически пишет запись в `audit_logs` (действие, сущность, изменённые поля, результат, длительность). Дополнительно EF-интерсептор фиксирует before/after для `Matches`, `Subscriptions`, `Plans`, `PlanFeatures`, `Orders`, `Products`.
* Обязательно логируются события из §8.2.
* **Маскирование:** пароли, токены, recovery-коды, номер карты, CVV и код подтверждения никогда не попадают в логи (destructuring policy + список запрещённых ключей).

---

## 14. Middleware и ошибки

### 14.1. Собственные middleware (4)

| # | Middleware | Польза |
|---|---|---|
| 1 | `GlobalExceptionHandlingMiddleware` | Единая обработка: доменные исключения → 400/409 с локализованным сообщением, `ValidationException` → `ProblemDetails` с ошибками по полям, прочее → 500 + `TraceId` без утечки стека. Для `Accept: application/json` — `ProblemDetails`, для браузера — HTML-страница |
| 2 | `CorrelationIdMiddleware` | Сквозной идентификатор запроса в логах и в заголовке ответа |
| 3 | `SecurityHeadersMiddleware` | CSP, `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, HSTS |
| 4 | `CartCookieMiddleware` | Выдаёт и проверяет подписанный `AnonymousId` для гостевой корзины, инициирует слияние гостевой и пользовательской корзины при входе — специфичный для проекта и реально полезный |

Плюс встроенные: `UseRateLimiter`, `UseResponseCompression`, `UseRequestLocalization`, `UseStatusCodePagesWithReExecute`.

Порядок в `Program.cs` зафиксировать комментарием: Exception → HSTS/HTTPS → SecurityHeaders → CorrelationId → StaticFiles + Compression → Routing → RateLimiter → Localization → Authentication → Authorization → CartCookie → Endpoints.

### 14.2. Страницы ошибок

Собственные, стилизованные, локализованные страницы ошибок.

**Макет страницы 404 есть в шаблоне** (`docs/design/pages/404.html`) — она верстается по нему один в один, а все остальные страницы ошибок делаются по её образцу: та же композиция, те же токены, меняются код, заголовок, текст и набор кнопок. Отдельный `_ErrorLayout` создаётся один раз и переиспользуется.

Набор: `400`; `401` (с кнопкой входа и сохранением `returnUrl`); `403` в двух вариантах — недостаточно прав и **«нужна подписка»** с кнопкой перехода к тарифам; `404` (с поиском по сайту и ссылками на популярные турниры); `429` (с указанием времени ожидания); `500` (с `TraceId` для обращения в поддержку).

Реализация: `UseStatusCodePagesWithReExecute("/Error/{0}")` + `ErrorController`. Все 4xx/5xx логируются с контекстом. В `Areas/Api` вместо HTML — `ProblemDetails`.

---

## 15. Валидация и защита ввода

**Двухуровневая валидация:**
* **Клиент:** DataAnnotations на ViewModel + `jquery-validation-unobtrusive`, Remote-валидация email, подсветка полей, блокировка кнопки на время запроса.
* **Сервер:** FluentValidation в Application — единственный источник бизнес-правил, выполняется в `ValidationBehavior` до хендлера. Ошибки маппятся в `ModelState` (MVC) и `ProblemDetails` (API). Сервер валидирует всегда.
* Сообщения конкретные и локализованные: «На складе осталось 3 шт.», «Код действителен 10 минут», «Осталось 2 попытки».

**Правила предметной области:** счёт не может превышать `BestOf`; нельзя менять счёт завершённого матча; команда не может играть сама с собой; игрок не может состоять в двух командах одновременно; количество в корзине не больше остатка; один пользователь может владеть не более чем одной командой (лимит задаётся через `PlanFeature.Value`).

**Защита:**
* Razor кодирует вывод по умолчанию; `Html.Raw` только для `RulesHtml` и `BodyHtml`, прошедших `HtmlSanitizer` **на входе**.
* Антифоржери-токены на всех изменяющих запросах, включая AJAX.
* Только параметризованные запросы EF; сырой SQL запрещён; сортировка по whitelist.
* Загрузка файлов (логотипы команд, аватары, изображения товаров): whitelist расширений + проверка magic bytes, лимит размера, переименование в GUID, хранение вне `wwwroot`, раздача через контроллер.
* Отдельные ViewModel/Command на каждый сценарий; доменные сущности не биндятся из запроса.
* IDOR: доступ к заказу, корзине, своей команде проверяется по владельцу в хендлере.
* `Url.IsLocalUrl` на всех `returnUrl`.
* Цены и суммы **всегда** пересчитываются на сервере; значения с клиента не принимаются.
* `RowVersion` + обработка `DbUpdateConcurrencyException` (типичный случай — два модератора правят счёт одного матча).

---

## 16. Производительность и деплой

* `AddResponseCompression`: Brotli + Gzip, включая `text/html`, `application/json`, `text/css`, `application/javascript`.
* **Минификация и бандлинг** — WebOptimizer: минификация CSS/JS на сборке, `asp-append-version="true"`, `Cache-Control: public, max-age=31536000, immutable` для хешированной статики.
* EF: `AsNoTracking()` во всех чтениях, проекция сразу в DTO, `Include` по необходимости. **Обязательная проверка на N+1** на страницах матча и турнира.
* Изображения: WebP, `loading="lazy"`, генерация превью при загрузке.
* Health check `/health`.
* `docker-compose.yml`: `web`, `postgres`, `mongo`, `mailhog`, `nginx` (gzip/brotli, HTTP/2). Multi-stage `Dockerfile`, non-root пользователь.
* Конфигурация через переменные окружения; секреты не в репозитории.
* CI (`.github/workflows/ci.yml`): restore → build → тесты. PR не мержится с красным CI.
* README: `docker compose up -d` поднимает всё; приведены демо-учётки.
* Цели: Lighthouse Performance ≥ 85, Accessibility ≥ 90.

---

## 17. Приоритеты при нехватке времени

**Приоритет A — обязательно, не резать:** архитектура и архитектурные тесты; схема БД и сид; Identity, 3 роли, 2FA; подписки, фичи, фича-гейт, мнимая оплата с кодом; главная, турниры, матч, команды, игроки, создание команды; магазин, корзина, оформление заказа; AJAX, фильтры, пагинация; 10 админ-страниц; SignalR (live-счёт + чат + уведомления); Mongo (события матча, чат, аудит, логи); Serilog и аудит; локализация ru/en; страницы ошибок; 4 middleware; сжатие и минификация; Docker.

**Приоритет B — режется первым:** админ-страницы новостей и модерации; `ContentTranslations` для товаров (оставить для турниров и новостей); Follow; комментарии; `MongoNewsRepository` (тогда EP-1 обосновывается только теоретически — **резать в последнюю очередь**); nginx (можно оставить Kestrel со встроенным сжатием); расширенная статистика с графиками (заменить таблицей).

---

## 18. План работ (11 фаз)

> Одна фаза = одна сессия Claude Code = один PR. Начало фазы — режим Plan. Конец — зелёная сборка, тесты, коммит, обновление `docs/PROGRESS.md`.

**Фаза 0 — Фундамент.** Solution, 4 + 2 проекта, `Directory.Packages.props`, `.editorconfig`, `.gitignore`, `global.json`, README, `docker-compose` (postgres + mongo + mailhog), CI, `CLAUDE.md`, ADR-0001.

Архитектурные тесты пишутся здесь **полностью и сразу** — все правила из §5.2. На пустых проектах они проходят вакуумно (нарушителей нет, потому что типов ещё нет), поэтому сборка и CI зелёные с самого начала. Реально ловить нарушения правила начинают с Фазы 1, как только появятся первые типы. В Фазе 1 к ним добавляется sentinel-тест, утверждающий, что сборки `Domain` и `Application` содержат типы, — он страхует от ситуации, когда архитектурные проверки молча проходят из-за пустой или несобравшейся сборки.

**Инвариант проекта: сборка и тесты зелёные в конце каждой задачи, на всех фазах без исключения.** Красных тестов «по замыслу» в проекте не бывает.

**Фаза 1 — Домен и БД.** Все сущности с инвариантами, VO, enum'ы, доменные события. `AppDbContext` + конфигурации, первая миграция, сид. `docs/erd.md`. Unit-тесты домена. UI ещё нет.

**Фаза 2 — Каркас Application.** Порты, `Result`, `PagedResult`, MediatR + behaviors, FluentValidation, маппинг, `EfRepository<T>`, `UnitOfWork`.

**Фаза 3 — Identity и безопасность.** Identity, 3 роли + Premium, политики, Razor Pages аккаунта, 2FA, подтверждение email, смена пароля и email, lockout, rate limiting, письма через MailHog, сидер администратора.

**Фаза 4 — Подписки, фича-гейт, мнимая оплата.** Тарифы, фичи, `PlanFeature`, подписки, `IEntitlementService` + кэш, `FeaturePolicyProvider`, `[RequireFeature]`, `<feature-gate>`, `FakePaymentGateway` с кодом на почту, авто-роль Premium, `SubscriptionExpirationService`. Тесты прав доступа. **Самая важная фаза.**

**Фаза 5 — Дизайн-система и каркас UI.** Анализ макетов → `design-system.md` → `_tokens.scss`, `_Layout`, шапка, подвал, общие компоненты, адаптивная сетка, главная страница.

**Фаза 6 — Киберспортивный модуль.** Список и страница турнира с вкладками матчей, страница матча, команды, игроки, расписание, новости, **создание и управление своей командой по подписке**.

**Фаза 7 — Магазин.** Каталог с фильтрами, корзина (гость + пользователь + слияние), оформление заказа с оплатой по коду, история заказов.

**Фаза 8 — AJAX, фильтры, пагинация.** `Areas/Api`, все сценарии §10.1, `http.js`, `pushState`/`popstate`, скелетоны, тосты. Проверка: F5 сохраняет фильтр, «Назад» работает, ссылка на страницу 3 открывается корректно.

**Фаза 9 — Админ-панель.** Area `Admin`, layout, 10 страниц приоритета A, AJAX-таблицы, дашборд, пульт счёта матча.

**Фаза 10 — SignalR, MongoDB, логирование.** `LiveHub` (live-счёт, чат, лента событий, уведомления), интеграция пульта, все коллекции Mongo, Serilog, `AuditBehavior`, страница аудита, `MongoNewsRepository` для EP-1.

**Фаза 11 — Локализация, ошибки, middleware, деплой, документация.** ru/en и `.resx`, `ContentTranslations`, переключатель языка, 4 middleware, страницы ошибок, сжатие и минификация, Dockerfile и compose, `architecture.md` с диаграммами, ADR, `extension-points.md`, финальный README.

---

## 19. Definition of Done — чек-лист для защиты

- [ ] `dotnet build` без ошибок, `dotnet test` зелёный, включая архитектурные тесты
- [ ] 4 отдельных проекта, правило зависимостей не нарушено (доказано тестом)
- [ ] Domain без EF Core, Application без ASP.NET Core
- [ ] Работает на 360 / 768 / 1440 px, макет соблюдён, токены вынесены
- [ ] ≥10 AJAX-сценариев работают, антифоржери проходит
- [ ] ≥10 админ-страниц, права Admin и Moderator разграничены
- [ ] 3 роли + политики + динамические фича-политики; 2FA включается и обязательна для админа
- [ ] ≥17 публичных страниц, ≥13 динамических
- [ ] Фильтр: скопировал URL → открыл в новой вкладке → то же состояние; F5 не сбрасывает; «Назад» работает
- [ ] Выключил JS → сервер всё равно отверг некорректные данные с локализованным сообщением
- [ ] Пагинация AJAX с состоянием в URL
- [ ] 30 таблиц, схема нормализована, FK и индексы на месте
- [ ] **Демонстрация оплаты:** ввёл реквизиты → на MailHog пришло письмо с кодом → ввёл код → подписка активна; в таблице `Payments` только last4 и хеш кода
- [ ] **Демонстрация подписки:** без подписки кнопка «Создать команду» ведёт на тарифы; после оплаты команда создаётся
- [ ] **Демонстрация EP-3/EP-4:** админ снимает `stats.advanced` с тарифа Pro и создаёт для неё отдельный тариф — доступ у пользователей меняется без деплоя
- [ ] **Демонстрация EP-1:** смена `Persistence:NewsProvider` переключает хранилище новостей с PostgreSQL на MongoDB
- [ ] **Демонстрация live:** в одном окне модератор меняет счёт — в другом счёт обновляется мгновенно; чат матча работает
- [ ] Переключение ru/en меняет всё, включая валидацию, письма, статусы и страницы ошибок
- [ ] Mongo реально используется: лента событий, чат, аудит в админке
- [ ] Действия логируются с IP и correlation id; карта, CVV и код в логах отсутствуют
- [ ] Кастомные 400/401/403/404/429/500, локализованные
- [ ] 4 middleware, порядок корректный
- [ ] Корзина гостя сливается с пользовательской при входе
- [ ] `docker compose up -d` поднимает приложение с нуля
- [ ] Brotli/Gzip включены (проверить в DevTools), CSS/JS минифицированы и версионированы
- [ ] ≥50 осмысленных коммитов, доска задач заполнена, ≥6 ADR написаны

---

## 20. Чего нельзя делать

1. Бизнес-логика в контроллере или во вьюхе.
2. `User.IsInRole("Premium")` для проверки доступа вместо `IEntitlementService`.
3. Проверка доступа только в разметке (кнопку скрыли, эндпоинт открыт).
4. Только клиентская валидация.
5. Фильтр в JS-состоянии вместо URL.
6. Хранение полного номера карты, CVV или незахешированного кода подтверждения.
7. Суммы, приходящие с клиента и принимаемые на веру.
8. `Html.Raw` без санитизации.
9. MongoDB «для галочки».
10. Один коммит на весь проект.
