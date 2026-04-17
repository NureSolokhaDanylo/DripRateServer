namespace Api.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ApiErrorsAttribute : Attribute
{
    public int StatusCode { get; }
    public string[] Codes { get; }

    public ApiErrorsAttribute(int statusCode, params string[] codes)
    {
        StatusCode = statusCode;
        Codes = codes;
    }
}
