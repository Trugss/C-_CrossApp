using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core;

Console.OutputEncoding = System.Text.Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();

if (args.Contains("--json", StringComparer.OrdinalIgnoreCase))
{
    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    Console.WriteLine(JsonSerializer.Serialize(report, jsonOptions));
}
else
{
    Console.WriteLine("CrossApp - інформація про середовище");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"OC (OSDescription): {report.OsDescription}");
    Console.WriteLine($"Архітектура процесу: {report.ProcessArchitecture}");
    Console.WriteLine($"Runtime: {report.FrameworkDescription}");
    Console.WriteLine($"Каталог застосунку: {report.BaseDirectory}");
    Console.WriteLine($"Визначений RID: {report.DetectedRid}");
    Console.WriteLine($"RID (Від .NET): {report.ReportedRid}");
    Console.WriteLine($"Примітка збірки: {report.BuildNote}");
    Console.WriteLine(new string('-', 52));
}