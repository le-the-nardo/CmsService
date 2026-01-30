using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using CmsService.Api.Auth.Options;
using CmsService.Api.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CmsService.Api.Auth;

public class UserBasicAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly UserBasicAuthOptions _options;

    public UserBasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> schemeOptions,
        IOptions<UserBasicAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUserContextAccessor userContextAccessor)
        : base(schemeOptions, logger, encoder)
    {
        _userContextAccessor = userContextAccessor;
        _options = options.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(
                AuthenticateResult.Fail("Missing Authorization Header"));

        var authHeader = AuthenticationHeaderValue.Parse(
            Request.Headers["Authorization"]!);

        var credentials = Encoding.UTF8
            .GetString(Convert.FromBase64String(authHeader.Parameter!))
            .Split(':');

        if (credentials.Length != 2)
            return Task.FromResult(
                AuthenticateResult.Fail("Invalid Authorization Header"));

        var username = credentials[0];
        var password = credentials[1];

        var isUser =
            username.Equals(_options.Username) &&
            password.Equals(_options.Password);

        var isAdmin =
            username.Equals(_options.AdminUsername) &&
            password.Equals(_options.AdminPassword);

        if (!isUser && !isAdmin)
        {
            return Task.FromResult(
                AuthenticateResult.Fail("Invalid credentials"));
        }

        _userContextAccessor.SetUser(username);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
