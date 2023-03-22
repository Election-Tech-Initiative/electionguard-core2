namespace ElectionGuard.UI.Views;

public partial class CreateElectionAdminPage
{
	public CreateElectionAdminPage(CreateElectionViewModel vm) : base(vm)
	{
		InitializeComponent();
	}

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
//		var picker = (Picker)sender;
//		picker.TextColor = Colors.Black;
    }
}
