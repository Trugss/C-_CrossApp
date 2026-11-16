using Core.Storage;

namespace Core.Configuration;

public static class AppOptionsValidator
{
    public static IReadOnlyList<string> Validate(AppOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var errors = new List<string>();
        var storage = options.Storage;

        if (string.IsNullOrWhiteSpace(storage.DataPath))
            errors.Add("Storage:DataPath не може бути порожнім.");

        if (string.IsNullOrWhiteSpace(storage.FileName))
            errors.Add("Storage:FileName не може бути порожнім.");
        else if (!storage.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            errors.Add($"Storage:FileName має закінчуватися на .json, отримано '{storage.FileName}'.");
        else if (Path.GetFileName(storage.FileName) != storage.FileName)
            errors.Add($"Storage:FileName має бути лише іменем файлу без каталогів, отримано '{storage.FileName}'.");

        if (!CatalogExporters.Supported.Contains(storage.Format, StringComparer.OrdinalIgnoreCase))
            errors.Add($"Storage:Format має бути одним із: {string.Join(", ", CatalogExporters.Supported)}, отримано '{storage.Format}'.");

        if (options.Library.LoanDays is < 1 or > 365)
            errors.Add($"Library:LoanDays має бути від 1 до 365, отримано {options.Library.LoanDays}.");

        return errors;
    }
}
