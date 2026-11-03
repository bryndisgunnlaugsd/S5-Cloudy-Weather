namespace CloudyWeather.API.Environment;

public static class DotEnv
{
    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split(
                '=',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            if (System.Environment.GetEnvironmentVariable(parts[0]) != null)
                continue;

            System.Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
}