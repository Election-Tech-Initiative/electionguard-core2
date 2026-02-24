using Microsoft.Extensions.Logging;

namespace ElectionGuard.UI.Lib.Services;

public interface IAuthenticationService
{
    Task Login(string username, ILogger logger);
    string? UserName { get; }
    bool IsAdmin { get; }
}
