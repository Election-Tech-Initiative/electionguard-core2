using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace ElectionGuard
{
    /// <summary>
    /// A Chaum-Pedersen proof
    /// </summary>
    /// <remarks>
    /// A Chaum-Pedersen proof is a proof that a value `m` is in the range `[0, q)`, where `q` is the
    /// order of the group of the public key. It is used to prove that a value `m` is the plaintext
    /// of a ciphertext `c` encrypted under a public key `p` and a random nonce `k`.
    ///
    /// The proof is a tuple `(c, k, m)`, where `c` is the ciphertext, `k` is the nonce, and `m` is the
    /// plaintext. The proof is valid if `c = p^m * g^k` and `m &lt; q`.
    /// </remarks>
    /// <note>
    /// This is a simplified version of the Chaum-Pedersen proof that does not include a challenge
    /// value. This is sufficient for our use case because the proof is only used to prove that a
    /// value is in the range `[0, q)`, which is a property of the value itself and not of the
    /// prover.
    /// </note>
    public class ChaumPedersenProof : DisposableBase
    {
        /// <Summary>
        /// a in the spec
        /// </Summary>
        public ElementModP Pad
        {
            get
            {
                var status = External.ChaumPedersenProof.GetPad(
                    Handle, out var value);
                status.ThrowIfError();
                return new ElementModP(value);
            }
        }

        /// <Summary>
        /// b in the spec
        /// </Summary>
        public ElementModP Data
        {
            get
            {
                var status = External.ChaumPedersenProof.GetData(
                    Handle, out var value);
                status.ThrowIfError();
                return new ElementModP(value);
            }
        }

        /// <Summary>
        /// c in the spec
        /// </Summary>
        public ElementModQ Challenge
        {
            get
            {
                var status = External.ChaumPedersenProof.GetChallenge(
                    Handle, out var value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <Summary>
        /// v in the spec
        /// </Summary>
        public ElementModQ Response
        {
            get
            {
                var status = External.ChaumPedersenProof.GetResponse(
                    Handle, out var value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        internal External.ChaumPedersenProof.ChaumPedersenProofHandle Handle;

        internal ChaumPedersenProof(External.ChaumPedersenProof.ChaumPedersenProofHandle handle)
        {
            Handle = handle;
        }

        public ChaumPedersenProof(ChaumPedersenProof other)
        {
            var commitment = new ElGamalCiphertext(other.Pad, other.Data);
            var status = External.ChaumPedersenProof.Make(
                commitment.Handle, other.Challenge.Handle, other.Response.Handle, out Handle);
            commitment.Dispose();
            status.ThrowIfError();
        }

        [JsonConstructor]
        public ChaumPedersenProof(ElementModP pad, ElementModP data, ElementModQ challenge, ElementModQ response)
        {
            var commitment = new ElGamalCiphertext(pad, data);
            var status = External.ChaumPedersenProof.Make(
                commitment.Handle, challenge.Handle, response.Handle, out Handle);
            commitment.Dispose();
            status.ThrowIfError();
        }

        public ChaumPedersenProof(
            ElGamalCiphertext commitment, ElementModQ challenge, ElementModQ response)
        {
            var status = External.ChaumPedersenProof.Make(
                commitment.Handle, challenge.Handle, response.Handle, out Handle);
            status.ThrowIfError();
        }

        public bool IsValid(
            ElGamalCiphertext message, ElementModP k, ElementModP m, ElementModQ q)
        {
            var consistentA = false;

            // Verification 9.2 - 𝑎 = 𝑔^𝑣 • 𝐾^𝑐 mod 𝑝
            using (var gv = BigMath.PowModP(Constants.G, Response))
            using (var kc = BigMath.PowModP(k, Challenge))
            using (var gvkc = BigMath.MultModP(gv, kc))
            {
                consistentA = Pad.Equals(gvkc);
            }

            if (!consistentA)
            {
                Console.WriteLine($"ChaumPedersenProof: Invalid A");
            }

            var consistentB = false;

            // Verification 9.3 - 𝑏 = 𝐴^𝑣 • 𝑀^𝑐 mod 𝑝
            using (var av = BigMath.PowModP(message.Pad, Response))
            using (var mc = BigMath.PowModP(m, Challenge))
            using (var avmc = BigMath.MultModP(av, mc))
            {
                consistentB = Data.Equals(avmc);
            }

            if (!consistentB)
            {
                Console.WriteLine($"ChaumPedersenProof: Invalid B");
            }

            return consistentA && consistentB;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override void DisposeUnmanaged()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            base.DisposeUnmanaged();

            if (Handle == null || Handle.IsInvalid) return;
            Handle.Dispose();
            Handle = null;
        }

        internal static unsafe class External
        {
            internal static class ChaumPedersenProof
            {
                internal struct ChaumPedersenProofType { };

                internal class ChaumPedersenProofHandle
                    : ElectionGuardSafeHandle<ChaumPedersenProofType>
                {
                    protected override bool Free()
                    {
                        if (IsClosed) return true;

                        var status = ChaumPedersenProof.Free(TypedPtr);
                        if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                        {
                            throw new ElectionGuardException($"ChaumPedersenProof Error Free: {status}", status);
                        }
                        return true;
                    }
                }

                [DllImport(NativeInterface.DllName,
                    EntryPoint = "eg_chaum_pedersen_proof_free",
                    CallingConvention = CallingConvention.Cdecl,
                    SetLastError = true)]
                internal static extern Status Free(ChaumPedersenProofType* handle);

                [DllImport(NativeInterface.DllName,
                    EntryPoint = "eg_chaum_pedersen_proof_get_pad",
                    CallingConvention = CallingConvention.Cdecl,
                    SetLastError = true)]
                internal static extern Status GetPad(
                    ChaumPedersenProofHandle handle,
                    out NativeInterface.ElementModP.ElementModPHandle element);

                [DllImport(NativeInterface.DllName,
                    EntryPoint = "eg_chaum_pedersen_proof_get_data",
                    CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
                internal static extern Status GetData(
                    ChaumPedersenProofHandle handle,
                    out NativeInterface.ElementModP.ElementModPHandle element);

                [DllImport(NativeInterface.DllName,
                    EntryPoint = "eg_chaum_pedersen_proof_get_challenge",
                    CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
                internal static extern Status GetChallenge(
                    ChaumPedersenProofHandle handle,
                    out NativeInterface.ElementModQ.ElementModQHandle element);

                [DllImport(NativeInterface.DllName,
                    EntryPoint = "eg_chaum_pedersen_proof_get_response",
                    CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
                internal static extern Status GetResponse(
                    ChaumPedersenProofHandle handle,
                    out NativeInterface.ElementModQ.ElementModQHandle element);

                [DllImport(NativeInterface.DllName,
                    EntryPoint = "eg_chaum_pedersen_proof_make",
                    CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
                internal static extern Status Make(
                    NativeInterface.ElGamalCiphertext.ElGamalCiphertextHandle commitment,
                    NativeInterface.ElementModQ.ElementModQHandle challenge,
                    NativeInterface.ElementModQ.ElementModQHandle response,
                    out ChaumPedersenProofHandle handle);

                [DllImport(NativeInterface.DllName,
                    EntryPoint = "eg_chaum_pedersen_proof_is_valid",
                    CallingConvention = CallingConvention.Cdecl,
                    SetLastError = true)]
                internal static extern bool IsValid(
                    ChaumPedersenProofHandle handle,
                    NativeInterface.ElGamalCiphertext.ElGamalCiphertextHandle message,
                    NativeInterface.ElementModP.ElementModPHandle k,
                    NativeInterface.ElementModP.ElementModPHandle m,
                    NativeInterface.ElementModQ.ElementModQHandle q);

            }
        }
    }
}
