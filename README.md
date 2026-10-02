# Практическая работа. Модуль 04 — команды, EF Core, Repository, DTO и AutoMapper

**Некоммерческое АО «Казахский национальный исследовательский технический университет имени К. И. Сатпаева»**  
Институт Автоматики и информационных технологий · Кафедра Программной инженерии  
CSE5032 «Разработка веб-сервисов» · Практическая работа  
Зав. кафедры ПИ: Абдолдина Ф. Н. · Экзаменатор: Герцен Е. А.

**Тема:** DTO и AutoMapper. Repository с использованием EF Core.  
**Цель:** совместно с преподавателем реализовать базовый доступ к данным и GET/POST, затем самостоятельно добавить PUT, DELETE и поиск по городу.

## Краткий отчёт

Создано ASP.NET Core Web API `WebApiPrac4` по шаблону приложения с контроллерами и Swagger. API управляет командами `Team`. EF Core хранит их в SQLite; схему базы Code First создаёт миграция. Контроллер использует только `ITeamRepository`, модели ответа преобразуются AutoMapper в `TeamDto`, а все ответы возвращаются в единой оболочке `ReturnResult<T>`.

Сущность `Team` содержит `Id`, `Name`, `City`, `Description`. Согласно условию `TeamDto` содержит только `Id`, `Name`, `City`: `Description` остаётся во внутренней Entity и не выдаётся клиенту. Для POST/PUT применяется `TeamInputDto`, в который описание включено, чтобы его можно было записывать и редактировать.

## Архитектура

```text
Клиент / Swagger
      ↓
TeamController
      ↓  AutoMapper
TeamInputDto ↔ TeamDto
      ↓  ITeamRepository
TeamRepository
      ↓
AppDbContext / DbSet<Team>
      ↓  EF Core + Code First migration
SQLite (teams.db)
```

| Элемент | Что делает в этой работе |
| --- | --- |
| Entity `Team` | Представляет сохраняемую команду с описанием. |
| `AppDbContext` / `DbSet<Team>` | Связывают CLR-модель с таблицей `Teams`; контекст отслеживает изменения. |
| Code First и миграция | Создают таблицу по модели и хранят историю применённых изменений схемы. |
| Repository | Скрывает EF-запросы за интерфейсом; контроллер не обращается к `AppDbContext` напрямую. |
| `TeamDto` | Ограничивает ответ для клиента полями `Id`, `Name`, `City`; скрывает `Description`. |
| AutoMapper | Переносит значения между Entity и DTO по профилю `CreateMap<Team, TeamDto>().ReverseMap()`. |
| `ReturnResult<T>` | Даёт одинаковые поля `isSuccess`, `result`, `errorMessage`, не заменяя HTTP-коды. |

## Как работа выполнялась по шагам

### Часть 1. Вместе с преподавателем

1. **Создана модель `Team`.** Добавлены `Id`, `Name`, `City`, `Description`; для строк заданы ограничения длины и обязательные поля имени/города.
2. **Подключён EF Core.** Используется провайдер SQLite для локальной базы без отдельного сервера БД.
3. **Создан `AppDbContext`.** Контекст наследуется от `DbContext` и содержит `public DbSet<Team> Teams { get; set; }`.
4. **Настроена база и DI.** Строка `DefaultConnection` находится в `appsettings.json`. В `Program.cs` контекст регистрируется через `AddDbContext<AppDbContext>(...UseSqlite(...))`.
5. **Создан интерфейс `ITeamRepository`.** Он задаёт асинхронные операции получения списка и записи; дополнительно добавлен метод поиска по городу.
6. **Реализован `TeamRepository`.** Все операции чтения/создания/обновления/удаления выполняются через `AppDbContext`; чтение использует `AsNoTracking`.
7. **Репозиторий зарегистрирован в DI.** `AddScoped<ITeamRepository, TeamRepository>()` обеспечивает экземпляр на время HTTP-запроса.
8. **Создан `TeamDto`.** Он намеренно не содержит `Description`; отдельный `TeamInputDto` нужен для полей входной записи.
9. **Подключён AutoMapper.** В `AutoMapperProfile` настроено требуемое отображение `CreateMap<Team, TeamDto>().ReverseMap()` и преобразование входного DTO в Entity.
10. **Создан `TeamController`.** Через constructor injection он получает репозиторий, `IMapper` и логгер. Вместе реализованы GET списка, GET по ID и POST.
11. **Добавлен `ReturnResult<T>`.** Успех и ошибки валидации/отсутствия ресурса оформляются одной структурой; успешное создание при этом возвращает HTTP `201`.

### Часть 2. Самостоятельно

