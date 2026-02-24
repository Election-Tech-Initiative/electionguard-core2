namespace ElectionGuard.UI.Controls;

public partial class ExpanderControl : ContentView
{
	public ExpanderControl()
	{
		InitializeComponent();
	}

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(ExpanderControl));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }


}
