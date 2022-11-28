namespace ElectionGuard.UI.Views;

public partial class NoContentView
{
	public NoContentView()
	{
		InitializeComponent();
        BindingContext = this;
	}

    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(NoContentView));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}