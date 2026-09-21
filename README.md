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
│   │   ├── Dto/
│   │   │   ├── BookDto.cs
│   │   │   ├── ReaderDto.cs
│   │   │   ├── ImportResult.cs
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

## Додаткове завдання

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