using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI.Lib.Models
{
    public partial class TallyJoinedRecord : DatabaseRecord
    {
        [ObservableProperty]
        private string _tallyId = string.Empty;

        [ObservableProperty]
        private string _guardianId = string.Empty;

        [ObservableProperty]
        private DateTime _joined = DateTime.UtcNow;
        

        public TallyJoinedRecord() : base(nameof(TallyJoinedRecord))
        {
        }
    }
}
