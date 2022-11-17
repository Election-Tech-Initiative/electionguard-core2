using System.Resources;
using System.ComponentModel;
using System.Globalization;

namespace ElectionGuard.UI.Helpers;

public class LocalizationResourceManager : ObservableObject
{
    static readonly Lazy<LocalizationResourceManager> currentHolder = new Lazy<LocalizationResourceManager>(() => new LocalizationResourceManager());

    public static LocalizationResourceManager Current => currentHolder.Value;

    private ResourceManager? resourceManager;
    CultureInfo currentCulture = Thread.CurrentThread.CurrentUICulture;

    LocalizationResourceManager()
    {
    }

    public void Init(ResourceManager resource) => resourceManager = resource;

    public void Init(ResourceManager resource, CultureInfo initialCulture)
    {
        CurrentCulture = initialCulture;
        Init(resource);
    }

    public string GetValue(string text)
    {
        if (resourceManager == null)
            throw new InvalidOperationException($"Must call {nameof(LocalizationResourceManager)}.{nameof(Init)} first");

        return resourceManager.GetString(text, CurrentCulture) ?? throw new NullReferenceException($"{nameof(text)}: {text} not found");
    }

    public string this[string text] => GetValue(text);

    public CultureInfo CurrentCulture
    {
        get => currentCulture;
        set => SetProperty(ref currentCulture, value, null);
    }

}
