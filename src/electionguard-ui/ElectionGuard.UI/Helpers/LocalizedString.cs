namespace ElectionGuard.UI.Helpers;

public class LocalizedString : ObservableObject
{
    readonly Func<string> _generator;

    public LocalizedString(Func<string> generator)
        : this(LocalizationResourceManager.Current, generator)
    {
    }

    public LocalizedString(LocalizationResourceManager localizationManager, Func<string> generator)
    {
        this._generator = generator;

        // This instance will be unsubscribed and GCed if no one references it
        // since LocalizationResourceManager uses WeakEventManger
        localizationManager.PropertyChanged += (_, _) => OnPropertyChanged((string?)null);
    }

    public string Localized => _generator();

    public static implicit operator LocalizedString(Func<string> func) => new(func);
}
