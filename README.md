# ZeldaArena

Киберспортивная платформа по игре Zelda: турниры, матчи с live-счётом, команды, игроки,
статистика, расписание, новости и магазин игровой периферии. Платная подписка открывает
создание собственной команды и расширенную статистику.

Полное техническое задание — [`docs/SPEC.md`](docs/SPEC.md).
Ход работ — [`docs/PROGRESS.md`](docs/PROGRESS.md).

## Стек

.NET 10 (LTS) · ASP.NET Core MVC + Razor Pages · EF Core 10 · PostgreSQL · MongoDB · SignalR ·
MediatR · FluentValidation · Serilog · Clean Architecture.

## Что нужно установить

| Инструмент | Проверка |
|---|---|
| .NET 10 SDK | `dotnet --list-sdks` → строка `10.x` |
| Docker Desktop | `docker --version` и запущенный демон |
| Git | `git --version` |

## Быстрый старт

```bash
# 1. Инфраструктура: postgres, mongo, mailhog
docker compose -f deploy/docker-compose.yml up -d

# 2. Сборка и тесты
dotnet tool restore     # dotnet-ef из .config/dotnet-tools.json
dotnet restore
dotnet build
dotnet test

# 3. Запуск: миграции и сид применяются автоматически в среде Development
dotnet run --project src/ZeldaArena.Web
```

При первом запуске база создаётся миграцией и наполняется демонстрационными данными:
12 команд, 60 игроков, 4 турнира, 40 матчей (два идут прямо сейчас), 24 товара и 3 тарифа.
Сид идемпотентен — повторный запуск ничего не дублирует. Состав данных и схема описаны
в `docs/erd.md`.

Настройки контейнеров можно переопределить: скопировать `deploy/.env.example` в `deploy/.env`
и поправить значения.

| Сервис | Адрес | Примечание |
|---|---|---|
| PostgreSQL | `localhost:5433` | **не 5432** — этот порт обычно занят локально установленным сервером |
| MongoDB | `localhost:27017` | |
| MailHog | http://localhost:8025 | здесь видно письмо с кодом подтверждения оплаты |

## Структура решения

```
src/
  ZeldaArena.Domain/          сущности, value objects, доменные события. Только BCL
  ZeldaArena.Application/     CQRS-сценарии, порты, валидаторы, behaviors
  ZeldaArena.Infrastructure/  EF Core, Identity, Mongo, почта, SignalR, фоновые службы
  ZeldaArena.Web/             MVC, Razor Pages, API для AJAX, middleware, хаб
tests/
  ZeldaArena.UnitTests/          домен и Application
  ZeldaArena.ArchitectureTests/  правило зависимостей (docs/SPEC.md §5.2)
deploy/                       docker-compose и конфигурация развёртывания
docs/                         ТЗ, ADR, макеты, журнал прогресса
```

Правило зависимостей: `Domain ← Application ← Infrastructure`, `Web` ссылается на
`Infrastructure` единственно ради `AddInfrastructure()` в `Program.cs`. Нарушение роняет
`dotnet test` и CI.

## Демо-учётки

Создаются сидом при первом запуске в среде `Development`. Пароли лежат
в `appsettings.Development.json` — это фикстуры разработки, а не секреты: сид выполняется
только в `Development`.

| Учётная запись | Пароль | Роль |
|---|---|---|
| `admin@zeldaarena.local` | `Admin-Zelda-2026!` | Admin |
| `moderator@zeldaarena.local` | `Moder-Zelda-2026!` | Moderator |
| `subscriber@zeldaarena.local` | `Demo-Zelda-2026!` | User (подписка появится в Фазе 4) |
| `expired@zeldaarena.local` | `Demo-Zelda-2026!` | User |
| `visitor@zeldaarena.local` | `Demo-Zelda-2026!` | User |

Адрес у всех сидовых учёток уже подтверждён, второй фактор выключен.

**На боевом сервере учётные записи из файла не заводятся.** Раздел `SeedAccounts`
в `appsettings.json` пуст, а ненастроенная учётная запись пропускается. Администратор
создаётся переменными окружения:

```bash
SeedAccounts__Admin__Email=admin@example.com
SeedAccounts__Admin__Password=<из менеджера секретов>
```

Локально то же самое удобно держать в User Secrets:

```bash
dotnet user-secrets set "SeedAccounts:Admin:Password" "<пароль>" --project src/ZeldaArena.Web
```

## Регистрация и почта

Регистрация требует подтверждения адреса: войти до перехода по ссылке из письма нельзя.
Письмо забирает MailHog — откройте http://localhost:8025, там же видны письма
восстановления пароля и смены адреса.

Двухфакторная аутентификация подключается в личном кабинете
(`/Identity/Account/Manage/TwoFactorAuthentication`): QR-код рисуется на сервере,
подходит любое приложение с поддержкой TOTP. Для роли `Admin` второй фактор обязателен —
отключить его нельзя.

Если приложение запускается по HTTP (профиль `http` из `launchSettings.json`), cookie
аутентификации выдаётся без флага `Secure` — за это отвечает `Identity:RequireSecureCookie`
в `appsettings.Development.json`. В остальных средах флаг включён.

## Разработка

```bash
dotnet format                                   # форматирование по .editorconfig
dotnet test                                     # юнит- и архитектурные тесты
dotnet ef migrations add <Name> -p src/ZeldaArena.Infrastructure -s src/ZeldaArena.Web
dotnet ef database update  -p src/ZeldaArena.Infrastructure -s src/ZeldaArena.Web
```

Версии пакетов задаются централизованно в `Directory.Packages.props`,
версия SDK — в `global.json`.
