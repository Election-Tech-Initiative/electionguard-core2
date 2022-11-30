namespace ElectionGuard.UI.Lib.Services;

public interface IAuthenticationService
{
    Task Login(string name);
    string? UserName { get; }
}
