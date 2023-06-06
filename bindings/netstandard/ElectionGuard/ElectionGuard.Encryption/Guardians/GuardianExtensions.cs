
using System.Collections.Generic;
using ElectionGuard.Extensions;

namespace ElectionGuard.Guardians
{
    public static class GuardianExtensions
    {
        /// <summary>
        /// Compute the commitment offset for a given sequence order.
        ///
        /// this is the inner most product of the following equation 
        /// used to regenerate commitments
        /// 
        /// Π Π 𝐾j^𝑖^m mod 𝑝 
        /// </summary>
        public static ElementModP ComputeCommitmentOffset(
        this Dictionary<string, ElectionPublicKey> guardians,
        ulong sequenceOrder)
        {
            // Π 𝐾^𝑖^m mod 𝑝
            var commitmentOffset = new ElementModP(Constants.ONE_MOD_P);

            // for each guardian commitment
            // this can be computed aheadof time outside of this function
            foreach (var guardian in guardians)
            {
                // iterate over their commitments
                foreach (var (commitment, index) in guardian.Value.CoefficientCommitments.WithIndex())
                {
                    // and apply them to the product
                    using (var exponent = BigMath.PowModP(sequenceOrder, index)) // i^m
                    using (var factor = BigMath.PowModP(commitment, exponent)) // 𝐾^𝑖^m mod 𝑝
                    {
                        commitmentOffset = BigMath.MultModP(commitmentOffset, factor);
                    }
                }
            }
            return commitmentOffset;
        }
    }
}
