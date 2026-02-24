namespace ElectionGuard.UI.Views;

public partial class CreateElectionAdminPage
{
    public CreateElectionAdminPage(CreateElectionViewModel vm) : base(vm)
    {
        InitializeComponent();
    }

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        keyEntry.Text = ((KeyCeremonyRecord)keyPicker.SelectedItem).Name;
    }
}