12. **Добавлен PUT `/api/team/{id}`.** Сначала команда ищется через Repository; затем AutoMapper обновляет её поля, Repository сохраняет изменения. Для отсутствующего ID возвращается `404`.
13. **Добавлен DELETE `/api/team/{id}`.** Наличие команды проверяется через Repository; удаление выполняется тем же слоем. Для отсутствующего ID возвращается `404`.
14. **Добавлен поиск GET `/api/team/city/{city}`.** Репозиторий фильтрует по точному совпадению города и возвращает только команды выбранного города.
15. **Проверена работа в Swagger.** Выполнены GET списка, невалидный и успешный POST, GET по ID, PUT, поиск по городу, DELETE и повторный GET удалённого ID.

Для воспроизведения миграций из каталога проекта:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef -- database update
dotnet run --urls http://127.0.0.1:5087
```

При запуске приложение также вызывает `Database.Migrate()`. Swagger UI: `http://127.0.0.1:5087/swagger`.

## Endpoint и результаты проверки

| Метод | Маршрут | Для чего | Проверенный HTTP-результат |
| --- | --- | --- | --- |
| `GET` | `/api/team` | Получить все команды | `200 OK` |
| `GET` | `/api/team/{id}` | Получить команду по ID | `200 OK`, нет команды — `404 Not Found` |
| `POST` | `/api/team` | Добавить команду | `201 Created`, невалидное тело — `400 Bad Request` |
| `PUT` | `/api/team/{id}` | Изменить команду | `200 OK`, нет команды — `404 Not Found` |
| `DELETE` | `/api/team/{id}` | Удалить команду | `200 OK`, нет команды — `404 Not Found` |
| `GET` | `/api/team/city/{city}` | Найти команды указанного города | `200 OK` с массивом совпадений |

## Примеры JSON

POST/PUT принимают Entity-поля через входной DTO:

```json
{
  "name": "Codex Demo Team",
  "city": "Almaty",
  "description": "Команда для демонстрации CRUD."
}
```

В ответе `Description` отсутствует — AutoMapper сформировал `TeamDto`:

```json
{
  "isSuccess": true,
  "result": {
    "id": 4,
    "name": "Codex Demo Team",
    "city": "Almaty"
  },
  "errorMessage": []
}
```

Пример ошибки: HTTP `404`, `isSuccess` — `false`, `result` — `null`, описание находится в `errorMessage`. Код состояния HTTP сохраняется отдельно от единого JSON-контракта.

## Скриншоты выполнения

Каждый снимок Swagger — отдельный реальный результат запроса после **Try it out** → **Execute**. Скриншоты показывают URL, HTTP-код и JSON-ответ.

### Swagger с реализованными endpoint

![Swagger: маршруты Team API](docs/swagger-endpoints.png)

### Шаг 1. Получение списка — GET 200

![GET api/team — список команд](docs/get-all-200.png)

### Шаг 2. Проверка валидации — POST 400

![POST api/team с пустыми полями — HTTP 400 и ошибки в ReturnResult](docs/post-validation-400.png)

### Шаг 3. Создание — POST 201

![POST api/team — команда создана, ответ содержит только Id, Name, City](docs/post-create-201.png)

### Шаг 4. Чтение по ID — GET 200

![GET api/team по ID — команда возвращена через TeamDto](docs/get-by-id-200.png)

### Шаг 5. Самостоятельное изменение — PUT 200

![PUT api/team по ID — команда и город обновлены](docs/put-update-200.png)

### Шаг 6. Поиск по городу — GET 200

![GET api/team/city/Astana — выдаются только команды Astana](docs/get-by-city-200.png)

### Шаг 7. Самостоятельное удаление — DELETE 200

![DELETE api/team по ID — успешное удаление](docs/delete-200.png)

### Шаг 8. Проверка отсутствующего ID — GET 404

![Повторный GET удалённой команды — Not Found 404](docs/get-after-delete-404.png)

### Вывод EF Core и приложения в PowerShell

![PowerShell: запуск и SQL-логи работы с таблицей Teams](docs/powershell-logs.png)

## Что рассказать на защите

> Клиент отправляет запрос в `TeamController`. Контроллер получает `ITeamRepository` через DI и не знает деталей EF Core. Репозиторий выполняет запросы через `AppDbContext`, а EF Core переводит их в SQL для SQLite. Полученную Entity AutoMapper преобразует в `TeamDto`, поэтому поле `Description` не выходит наружу. `ReturnResult` стандартизирует тело ответа, а статус HTTP (`200`, `201`, `400`, `404`) остаётся корректным.

Короткая демонстрация: открыть Swagger → получить список → создать команду → получить её по ID → изменить → найти по новому городу → удалить → повторить GET и показать `404`. Для поиска использовать `Astana`: после PUT демонстрационная команда переехала туда и появляется вместе с командой из начальных данных.

## Вывод

Выполнены обе части практической работы: совместно разработана базовая цепочка Entity/EF Core/Repository/DTO/AutoMapper и GET/POST, самостоятельно добавлены PUT, DELETE и фильтр по городу. Все запросы к БД проходят через Repository, клиент получает DTO без `Description`, а Swagger подтверждает CRUD, валидацию и поиск.
