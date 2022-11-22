using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI.Helpers;

[ContentProperty(nameof(Text))]
public class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Text { get; set; } = string.Empty;

    public string? StringFormat { get; set; }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        #region Required work-around to prevent linker from removing the implementation
        if (DateTime.Now.Ticks < 0)
            _ = LocalizationResourceManager.Current[Text];
        #endregion

        var binding = new Binding
        {
            Mode = BindingMode.OneWay,
            Path = $"[{Text}]",
            Source = LocalizationResourceManager.Current,
            StringFormat = StringFormat
        };
        return binding;
    }
}
