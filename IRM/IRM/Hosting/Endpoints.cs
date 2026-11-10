using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace IRM.Hosting;

internal static class Endpoints
{
    public static void MapIrmEndpoints(this WebApplication app)
    {
        app.MapGet("/health/live", () => Results.Ok(new
        {
            status = "ok",
            version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "unknown"
        })).AllowAnonymous();
        app.MapGet("/health/ready", async (IRM.Application.Abstractions.IDatabaseReadiness readiness, CancellationToken cancellationToken) =>
        {
            return await readiness.CanConnectAsync(cancellationToken)
                ? Results.Ok(new { status = "ready", database = "sqlserver", schema = readiness.SchemaVersion, schemaChecksum = readiness.SchemaChecksum })
                : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }).AllowAnonymous();

        app.MapPost("/auth/login", async (HttpContext context, IAuthService authService) =>
        {
            var form = await context.Request.ReadFormAsync(context.RequestAborted);
            var principal = await authService.AuthenticateWebAsync(form["username"].ToString(), form["password"].ToString(),
                context.Connection.RemoteIpAddress?.ToString(), context.RequestAborted);
            if (principal is null) return Results.LocalRedirect("/login?loginError=1");
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = false });
            return Results.LocalRedirect("/");
        }).AllowAnonymous();

        app.MapPost("/auth/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.LocalRedirect("/");
        }).RequireAuthorization();

    }
}
