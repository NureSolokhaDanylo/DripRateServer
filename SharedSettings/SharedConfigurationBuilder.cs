using Microsoft.Extensions.Configuration;

namespace SharedSettings;

public static class SharedConfigurationBuilder
{
    public static IConfiguration Build()
    {
        var basePath = Path.GetDirectoryName(typeof(SharedConfigurationBuilder).Assembly.Location)!;


        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("sharedsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();
    }
}
