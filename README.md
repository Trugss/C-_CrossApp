# CrossApp

Наскрізний проєкт з крос-платформного програмування.

**Предметна область:** Бібліотека.   
**Сутності:** Book (Видання), BookCopy (Примірник), Reader (Читач), Loan(Видача).  
**Призначення:** облік видач примірників книг читачам.  

**Структура Solution**
```text
CrossApp/
├── CrossApp.sln
├── README.md
├── data/
│   ├── sample.csv
│   ├── sample.json
│   └── mixed.csv
├── src/
│   ├── Core/
│   │   ├── Abstractions/
│   │   │   └── IBookStore.cs
│   │   ├── Services/
│   │   │   └── LendingService.cs
│   │   ├── Storage/
│   │   │   ├── InMemoryBookStore.cs
│   │   │   ├── FileBookStore.cs
│   │   │   └── CachingBookStore.cs
|   |   ├── Domain/
|   |   |   ├── BookCopy.cs
|   |   |   ├── DomainBatchConverter.cs
|   |   |   ├── Loan.cs
|   |   |   ├── LoanService.cs
|   |   |   └── LoanStatus.cs
│   │   ├── Dto/
|   |   |   ├── BookCopyDto.cs
│   │   │   ├── BookDto.cs
│   │   │   ├── ReaderDto.cs
│   │   │   ├── ImportResult.cs
|   |   |   ├── LoanDto.cs
│   │   │   └── LibraryBatchResult.cs
│   │   ├── Import/
│   │   │   ├── BookCsvImporter.cs
│   │   │   ├── BookJsonImporter.cs
│   │   │   └── MixedCsvImporter.cs
│   │   ├── SampleData.cs
│   │   └── EnvironmentInfo.cs
│   └── Cli/
│       ├── StoreFactory.cs
│       └── Program.cs
└── publish/   
```

## Запуск
```
dotnet build  
dotnet run --project src/Cli  
dotnet run --project src/Cli -- "data/sample.json"  
dotnet run --project src/Cli -- "data/mixed.csv"  
dotnet run --project src/Cli -- "data/missing.csv"  
```
**ЛР5: вибір сховища**
```
dotnet run --project src/Cli                         # InMemoryBookStore, дані зникають після виходу
dotnet run --project src/Cli -- --file               # FileBookStore, дані накопичуються в data/library.json
dotnet run --project src/Cli -- --cache              # CachingBookStore поверх InMemoryBookStore
dotnet run --project src/Cli -- --file --cache       # CachingBookStore поверх FileBookStore
dotnet run --project src/Cli -- --factory --file     # той самий вибір, але через StoreFactory
```
Файл `data/library.json` створюється поруч зі зібраним застосунком: `src/Cli/bin/Debug/net10.0/data/library.json`.
## Публікація застосунку
**Self-contained публікація під Windows**
```
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -o ./publish/self-contained
```
**Framework-dependent публікація під Windows**
```
dotnet publish src/Cli -c Release -r win-x64 --self-contained false -o ./publish/framework-dependent
```
**Self-Contained публікація під лінукс**
```
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true -o ./publish/linux-x64
```

## Порівняння публікацій

| RID | Режим | Розмір |Потрібен встановлений runtime |
| --- | --- | --- | --- |
| win-x64 | self-contained | ~70.50 МБ | Ні |
| win-x64 | framework-dependent | ~0.20 МБ (208 КБ) | Так |
| linux-x64 | self-contained | ~70.51 МБ | Ні |

Framework-dependent містить лише скомпільований код застосунку та залежності, тому потребує встановленого в системі .NET Runtime.

Self-contained містить код застосунку разом із повним середовищем виконання .NET, тому працює автономно на цільовій ОС без попередньо встановленого .NET.

## Середовище
.NET SDK 10.0.11, Windows 11 x64  

### Опис доменної моделі
* **BookCopy** - сутність, що інкапсулює фізичний стан примірника книги (`Id`, `Isbn`, стан видачі `IsIssued`). Зміна стану можлива виключно через методи `Issue()` та `Return()`.  
* **Loan** - сутність, що фіксує процес видачі книги читачеві (`Id`, `BookCopyId`, `ReaderId`, `IssuedOn`, `ReturnedOn`, `Status`).  
* **DTO vs Domain**: типи DTO (`BookCopyDto`, `LoanDto`) використовуються суто для передачі даних, тоді як сутності домену захищають бізнес-правила та забороняють створення чи переведення об'єкта в некоректний стан. Створення реалізовано через приватні конструктори та статичні фабричні методи `Create` / `Open`.  

### Реалізовані інваріанти  

1. **Обов'язковість рядкових ідентифікаторів** (`ArgumentException`):  Поля `Id`, `Isbn` у `BookCopy` та `Id`, `ReaderId` у `Loan` не можуть бути порожніми або складатись лише з пробілів.  
2. **Неможливість повторної видачі** (`InvalidOperationException`): Якщо примірник має статус `IsIssued == true`, повторна спроба викликати `BookCopy.Issue()` або відкрити нову видачу `Loan.Open` заборонена.  
3. **Неможливість повернення не виданого примірника** (`InvalidOperationException`): Не можна викликати `BookCopy.Return()`, якщо примірник уже має статус `IsIssued == false`.  
4. **Хронологічна послідовність дат** (`ArgumentOutOfRangeException`): Дата повернення книги (`ReturnedOn`) не може передувати даті видачі книги (`IssuedOn`).  
5. **Неможливість модифікації завершеної видачі** (`InvalidOperationException`): Повторна спроба викликати `Close()` на вже закритій видачі заборонена. 

