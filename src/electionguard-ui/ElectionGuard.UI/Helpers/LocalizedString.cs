using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI.Helpers;

public class LocalizedString : ObservableObject
{
    readonly Func<string> generator;

    public LocalizedString(Func<string> generator)
        : this(LocalizationResourceManager.Current, generator)
    {
    }

    public LocalizedString(LocalizationResourceManager localizationManager, Func<string> generator)
    {
        this.generator = generator;

        // This instance will be unsubscribed and GCed if no one references it
        // since LocalizationResourceManager uses WeakEventManger
        localizationManager.PropertyChanged += (sender, e) => OnPropertyChanged((string?)null);
    }

    public string Localized => generator();

    public static implicit operator LocalizedString(Func<string> func) => new LocalizedString(func);
}
