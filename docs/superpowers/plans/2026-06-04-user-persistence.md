# User Persistence Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** При каждом аутентифицированном запросе идемпотентно заводить запись пользователя в таблицу `Users`, чтобы позже к ней можно было привязывать настройки.

**Architecture:** Уникальный индекс на `User.Username` (через `IEntityTypeConfiguration`) + миграция. Идемпотентный `UserRepository.EnsureUserExistsAsync` в Infrastructure. Ленивый upsert в новом `UserProvisioningMiddleware` (Web), который выполняется после аутентификации и хранит in-process кэш уже обработанных username, чтобы не ходить в БД на каждый запрос. Покрывает и новые входы, и существующие cookie.

**Tech Stack:** .NET 6, EF Core 6.0.24 (Sqlite), ASP.NET Core middleware, NUnit + Moq, EF Core Sqlite in-memory для тестов.

**Спецификация:** `docs/superpowers/specs/2026-06-04-user-persistence-design.md`

---

## Файловая структура

- Create: `src/Pet.Jira.Infrastructure/Data/Configurations/UserConfiguration.cs` — конфигурация EF для `User` (уникальный индекс на `Username`).
- Create: `src/Pet.Jira.Infrastructure/Migrations/<timestamp>_Add_User_Username_UniqueIndex*.cs` — миграция (генерируется `dotnet ef`).
- Create: `src/Pet.Jira.Application/Users/IUserRepository.cs` — интерфейс репозитория.
- Create: `src/Pet.Jira.Infrastructure/Users/UserRepository.cs` — реализация.
- Create: `src/Pet.Jira.Web/Authentication/UserProvisioningMiddleware.cs` — middleware с кэш-гардом.
- Modify: `src/Pet.Jira.Infrastructure/ServiceCollectionExtensions.cs` — регистрация `IUserRepository`.
- Modify: `src/Pet.Jira.Web/Startup.cs:123` — регистрация middleware в конвейере.
- Modify: `tests/Pet.Jira.UnitTests/Pet.Jira.UnitTests.csproj` — пакет Sqlite + ссылка на Web + AspNetCore framework reference.
- Create: `tests/Pet.Jira.UnitTests/Infrastructure/Users/UserRepositoryTests.cs`
- Create: `tests/Pet.Jira.UnitTests/Web/Authentication/UserProvisioningMiddlewareTests.cs`

> Примечание: таблица `Users` сейчас пустая (в неё никогда не писали), поэтому добавление уникального индекса не упрётся в существующие дубли.

---

## Task 1: Уникальный индекс на User.Username + миграция

**Files:**
- Create: `src/Pet.Jira.Infrastructure/Data/Configurations/UserConfiguration.cs`
- Create: `src/Pet.Jira.Infrastructure/Migrations/` (через `dotnet ef`)

- [ ] **Step 1: Создать конфигурацию сущности**

`src/Pet.Jira.Infrastructure/Data/Configurations/UserConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pet.Jira.Domain.Entities.Users;

namespace Pet.Jira.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder
                .HasIndex(user => user.Username)
                .IsUnique();
        }
    }
}
```

Конфигурация подхватывается автоматически — `ApplicationDbContext.OnModelCreating` уже вызывает `builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly())` (`src/Pet.Jira.Infrastructure/Data/Contexts/ApplicationDbContext.cs:82`).

- [ ] **Step 2: Сборка — убедиться, что компилируется**

Run: `dotnet build src/Pet.Jira.Infrastructure/Pet.Jira.Infrastructure.csproj`
Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Создать миграцию**

Из каталога `src/Pet.Jira.Infrastructure` (как в `src/Pet.Jira.Web/development.readme.md`):

Run:
```bash
cd src/Pet.Jira.Infrastructure
dotnet ef migrations add Add_User_Username_UniqueIndex --startup-project ../Pet.Jira.Web --context ApplicationDbContext
```
Expected: создаются файлы `Migrations/<timestamp>_Add_User_Username_UniqueIndex.cs` и `.Designer.cs`, обновляется `ApplicationDbContextModelSnapshot.cs`. В `Up()` должен быть `CreateIndex(... "Username", unique: true)`.

> Если `dotnet ef` не установлен: `dotnet tool install --global dotnet-ef --version 6.*`

- [ ] **Step 4: Проверить, что миграция применяется**

Миграции применяются на старте в `Startup.Configure` (`Database.Migrate()`, `src/Pet.Jira.Web/Startup.cs:147`). Достаточно убедиться, что решение собирается:

