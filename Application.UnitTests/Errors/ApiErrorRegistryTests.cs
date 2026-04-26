using Domain.Errors;
using Xunit;

namespace Application.UnitTests.Errors;

public sealed class ApiErrorRegistryTests
{
    [Fact]
    public void AllDomainErrorCodes_MustBeRegistered()
    {
        var codes = typeof(AuthErrors).Assembly
            .GetTypes()
            .Where(t => t.IsClass && t.IsAbstract && t.IsSealed && t.Namespace == "Domain.Errors" && t.Name.EndsWith("Errors", StringComparison.Ordinal))
            .SelectMany(t => t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            .Where(f => f.FieldType == typeof(string) && f.Name.EndsWith("Code", StringComparison.Ordinal))
            .Select(f => (Code: (string?)f.GetValue(null), Source: $"{f.DeclaringType!.Name}.{f.Name}"))
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .Select(x => (x.Code!, x.Source))
            .ToList();

        var missing = codes
            .Where(x => !ApiErrorRegistry.TryGet(x.Item1, out _))
            .Select(x => $"{x.Source} -> {x.Item1}")
            .ToList();

        Assert.Empty(missing);
    }
}
