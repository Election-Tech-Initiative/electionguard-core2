namespace ElectionGuard.UI.Lib.Models
{
    public partial class ChallengeResponseRecord : DatabaseRecord
    {
        [ObservableProperty]
        private string _tallyId = string.Empty;

        [ObservableProperty]
        private string _guardianId = string.Empty;

        [ObservableProperty]
        private string _responseData = string.Empty;

        public ChallengeResponseRecord() : base(nameof(ChallengeResponseRecord))
        {
        }

        public override string ToString() => ResponseData ?? string.Empty;
        public static implicit operator string(ChallengeResponseRecord? record) => record?.ToString() ?? string.Empty;
    }
}