Run: `dotnet build`
Expected: Build succeeded.

- [ ] **Step 5: Commit**

```bash
git add src/Pet.Jira.Infrastructure/Data/Configurations/UserConfiguration.cs src/Pet.Jira.Infrastructure/Migrations/
git commit -m "feat: add unique index on User.Username with migration"
```

---

## Task 2: IUserRepository + UserRepository (идемпотентный upsert)

**Files:**
- Create: `src/Pet.Jira.Application/Users/IUserRepository.cs`
- Create: `src/Pet.Jira.Infrastructure/Users/UserRepository.cs`
- Modify: `src/Pet.Jira.Infrastructure/ServiceCollectionExtensions.cs`
- Modify: `tests/Pet.Jira.UnitTests/Pet.Jira.UnitTests.csproj`
- Test: `tests/Pet.Jira.UnitTests/Infrastructure/Users/UserRepositoryTests.cs`

- [ ] **Step 1: Добавить пакет Sqlite в тестовый проект**

В `tests/Pet.Jira.UnitTests/Pet.Jira.UnitTests.csproj` в `ItemGroup` с пакетами добавить (версия совпадает с Infrastructure):

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="6.0.24" />
```

- [ ] **Step 2: Написать падающий тест**

`tests/Pet.Jira.UnitTests/Infrastructure/Users/UserRepositoryTests.cs`:

```csharp
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pet.Jira.Infrastructure.Data.Contexts;
using Pet.Jira.Infrastructure.Users;

namespace Pet.Jira.UnitTests.Infrastructure.Users
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private SqliteConnection _connection;
        private DbContextOptions<ApplicationDbContext> _options;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = new ApplicationDbContext(_options);
            context.Database.EnsureCreated();
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Dispose();
        }

        [Test]
        public async Task EnsureUserExistsAsync_Should_CreateUser_WhenNotExists()
        {
            using (var context = new ApplicationDbContext(_options))
            {
                var repository = new UserRepository(context);
                await repository.EnsureUserExistsAsync("john");
            }

            using var assertContext = new ApplicationDbContext(_options);
            Assert.That(assertContext.Users.Count(user => user.Username == "john"), Is.EqualTo(1));
        }

        [Test]
        public async Task EnsureUserExistsAsync_Should_NotCreateDuplicate_WhenCalledTwice()
        {
            using (var context = new ApplicationDbContext(_options))
            {
                await new UserRepository(context).EnsureUserExistsAsync("john");
            }
            using (var context = new ApplicationDbContext(_options))
            {
                await new UserRepository(context).EnsureUserExistsAsync("john");
            }

            using var assertContext = new ApplicationDbContext(_options);
            Assert.That(assertContext.Users.Count(user => user.Username == "john"), Is.EqualTo(1));
        }
    }
}
```

> `System`, `System.Linq`, `System.Threading.Tasks` доступны через `ImplicitUsings` (включён в тестовом csproj). `NUnit.Framework` — через `tests/Pet.Jira.UnitTests/Usings.cs`.

- [ ] **Step 3: Запустить тест — убедиться, что не компилируется/падает**

Run: `dotnet test tests/Pet.Jira.UnitTests/Pet.Jira.UnitTests.csproj --filter "FullyQualifiedName~UserRepositoryTests"`
Expected: FAIL — компиляция падает, т.к. `IUserRepository`/`UserRepository` ещё не существуют ("type or namespace 'UserRepository' could not be found").

- [ ] **Step 4: Создать интерфейс**

`src/Pet.Jira.Application/Users/IUserRepository.cs`:

```csharp
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Users
{
    public interface IUserRepository
    {
        Task EnsureUserExistsAsync(string username, CancellationToken cancellationToken = default);
    }
}
```

- [ ] **Step 5: Реализовать репозиторий**

`src/Pet.Jira.Infrastructure/Users/UserRepository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Pet.Jira.Application.Users;
using Pet.Jira.Domain.Entities.Users;
using Pet.Jira.Infrastructure.Data.Contexts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task EnsureUserExistsAsync(string username, CancellationToken cancellationToken = default)
        {
            var exists = await _dbContext.Users
                .AnyAsync(user => user.Username == username, cancellationToken);
            if (exists)
            {
                return;
            }

            _dbContext.Users.Add(new User
            {
                Username = username,
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Запись создана параллельным запросом — уникальный индекс отклонил дубль. Это не ошибка.
            }
        }
    }
}
```

- [ ] **Step 6: Зарегистрировать репозиторий в DI**

В `src/Pet.Jira.Infrastructure/ServiceCollectionExtensions.cs` после строки с `IArticleDataSource` (около `:55`) добавить:

```csharp
            services.AddTransient<IUserRepository, UserRepository>();
