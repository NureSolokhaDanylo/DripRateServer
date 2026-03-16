using System.Reflection.PortableExecutable;
using Microsoft.Extensions.Configuration;
using SharedSettings.Options;

namespace SharedSettings;

public static class SharedConfiguration
{
    public static IConfiguration GetConfiguration()
    {
        var basePath = Path.GetDirectoryName(typeof(SharedConfiguration).Assembly.Location)!;

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("sharedsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();
    }

    public static T GetIOptions2<T>(this IConfiguration configuration) where T : IOptions2, new()
    {
        var section = configuration.GetSection(new T().GetSectionName());
        var t = section.Get<T>();
        if (t is null) throw new NullReferenceException();
        return t;
    }

    public static T GetIOptions2<T>() where T : IOptions2, new()
    {
        return GetConfiguration().GetIOptions2<T>();
    }
}
