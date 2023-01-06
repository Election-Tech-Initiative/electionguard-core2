using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.ElectionSetup
{
    public class ElectionPartialKeyChallenge
    {
        public string OwnerId { get; set; }
        public string DesignatedId { get; set; }
        public ulong DesignatedSequenceOrder { get; set; }
        public ElementModQ Value { get; set; }
        public List<ElementModP> CoefficientCommitments { get; set; }
        public List<SchnorrProof> CoefficientProofs { get; set; }
    }
}
