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
│   │   └── EnvironmentInfo.cs
│   └── Cli/
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

</details>
