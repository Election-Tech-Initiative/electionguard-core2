namespace ElectionGuard
{
    using System.Collections.Generic;
    using NativeElementModP = NativeInterface.ElementModP.ElementModPHandle;
    using NativeElementModQ = NativeInterface.ElementModQ.ElementModQHandle;
    // native types for convenience
    using NativeRangedChaumPedersenProof = NativeInterface.RangedChaumPedersenProof.RangedChaumPedersenProofHandle;

    /// <summary>
    /// The ranged Chaum Pedersen proof is a Non-Interactive Zero-Knowledge Proof that is a generalized version of the 
    /// Disjunctive Chaum Pedersen proof that allows for a variable number of selections to be proven.
    /// </summary>
    public class RangedChaumPedersenProof : DisposableBase
    {
        internal NativeRangedChaumPedersenProof Handle;
        public ulong RangeLimit
        {
            get
            {
                var status = NativeInterface.RangedChaumPedersenProof.GetRangeLimit(
                    Handle, out var value);
                status.ThrowIfError();

                return value;
            }
        }

        public ElementModQ Challenge
        {
            get
            {
                var status = NativeInterface.RangedChaumPedersenProof.GetChallenge(
                    Handle, out var value);
                status.ThrowIfError();

                return new ElementModQ(value);
            }
        }

        public Dictionary<ulong, ZeroKnowledgeProof> IntegerProofs => throw new System.NotImplementedException();

        public RangedChaumPedersenProof(
 ElGamalCiphertext message,
            ElementModQ r,
            ulong selected,
            ulong maxLimit,
            ElementModP k,
            ElementModQ q,
            ElementModQ seed
        )
        {
            var status = NativeInterface.RangedChaumPedersenProof.Make(
                message.Handle,
                r.Handle,
                selected,
                maxLimit,
                k.Handle,
                q.Handle,
                seed.Handle,
                out Handle);
            status.ThrowIfError();
        }

        public RangedChaumPedersenProof(
            ElGamalCiphertext message,
            ElementModQ r,
            ulong selected,
            ulong maxLimit,
            ElementModP k,
            ElementModQ q
        ) : this(
            message,
            r,
            selected,
            maxLimit,
            k,
            q,
            BigMath.RandQ()
        )
        {
        }

        public bool IsValid(ElGamalCiphertext ciphertext, ElementModP k, ElementModQ q, out string errorMessage)
        {
            errorMessage = string.Empty;

            var isValidResult = NativeInterface.RangedChaumPedersenProof.IsValid(
                Handle, ciphertext.Handle, k.Handle, q.Handle);

            if (isValidResult)
            {
                return isValidResult;
            }

            ExceptionHandler.GetData(out var function, out var message, out var _);
            errorMessage = $"status: {nameof(RangedChaumPedersenProof)} {nameof(IsValid)} function: {function} message: {message}";
            return isValidResult;
        }
    }
}
