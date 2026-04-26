namespace Api.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ApiErrorsAttribute : Attribute
{
    public string[] Codes { get; }

    public ApiErrorsAttribute(params string[] codes)
    {
        Codes = codes;
    }
}
