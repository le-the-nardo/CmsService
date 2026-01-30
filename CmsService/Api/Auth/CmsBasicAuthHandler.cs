using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using CmsService.Api.Auth.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CmsService.Api.Auth;

public class CmsBasicAuthHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly CmsBasicAuthOptions _options;

    public CmsBasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> schemeOptions,
        IOptions<CmsBasicAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(schemeOptions, logger, encoder)
    {
        _options = options.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(
                AuthenticateResult.Fail("Missing Authorization Header"));

        var authHeader = AuthenticationHeaderValue.Parse(
            Request.Headers["Authorization"]);

        var credentials = Encoding.UTF8
            .GetString(Convert.FromBase64String(authHeader.Parameter!))
            .Split(':');

        if (credentials.Length != 2)
            return Task.FromResult(
                AuthenticateResult.Fail("Invalid Authorization Header"));

        var username = credentials[0];
        var password = credentials[1];

        if (username != _options.Username || password != _options.Password)
            return Task.FromResult(
                AuthenticateResult.Fail("Invalid credentials"));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("source", "cms")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}