```

`IUserRepository` уже доступен — файл содержит `using Pet.Jira.Application.Users;` (`:8`); `UserRepository` — `using Pet.Jira.Infrastructure.Users;` (`:20`).

- [ ] **Step 7: Запустить тесты — убедиться, что проходят**

Run: `dotnet test tests/Pet.Jira.UnitTests/Pet.Jira.UnitTests.csproj --filter "FullyQualifiedName~UserRepositoryTests"`
Expected: PASS, 2 tests passed.

- [ ] **Step 8: Commit**

```bash
git add src/Pet.Jira.Application/Users/IUserRepository.cs src/Pet.Jira.Infrastructure/Users/UserRepository.cs src/Pet.Jira.Infrastructure/ServiceCollectionExtensions.cs tests/Pet.Jira.UnitTests/Pet.Jira.UnitTests.csproj tests/Pet.Jira.UnitTests/Infrastructure/Users/UserRepositoryTests.cs
git commit -m "feat: add idempotent UserRepository.EnsureUserExistsAsync"
```

---

## Task 3: UserProvisioningMiddleware + регистрация

**Files:**
- Create: `src/Pet.Jira.Web/Authentication/UserProvisioningMiddleware.cs`
- Modify: `src/Pet.Jira.Web/Startup.cs:123`
- Modify: `tests/Pet.Jira.UnitTests/Pet.Jira.UnitTests.csproj`
- Test: `tests/Pet.Jira.UnitTests/Web/Authentication/UserProvisioningMiddlewareTests.cs`

- [ ] **Step 1: Дать тестовому проекту доступ к Web-типам**

В `tests/Pet.Jira.UnitTests/Pet.Jira.UnitTests.csproj` добавить framework reference (в `PropertyGroup` или отдельный `ItemGroup`) и ссылку на Web-проект:

```xml
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Pet.Jira.Web\Pet.Jira.Web.csproj" />
  </ItemGroup>
```

`FrameworkReference` нужен, чтобы в тестах были доступны `HttpContext`, `DefaultHttpContext`, `RequestDelegate` для проверки middleware.

- [ ] **Step 2: Написать падающий тест**

`tests/Pet.Jira.UnitTests/Web/Authentication/UserProvisioningMiddlewareTests.cs`:

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Pet.Jira.Application.Users;
using Pet.Jira.Web.Authentication;
using System.Security.Claims;

namespace Pet.Jira.UnitTests.Web.Authentication
{
    [TestFixture]
    public class UserProvisioningMiddlewareTests
    {
        private Mock<IUserRepository> _userRepository;

        [SetUp]
        public void SetUp()
        {
            _userRepository = new Mock<IUserRepository>();
        }

        private static HttpContext CreateContext(ClaimsPrincipal user)
        {
            return new DefaultHttpContext { User = user };
        }

        private static ClaimsPrincipal AuthenticatedUser(string username)
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, username) },
                authenticationType: "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private UserProvisioningMiddleware CreateMiddleware(RequestDelegate next)
        {
            return new UserProvisioningMiddleware(next, NullLogger<UserProvisioningMiddleware>.Instance);
        }

        [Test]
        public async Task InvokeAsync_Should_ProvisionUserOnce_AcrossRepeatedRequests()
        {
            var nextCallCount = 0;
            RequestDelegate next = _ => { nextCallCount++; return Task.CompletedTask; };
            var middleware = CreateMiddleware(next);

            await middleware.InvokeAsync(CreateContext(AuthenticatedUser("john")), _userRepository.Object);
            await middleware.InvokeAsync(CreateContext(AuthenticatedUser("john")), _userRepository.Object);

            _userRepository.Verify(
                repository => repository.EnsureUserExistsAsync("john", It.IsAny<CancellationToken>()),
                Times.Once());
            Assert.That(nextCallCount, Is.EqualTo(2));
        }

        [Test]
        public async Task InvokeAsync_Should_SkipProvisioning_WhenUserIsAnonymous()
        {
            var nextCalled = false;
            RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
            var middleware = CreateMiddleware(next);

            await middleware.InvokeAsync(CreateContext(new ClaimsPrincipal(new ClaimsIdentity())), _userRepository.Object);

            _userRepository.Verify(
                repository => repository.EnsureUserExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never());
            Assert.That(nextCalled, Is.True);
        }

        [Test]
        public async Task InvokeAsync_Should_NotBreakPipeline_WhenRepositoryThrows()
        {
            var nextCalled = false;
            RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
            _userRepository
                .Setup(repository => repository.EnsureUserExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("db down"));
            var middleware = CreateMiddleware(next);

            await middleware.InvokeAsync(CreateContext(AuthenticatedUser("john")), _userRepository.Object);

            Assert.That(nextCalled, Is.True);
        }
    }
}
```

