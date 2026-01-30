namespace CmsService.Api.Security;

public class UserContext(string userName) : IUserContext
{
    public string UserName { get; } = userName;
    public bool IsAdmin { get; } = userName == "admin";
}