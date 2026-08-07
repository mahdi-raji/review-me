using System.Text.Json;

namespace ReviewMe;

public class ConfigService
{
    private static readonly string ConfigDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReviewMe");

    private static readonly string ConfigPath = Path.Combine(ConfigDirectory, "config.json");

    public ApplicationConfig GetOrCreateConfig()
    {
        string json;

        if (File.Exists(ConfigPath))
        {
            json = File.ReadAllText(ConfigPath);

            return JsonSerializer.Deserialize<ApplicationConfig>(json);
        }

        Directory.CreateDirectory(ConfigDirectory);

        var config = new ApplicationConfig
        {
            NotSafeWords =
            [
                "password",
                "username",
                "connection string"
            ],
            IgnoredFiles = []
        };

        json = JsonSerializer.Serialize(config);

        File.WriteAllText(ConfigPath, json);

        return config;
    }
}

public class ApplicationConfig
{
    public List<string> NotSafeWords { get; set; } = [];
    public List<string> IgnoredFiles { get; set; } = [];
}