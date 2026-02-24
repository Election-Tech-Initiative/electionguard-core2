namespace ElectionGuard.UI.Views;

public partial class CreateMultiTallyPage
{
	public CreateMultiTallyPage(CreateMultiTallyViewModel vm) : base(vm)
	{
		InitializeComponent();
	}

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        keyEntry.Text = ((KeyCeremonyRecord)keyPicker.SelectedItem).Name;
    }

}
