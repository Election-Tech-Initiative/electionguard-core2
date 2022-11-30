using System.Resources;
using System.Globalization;

namespace ElectionGuard.UI.Helpers;

public class LocalizationResourceManager : ObservableObject
{
    public static LocalizationResourceManager Current => CurrentHolder.Value;

    private static readonly Lazy<LocalizationResourceManager> CurrentHolder = new(() => new LocalizationResourceManager());

    private ResourceManager? _resourceManager;

    private CultureInfo _currentCulture = Thread.CurrentThread.CurrentUICulture;

    LocalizationResourceManager()
    {
    }

    public void Init(ResourceManager resource) => _resourceManager = resource;

    public void Init(ResourceManager resource, CultureInfo initialCulture)
    {
        CurrentCulture = initialCulture;
        Init(resource);
    }

    public string GetValue(string text)
    {
        if (_resourceManager == null)
            throw new InvalidOperationException($"Must call {nameof(LocalizationResourceManager)}.{nameof(Init)} first");

        return _resourceManager.GetString(text, CurrentCulture) ?? $"*** {text} not found ***";
    }

    public string this[string text] => GetValue(text);

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        set => SetProperty(ref _currentCulture, value, null);
    }

}
