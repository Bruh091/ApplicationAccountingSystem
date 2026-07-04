# Help Desk Ticketing System

Десктопное приложение для учёта и обработки заявок (тикетов) службы поддержки. Построено на **Avalonia UI** (.NET 9) с **PostgreSQL** в качестве БД.

## Стек

- **.NET 9** — целевая платформа
- **Avalonia 12** — кросс-платформенный UI (Windows, macOS, Linux)
- **Entity Framework Core 9** — ORM
- **PostgreSQL** — база данных
- **BCrypt.Net-Next** — хеширование паролей
- **Clean Architecture** — разделение на Domain / Application / Infrastructure
- **Docker Compose** — быстрый запуск PostgreSQL + pgAdmin

## Архитектура

```
ApplicationAccountingSystem.sln
├── ApplicationAccountingSystem/     # Avalonia-приложение
│   ├── Domain/                      # Сущности, перечисления, интерфейсы репозиториев
│   │   ├── Model/                   # User, Tickets, SLAPolicy, Comment, Attachment, TicketHistory
│   │   ├── Designation/             # UserRole, TicketStatus, TicketPriority
│   │   └── Interfaces/              # IUserRepository, ITicketRepository, ...
│   ├── Application/                 # Бизнес-логика (сервисы + DTO)
│   │   ├── Services/                # AuthService, UserService, TicketService, CommentService
│   │   ├── Interfaces/              # IAuthService, IUserService, ...
│   │   └── DTOs/                    # LoginDto, RegisterUserDto, TicketDto, ...
│   ├── Infrastructure/              # Доступ к данным, DI, хеширование
│   │   ├── Data/AppDbContext.cs      # EF Core контекст
│   │   ├── Repository/              # Реализации репозиториев
│   │   ├── Auth/PasswordHasher.cs   # BCrypt
│   │   └── DependencyInjection/     # Регистрация сервисов
│   └── UI                           # Avalonia (App.axaml, MainWindow.axaml)
├── docker-compose.yml               # PostgreSQL 16 + pgAdmin
└── README.md
```

## Функциональность

- **Роли пользователей:** Admin, Agent, User
- **Управление тикетами:** создание, назначение на агента, смена статуса (`New → InProgress → Resolved → Closed`), приоритеты (`Low / Medium / High / Urgent`)
- **SLA-политики:** настраиваемое время ответа и решения для каждого приоритета, контроль просрочек
- **Аутентификация:** регистрация / вход с хешированием BCrypt
- **Комментарии и вложения** к тикетам
- **История изменений** тикета

## Быстрый старт

### 1. Запуск PostgreSQL

```bash
docker-compose up -d
```

Поднимутся:
- **PostgreSQL 16** на порту `5432` (БД: `helpdesk`, user: `user`, pass: `pass`)
- **pgAdmin** на порту `5050` (логин: `admin@example.com`, пароль: `admin`)

### 2. Запуск приложения

```bash
cd ApplicationAccountingSystem
dotnet run
```

### 3. Настройка

Строка подключения и SLA-параметры задаются в `appsettings.json`:
- `ConnectionStrings:PostgreSQL` — подключение к БД
- `SLA` — тайминги ответа и решения для каждого приоритета

## Миграции

Миграции создаются и применяются через EF Core Tools:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```
