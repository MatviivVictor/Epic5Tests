using System.Net;
using Epic5Task.Application.Exceptions;
using Epic5Task.Application.Interfaces;

namespace Epic5Task.Api.Middlewares;

public sealed class PhoneNumberHeaderMiddleware
{
    public const string HeaderName = "x-phone-number";

    private readonly RequestDelegate _next;

    public PhoneNumberHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IUserContextProvider userContext)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var values) ||
            string.IsNullOrWhiteSpace(values.ToString()))
        {
            throw new AuthZException("Phone number header is missing or empty");
        }

        userContext.UserPhoneNumber = values.ToString();
        
        await _next(context);
    }
}