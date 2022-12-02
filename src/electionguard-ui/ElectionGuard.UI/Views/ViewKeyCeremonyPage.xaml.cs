namespace ElectionGuard.UI.Views;

public partial class ViewKeyCeremonyPage : IQueryAttributable
{
    public ViewKeyCeremonyPage(ViewKeyCeremonyViewModel viewKeyCeremonyViewModel) : base(viewKeyCeremonyViewModel)
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var keyCeremonyId = query[ViewKeyCeremonyViewModel.CurrentKeyCeremonyParam] as int?;
        if (keyCeremonyId == null)
            throw new ArgumentException("Need a keyCeremonyId to navigate to the view key ceremony page");
        ViewModel.RetrieveKeyCeremonyCommand.Execute(keyCeremonyId.Value);
    }
}