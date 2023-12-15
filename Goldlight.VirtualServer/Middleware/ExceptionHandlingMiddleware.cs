using Goldlight.ExceptionManagement;

namespace Goldlight.VirtualServer.Middleware;

public class ExceptionHandlingMiddleware
{
  private readonly RequestDelegate next;
  private readonly ILogger<ExceptionHandlingMiddleware> logger;

  public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
  {
    this.next = next;
    this.logger = logger;
  }

  // ReSharper disable once UnusedMember.Global
  public async Task InvokeAsync(HttpContext httpContext)
  {
    try
    {
      await next(httpContext);
    }
    catch (Exception ex)
    {
      await HandleException(ex, httpContext);
    }
  }

  private async Task HandleException(Exception ex, HttpContext httpContext)
  {
    switch (ex)
    {
      case UserNotMemberOfOrganizationException:
        await WriteResponse(httpContext, 401, "The user it not present in the organization");
        break;
      case SaveConflictException:
        await WriteResponse(httpContext, 409, "There was a conflict saving this record.");
        break;
      case ForbiddenException:
        await WriteResponse(httpContext, 403, "Forbidden");
        break;
      case InvalidOperationException:
        await WriteResponse(httpContext, 400, "Invalid Operation");
        break;
      case ArgumentException:
        await WriteResponse(httpContext, 400, "Invalid Argument");
        break;
      default:
        logger.LogError(ex, "An unknown error was detected");
        await WriteResponse(httpContext, 500, "Unknown Error");
        break;
    }
  }

  private async Task WriteResponse(HttpContext context, int statusCode, string message)
  {
    context.Response.StatusCode = statusCode;
    await context.Response.WriteAsJsonAsync(new HttpResponseModel(message, statusCode));
  }
}