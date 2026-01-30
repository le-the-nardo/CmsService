namespace CmsService.Api.Security;

public class UserContextAccessor : IUserContextAccessor
{
    private IUserContext? _current;

    public IUserContext Current =>
        _current ?? throw new InvalidOperationException("User not set");

    public void SetUser(string username)
    {
        _current = new UserContext(username);
    }
}