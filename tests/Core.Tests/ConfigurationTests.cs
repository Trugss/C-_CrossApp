using Core.Configuration;

namespace Core.Tests;

public sealed class ConfigurationTests : IDisposable
{
    private readonly TempDirectory _temp = new();

    public void Dispose() => _temp.Dispose();

    [Fact]
    public void ResolveFullPath_RelativeDataPath_IsUnderBaseDirectory_AndDirectoryIsCreated()
    {
        var storage = new StorageOptions { DataPath = "data", FileName = "library.json" };

        var path = storage.ResolveFullPath(_temp.Path);

        Assert.Equal(Path.Combine(_temp.Path, "data", "library.json"), path);
        Assert.True(Directory.Exists(Path.Combine(_temp.Path, "data")));
    }

    [Fact]
    public void ResolveFullPath_RootedDataPath_IgnoresBaseDirectory()
    {
        var root = Path.Combine(_temp.Path, "absolute");
        var storage = new StorageOptions { DataPath = root };

        var path = storage.ResolveFullPath(Path.Combine(_temp.Path, "other"));

        Assert.Equal(Path.Combine(root, "library.json"), path);
    }

    [Fact]
    public void ResolveRoot_WithLocalAppData_UsesSpecialFolder()
    {
        var storage = new StorageOptions { UseLocalAppData = true };
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StorageOptions.ApplicationFolder, "data");

        Assert.Equal(Path.GetFullPath(expected), storage.ResolveRoot(_temp.Path));
    }

    [Fact]
    public void Validate_DefaultOptions_HasNoErrors()
    {
        Assert.Empty(AppOptionsValidator.Validate(new AppOptions()));
    }

    [Theory]
    [InlineData("", "library.json", "json", "Storage:DataPath")]
    [InlineData("data", "", "json", "Storage:FileName")]
    [InlineData("data", "library.txt", "json", ".json")]
    [InlineData("data", "library.json", "xml", "Storage:Format")]
    public void Validate_BadStorageOptions_ReturnsReadableError(string dataPath, string fileName, string format, string expected)
    {
        var options = new AppOptions { Storage = new StorageOptions { DataPath = dataPath, FileName = fileName, Format = format } };

        var error = Assert.Single(AppOptionsValidator.Validate(options));

        Assert.Contains(expected, error);
    }
}
