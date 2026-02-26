using Epic5Task.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Epic5Task.Api;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware>
        logger, ProblemDetailsFactory problemDetailsFactory)
    {
        _next = next;
        _logger = logger;
        _problemDetailsFactory = problemDetailsFactory;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next.Invoke(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        ProblemDetails problemDetails;

        switch (exception)
        {
            case EntityNotFoundException entityNotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails = _problemDetailsFactory.CreateProblemDetails(httpContext: context,
                    title: "EntityNotFound",
                    detail: entityNotFoundException.Message,
                    statusCode: StatusCodes.Status404NotFound);
                break;
            case AuthZException unauthorizedAccessException:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                problemDetails = _problemDetailsFactory.CreateProblemDetails(httpContext: context,
                    title: "UnauthorizedError",
                    detail: exception.Message,
                    statusCode: StatusCodes.Status401Unauthorized);
                break;
            case EntityConflictException conflictException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails = _problemDetailsFactory.CreateProblemDetails(httpContext: context,
                    title: "EntityNotFound",
                    detail: conflictException.Message,
                    statusCode: StatusCodes.Status409Conflict);
                break;
            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                problemDetails = _problemDetailsFactory.CreateProblemDetails(httpContext: context,
                    title: "InternalServerError",
                    detail: exception.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
                break;
        }

        var json = JsonConvert.SerializeObject(problemDetails, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        return context.Response.WriteAsync(json);
    }
}