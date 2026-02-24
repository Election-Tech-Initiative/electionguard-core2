using CommunityToolkit.Mvvm.DependencyInjection;

namespace ElectionGuard.UI.Views;

public partial class ChallengedPopup
{
    public const string CurrentElectionIdParam = "ElectionId";

    public string ElectionId { get; set; } = string.Empty;

    public ChallengedPopup()
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService(typeof(ChallengedPopupViewModel));
        BindingContext = vm;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Close();
    }
}
