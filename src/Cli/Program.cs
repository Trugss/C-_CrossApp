using System.Runtime.InteropServices;

Console.WriteLine("CrossApp - практикум з крос-платформного програмування");
Console.WriteLine("Студент: Марцічук Олександр, Група ФеІ-34c");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"OC (OSDescription): {RuntimeInformation.OSDescription}");
Console.WriteLine($"OC (Environment): {Environment.OSVersion}");
Console.WriteLine($"Архітектура процесу: {RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"Версія .NET: {Environment.Version}");
Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Каталог застосунку: {AppContext.BaseDirectory}");
Console.WriteLine($"Поточний каталог: {Environment.CurrentDirectory}");

Console.WriteLine(new string('-', 52));
Console.WriteLine("Предметна область: Бібліотека (Видання, Примірник, Читач, Видача)");
