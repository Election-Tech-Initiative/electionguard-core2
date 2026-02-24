namespace ElectionGuard.UI.Helpers;

public static class ServiceProviderExtension
{
    public static T GetInstance<T>(this IServiceProvider serviceProvider)
    {
        var instance = serviceProvider.GetService(typeof(T));
        if (instance == null)
        {
            throw new ArgumentException($"{nameof(T)} is not registered with DI");
        }
        return (T)instance;
    }

}
