namespace CmsService.Api.Security;

public interface IUserContext
{
    string UserName { get; }
    bool IsAdmin { get; }
}