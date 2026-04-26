using Microsoft.AspNetCore.Authorization;

namespace Api.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class AuthorizeWithErrorAttribute : AuthorizeAttribute
{
    public string ErrorCode { get; }

    public AuthorizeWithErrorAttribute(string errorCode = "Auth.Unauthorized")
    {
        ErrorCode = errorCode;
    }
}