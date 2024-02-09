using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Primitives;

public class AuthMiddlewareTests
{
    private async Task<HttpContext> InvokeMiddlewareAsync(Dictionary<string, StringValues> queryStringParams)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Query = new QueryCollection(queryStringParams);
        request.Path = "/";
        var response = new MemoryStream();
        context.Response.Body = response;

        var middleware = new AuthMiddleware(next: (innerHttpContext) =>
        {
            innerHttpContext.Response.StatusCode = 200; // status OK
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);
        return context;
    }

    [Fact]
    // no username or password -> not authorized
    public async Task Middleware_Should_ReturnNotAuthorized_When_NoCredentials()
    {
        var context = await InvokeMiddlewareAsync(new Dictionary<string, StringValues>());
        Assert.Equal(401, context.Response.StatusCode);
    }

    // only username provided -> not authorized
    [Fact]
    public async Task Middleware_Should_ReturnNotAuthorized_When_OnlyUsernameProvided()
    {
        var context = await InvokeMiddlewareAsync(new Dictionary<string, StringValues>
        {
            { "username", new StringValues("user1") }
        });
        Assert.Equal(401, context.Response.StatusCode);
    }

    // only password provided -> not authorized
    [Fact]
    public async Task Middleware_Should_ReturnAuthorized_When_CorrectCredentials()
    {
        var context = await InvokeMiddlewareAsync(new Dictionary<string, StringValues>
        {
            { "username", new StringValues("user1") },
            { "password", new StringValues("password1") }
        });
        Assert.Equal(200, context.Response.StatusCode);
    }
    
    // wrong username -> not authorized
    [Fact]
    public async Task Middleware_Should_ReturnNotAuthorized_When_WrongCredentials()
    {
        var context = await InvokeMiddlewareAsync(new Dictionary<string, StringValues>
        {
            { "username", new StringValues("user5") },
            { "password", new StringValues("password2") }
        });
        Assert.Equal(401, context.Response.StatusCode);
    }
}