### Додаткові інваріанти та правила

1. **Обмеження на кількість активних видач читача** (`InvalidOperationException`): Доменний сервіс `LoanService` забороняє видачу примірника читачеві, якщо в нього вже відкрито 5 активних позик.  
2. **Контроль переходів статусів видачі** (`InvalidOperationException`): Зміна життєвого циклу через `LoanStatus` регулюється `switch expression`: перехід із закритого стану (`Returned` / `Lost`) назад у статус `Active` є неможливим.   

## Сервісний шар і сховища (ЛР5)

**Головна сутність:** `BookCopy` (примірник книги). Сховище тримає видання і примірники в одному списку DTO: кожен `BookCopyDto` містить і дані видання (`Isbn`, `Title`), і стан примірника (`Id`, `IsIssued`).

**Контракт `IBookStore`** (лише те, що потрібно сервісу):
* `List()` — показати всі примірники; повертає `IReadOnlyList`, щоб зовнішній код не змінював сховище в обхід `Add/Update/Remove`.
* `GetById(id)` — знайти примірник; `BookCopy?`, бо запису може не бути.
* `Add(item)` — додати новий примірник; сховище перевіряє лише унікальність id.
* `Update(item)` — зберегти зміну стану після `Issue()` / `Return()`.
* `Remove(id)` — списати примірник; `bool` показує, чи був такий запис.

**Дві реалізації:**
* `InMemoryBookStore` — `Dictionary` в пам'яті, початкові дані (18 примірників із `SampleData`) передаються через конструктор. Після виходу з програми дані зникають.
* `FileBookStore` — той самий контракт, інша механіка: кеш у пам'яті, ліниве завантаження файлу при першому зверненні, запис на диск після кожної зміни. Формат — JSON-масив `BookCopyDto`; мапінг робить `BookCopy.FromDto` / `ToDto`, тому формат файлу не диктує форму домену. Кирилиця у файлі екранується як `\u0410…` — це штатна поведінка `System.Text.Json`.

**Сервіс `LendingService`** приймає `IBookStore` у конструкторі й не знає, де лежать дані. Операції: `AddBook`, `IssueCopy`, `ReturnCopy`, `All`, `Find`, `Search`.

**Схема залежностей:**
```text
Cli (Program.cs) → LendingService → IBookStore → InMemoryBookStore | FileBookStore
                                              ↖ CachingBookStore (декоратор над будь-якою реалізацією)
```

**Composition root** (`src/Cli/Program.cs`) — єдине місце з конкретними класами:
```csharp
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "library.json");
IBookStore store = useFile
    ? new FileBookStore(dataPath)
    : new InMemoryBookStore(SampleData.BookCopies());
var service = new LendingService(store);
```
`Microsoft.Extensions.DependencyInjection` не підключено: залежності передаються вручну через `new`.

**Сценарій відмови:** видача / повернення неіснуючого id, додавання примірника з уже наявним id, повторна видача — винятки перехоплюються в `Cli` і друкуються зрозумілим текстом.

**Що зміниться з БД (ЛР11):** з'явиться ще одна реалізація, наприклад `EfBookStore : IBookStore` поверх `DbContext`, і в composition root зміниться один рядок `new ...`. `BookCopy`, `IBookStore` і `LendingService` залишаться без змін.

<details>
<summary><b>Додаткові завдання</b></summary>

**Лабораторна робота 1**  
Розмір win-x64:  70.5  MB  
Розмір linux-x64:  70.51  MB  
**Лабораторна робота 2**  
**PublishSingleFile**: Виконано
```
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/single-file
```  
**PublishTrimmed**: Виконано  
```
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishTrimmed=true -o ./publish/trimmed
```
**Multi-Targeting та умовна компіляці**: Виконано  
**Крос-компіляція під RID іншої ОС**: Виконано (В таблиці додано порівняння)
**Лабораторна робота 3**  
JSON-імпортер (BookJsonImporter): Виконано  
Різнорідні рядки (MixedCsvImporter): Виконано  
Статистика в один рядок: Виконано  
**Лабораторна робота 4**  
Пакетна валідація з тижня 3: Виконано  
Інваріант на дві сутності: Виконано  
Керування станами: Виконано    
**Лабораторна робота 5**  
Декоратор `CachingBookStore`: Виконано. Приймає будь-який `IBookStore` у конструкторі, кешує `List()` і `GetById()`, скидає кеш після `Add/Update/Remove`, рахує влучання і промахи (`--cache`).  
Пошук з `Func<BookCopy, bool>`: Виконано. `LendingService.Search(predicate)`, наприклад `service.Search(c => !c.IsIssued)`.  
Фабрика `StoreFactory.Create(args)`: Виконано (`--factory`). Порівняння: варіант «усе в Program.cs» наочніший, бо весь вибір видно в точці входу; фабрика зручніша, коли комбінацій більше (файл, кеш, згодом БД), і її можна перевикористати в Api. Обидва варіанти лежать у проєкті Cli, тобто залишаються частиною composition root.  

</details>
