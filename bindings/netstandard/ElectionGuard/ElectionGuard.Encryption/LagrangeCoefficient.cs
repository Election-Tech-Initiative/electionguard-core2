using System.Collections.Generic;
using System.Linq;
using ElectionGuard.Guardians;

namespace ElectionGuard
{
    /// <summary>
    /// A data structure for keeping track of the lagrange coefficients
    /// </summary>
    public class LagrangeCoefficient : DisposableBase
    {
        public string GuardianId { get; set; }
        public ulong SequenceOrder { get; set; }
        public ElementModQ Coefficient { get; set; }

        public LagrangeCoefficient(
            string guardianId,
            ulong sequenceOrder,
            ElementModQ coefficient)
        {
            GuardianId = guardianId;
            SequenceOrder = sequenceOrder;
            Coefficient = coefficient;
        }

        public LagrangeCoefficient(
            IElectionGuardian guardian,
            ElementModQ coefficient)
        {
            GuardianId = guardian.GuardianId;
            SequenceOrder = guardian.SequenceOrder;
            Coefficient = coefficient;
        }

        public LagrangeCoefficient(
            ElectionPublicKey guardian,
            ElementModQ coefficient)
        {
            GuardianId = guardian.GuardianId;
            SequenceOrder = guardian.SequenceOrder;
            Coefficient = coefficient;
        }

        /// <summary>
        /// Computes the lagrange coefficients for the guardians
        /// </summary>
        public static List<LagrangeCoefficient> Compute(
         List<ElectionPublicKey> guardians)
        {
            var lagrangeCoefficients = new List<LagrangeCoefficient>();
            foreach (var guardian in guardians.OrderBy(x => x.SequenceOrder))
            {
                var otherSequenceOrders = guardians
                    .Where(i => i.GuardianId != guardian.GuardianId)
                    .Select(x => x.SequenceOrder).ToList();
                var lagrangeCoefficient = Polynomial.Interpolate(
                    guardian.SequenceOrder, otherSequenceOrders
                    );
                lagrangeCoefficients.Add(
                    new LagrangeCoefficient(
                        guardian, lagrangeCoefficient));
            }
            return lagrangeCoefficients;
        }
    }
}