> `Task`, `CancellationToken`, `Exception` доступны через `ImplicitUsings`.

- [ ] **Step 3: Запустить тест — убедиться, что не компилируется/падает**

Run: `dotnet test tests/Pet.Jira.UnitTests/Pet.Jira.UnitTests.csproj --filter "FullyQualifiedName~UserProvisioningMiddlewareTests"`
Expected: FAIL — `UserProvisioningMiddleware` ещё не существует.

- [ ] **Step 4: Реализовать middleware**

`src/Pet.Jira.Web/Authentication/UserProvisioningMiddleware.cs`:

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Pet.Jira.Application.Users;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Authentication
{
    public class UserProvisioningMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserProvisioningMiddleware> _logger;
        private readonly ConcurrentDictionary<string, byte> _provisionedUsernames = new();

        public UserProvisioningMiddleware(
            RequestDelegate next,
            ILogger<UserProvisioningMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
        {
            var username = context.User?.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : null;

            if (!string.IsNullOrEmpty(username) && !_provisionedUsernames.ContainsKey(username))
            {
                try
                {
                    await userRepository.EnsureUserExistsAsync(username, context.RequestAborted);
                    _provisionedUsernames.TryAdd(username, 0);
                }
                catch (Exception exception)
                {
                    // Не валим запрос: пользователь продолжает работу, запись довнесётся позже.
                    // Username намеренно НЕ кладём в кэш — попытка повторится на следующем запросе.
                    _logger.LogError(exception, "Failed to provision user {Username} in the database", username);
                }
            }

            await _next(context);
        }
    }
}
```

> `IUserRepository` приходит параметром `InvokeAsync` (per-request DI из request scope). Middleware-экземпляр один на всё приложение, поэтому `_provisionedUsernames` живёт между запросами и служит кэш-гардом.

- [ ] **Step 5: Запустить тесты — убедиться, что проходят**

Run: `dotnet test tests/Pet.Jira.UnitTests/Pet.Jira.UnitTests.csproj --filter "FullyQualifiedName~UserProvisioningMiddlewareTests"`
Expected: PASS, 3 tests passed.

- [ ] **Step 6: Зарегистрировать middleware в конвейере**

В `src/Pet.Jira.Web/Startup.cs` сразу после `app.UseMiddleware<AuthenticationMiddleware>();` (`:123`) добавить:

```csharp
            app.UseMiddleware<UserProvisioningMiddleware>();
```

`using Pet.Jira.Web.Authentication;` в `Startup.cs` уже есть (`:18`). Порядок важен: middleware должен идти после `app.UseAuthentication()` (`:119`), чтобы `context.User` был заполнен из cookie.

- [ ] **Step 7: Сборка всего решения**

Run: `dotnet build`
Expected: Build succeeded, 0 errors.

- [ ] **Step 8: Прогнать все тесты**

Run: `dotnet test`
Expected: PASS, все тесты зелёные (включая существующие).

- [ ] **Step 9: Commit**

```bash
git add src/Pet.Jira.Web/Authentication/UserProvisioningMiddleware.cs src/Pet.Jira.Web/Startup.cs tests/Pet.Jira.UnitTests/Pet.Jira.UnitTests.csproj tests/Pet.Jira.UnitTests/Web/Authentication/UserProvisioningMiddlewareTests.cs
git commit -m "feat: provision users into DB via UserProvisioningMiddleware"
```

---

## Ручная проверка (после реализации)

- [ ] Запустить приложение, войти под Jira-аккаунтом.
- [ ] Открыть `JiraCopilot.sqlite3` и убедиться, что в таблице `Users` появилась строка с вашим `Username` (`CreatedAt` заполнен).
- [ ] Перезагрузить страницу несколько раз → дублей не появляется, запись одна.
- [ ] (Опционально) Удалить cookie и войти снова → запись по-прежнему одна.
