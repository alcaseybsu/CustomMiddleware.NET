using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var username = context.Request.Query["username"];
        var password = context.Request.Query["password"];

        if (username != "user1" || password != "password1")
        {
            context.Response.StatusCode = 401; // unauthorized
            await context.Response.WriteAsync("Not authorized.");
            return;
        }

        await _next(context);
    }
}
