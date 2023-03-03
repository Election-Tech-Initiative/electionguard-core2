namespace ElectionGuard.UI.Lib.Services;

public interface IAuthenticationService
{
    Task Login(string username);
    string? UserName { get; }
    bool IsAdmin { get; }
}
