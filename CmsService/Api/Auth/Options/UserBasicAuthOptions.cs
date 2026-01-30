namespace CmsService.Api.Auth.Options;

public class UserBasicAuthOptions
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string AdminUsername { get; set; } = null!;
    public string AdminPassword { get; set; } = null!;
}