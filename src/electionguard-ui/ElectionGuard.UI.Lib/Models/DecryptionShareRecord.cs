using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI.Lib.Models
{
    public partial class DecryptionShareRecord : DatabaseRecord
    {

        [ObservableProperty]
        private string _tallyId = string.Empty;

        [ObservableProperty]
        private string _guardianId = string.Empty;

        [ObservableProperty]
        private string _shareData = string.Empty;

        public DecryptionShareRecord() : base(nameof(DecryptionShareRecord))
        {
        }

        public override string ToString() => ShareData ?? string.Empty;
        public static implicit operator string(DecryptionShareRecord? record) => record?.ToString() ?? string.Empty;
    }
}
