namespace CmsService.Api.Security;

public interface IUserContextAccessor
{
    IUserContext Current { get; }
    void SetUser(string username);
}