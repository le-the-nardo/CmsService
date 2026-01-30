using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using CmsService.Api.Auth;
using CmsService.Api.Auth.Options;
using CmsService.Api.Security;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CmsService.Tests.Api.Auth;

public class UserBasicAuthenticationHandlerTests
{
    private UserBasicAuthenticationHandler CreateHandler(
        DefaultHttpContext context,
        UserBasicAuthOptions options,
        Mock<IUserContextAccessor> userContextMock)
    {
        var scheme = new AuthenticationScheme(
            "Basic",
            "Basic",
            typeof(UserBasicAuthenticationHandler));

        var optionsMonitor =
            Mock.Of<IOptionsMonitor<AuthenticationSchemeOptions>>(
                o => o.Get(It.IsAny<string>()) == new AuthenticationSchemeOptions());

        var authOptions =
            Options.Create(options);

        var loggerFactory = LoggerFactory.Create(builder => { });

        var handler = new UserBasicAuthenticationHandler(
            optionsMonitor,
            authOptions,
            loggerFactory,
            UrlEncoder.Default,
            userContextMock.Object
        );

        handler.InitializeAsync(scheme, context).Wait();

        return handler;
    }
    
    [Fact]
    
    public async Task Should_fail_when_header_authorization_doesnt_exist()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var userContext = new Mock<IUserContextAccessor>();

        var handler = CreateHandler(
            context,
            new UserBasicAuthOptions(),
            userContext);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure!.Message.Should().Be("Missing Authorization Header");
    }

    [Fact]
    
    public async Task Should_fail_when_invalid_credentials()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var authValue = Convert.ToBase64String(
            Encoding.UTF8.GetBytes("user:wrong"));

        context.Request.Headers["Authorization"] =
            new AuthenticationHeaderValue("Basic", authValue).ToString();

        var options = new UserBasicAuthOptions
        {
            Username = "user",
            Password = "123"
        };

        var userContext = new Mock<IUserContextAccessor>();

        var handler = CreateHandler(context, options, userContext);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure!.Message.Should().Be("Invalid credentials");
    }

    [Fact]
    
    public async Task Should_authenticate_common_user_with_success()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var authValue = Convert.ToBase64String(
            Encoding.UTF8.GetBytes("user:123"));

        context.Request.Headers["Authorization"] =
            new AuthenticationHeaderValue("Basic", authValue).ToString();

        var options = new UserBasicAuthOptions
        {
            Username = "user",
            Password = "123"
        };

        var userContext = new Mock<IUserContextAccessor>();

        var handler = CreateHandler(context, options, userContext);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Principal!.Identity!.Name.Should().Be("user");

        userContext.Verify(
            x => x.SetUser("user"),
            Times.Once);
    }

    [Fact]
    
    public async Task Should_authenticate_admin_with_success()
    {
        var context = new DefaultHttpContext();

        var authValue = Convert.ToBase64String(
            Encoding.UTF8.GetBytes("admin:admin123"));

        context.Request.Headers["Authorization"] =
            new AuthenticationHeaderValue("Basic", authValue).ToString();

        var options = new UserBasicAuthOptions
        {
            AdminUsername = "admin",
            AdminPassword = "admin123"
        };

        var userContext = new Mock<IUserContextAccessor>();

        var handler = CreateHandler(context, options, userContext);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.Should().BeTrue();
        result.Principal!.Identity!.Name.Should().Be("admin");
    }

}