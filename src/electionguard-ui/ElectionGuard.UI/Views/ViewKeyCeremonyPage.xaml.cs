namespace ElectionGuard.UI.Views;

public partial class ViewKeyCeremonyPage : IQueryAttributable
{
    private readonly ViewKeyCeremonyViewModel _viewKeyCeremonyViewModel;

    public ViewKeyCeremonyPage(ViewKeyCeremonyViewModel viewKeyCeremonyViewModel)
    {
        _viewKeyCeremonyViewModel = viewKeyCeremonyViewModel;
        InitializeComponent();
        BindingContext = viewKeyCeremonyViewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var keyCeremonyId = query[ViewKeyCeremonyViewModel.CurrentKeyCeremonyParam] as int?;
        if (keyCeremonyId == null)
            throw new ArgumentException("Need a keyCeremonyId to navigate to the view key ceremony page");
        _viewKeyCeremonyViewModel.RetrieveKeyCeremonyCommand.Execute(keyCeremonyId.Value);
    }
}