namespace Phonebook.WebApi.Security;

public class SecurityHeader
{
    private readonly RequestDelegate next;

    public SecurityHeader(RequestDelegate next)
    {
        this.next = next;
    }
    public Task Invoke(HttpContext context)
    {
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; style-src 'self' 'unsafe-inline';");
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer", "strict-origin-when-cross-origin");
        return next(context);
    }
}
