namespace ElectionGuard.UI.Views;

public class ContentPageBase<TViewModel> : ContentPage where TViewModel : BaseViewModel
{
    protected readonly TViewModel ViewModel;

    public ContentPageBase(TViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = ViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.AppearingCommand.ExecuteAsync(null);
    }
}
