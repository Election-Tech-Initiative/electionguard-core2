namespace ElectionGuard.UI.Lib.Models
{
    public partial class TallyJoinedRecord : DatabaseRecord
    {
        [ObservableProperty]
        private string _tallyId = string.Empty;

        [ObservableProperty]
        private string _guardianId = string.Empty;

        [ObservableProperty]
        private bool _joined = false;

        [ObservableProperty]
        private DateTime _joinedDate = DateTime.UtcNow;

        public TallyJoinedRecord() : base(nameof(TallyJoinedRecord)) { }
    }
}
