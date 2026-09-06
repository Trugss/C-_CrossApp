using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var information = new
{
    App = "CrossApp - практикум з крос-платформного програмування",
    Student = "Марцічук Олександр",
    Group = "ФеІ-34c",
    OsDescription = RuntimeInformation.OSDescription,
    OsVersion = Environment.OSVersion.ToString(),
    Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
    NETVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    BaseDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Бібліотека (Видання, Примірник, Читач, Видача)"
};

if (args.Contains("--json", StringComparer.OrdinalIgnoreCase))
{
    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    Console.WriteLine(JsonSerializer.Serialize(information, jsonOptions));
}
else
{
    Console.WriteLine(information.App);
    Console.WriteLine($"Студент: {information.Student}, Група {information.Group}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"OC (OSDescription): {information.OsDescription}");
    Console.WriteLine($"OC (Environment): {information.OsVersion}");
    Console.WriteLine($"Архітектура процесу: {information.Architecture}");
    Console.WriteLine($"Версія .NET: {information.NETVersion}");
    Console.WriteLine($"Runtime: {information.Runtime}");
    Console.WriteLine($"Каталог застосунку: {information.BaseDirectory}");
    Console.WriteLine($"Поточний каталог: {information.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область: {information.Domain}");
}