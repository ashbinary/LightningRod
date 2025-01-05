namespace LightningRod;

public static class Options
{
    private static readonly Dictionary<string, dynamic> _options = new();

    public static dynamic GetOption(string key) => _options[key];
    public static void SetOption(string key, dynamic value) => _options[key] = value;
}