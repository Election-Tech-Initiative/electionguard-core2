namespace ElectionGuard.UI.Behaviors;

public partial class IntegerValidationBehavior : Behavior<Entry>
{
    public static readonly BindableProperty AttachBehaviorProperty =
        BindableProperty.CreateAttached("AttachBehavior", typeof(bool), typeof(IntegerValidationBehavior), false, propertyChanged: OnAttachBehaviorChanged);

    public static bool GetAttachBehavior(BindableObject view)
    {
        return (bool)view.GetValue(AttachBehaviorProperty);
    }

    public static void SetAttachBehavior(BindableObject view, bool value)
    {
        view.SetValue(AttachBehaviorProperty, value);
    }

    private static void OnAttachBehaviorChanged(BindableObject view, object oldValue, object newValue)
    {
        Entry? entry = view as Entry;
        if (entry == null)
        {
            return;
        }

        bool attachBehavior = (bool)newValue;
        if (attachBehavior)
        {
            entry.TextChanged += OnEntryTextChanged;
        }
        else
        {
            entry.TextChanged -= OnEntryTextChanged;
        }
    }

    private static void OnEntryTextChanged(object? sender, TextChangedEventArgs args)
    {
        if (sender == null || string.IsNullOrWhiteSpace(args.NewTextValue))
        {
            return;
        }

        if (!args.NewTextValue.All(char.IsDigit))
        {
            ((Entry)sender).Text = args.OldTextValue;
        }
    }
}