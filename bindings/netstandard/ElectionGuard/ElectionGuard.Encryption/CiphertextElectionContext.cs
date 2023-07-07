using System;
using System.Text;
// ReSharper disable UnusedMember.Global

namespace ElectionGuard
{
    /// <summary>
    /// `CiphertextElectionContext` is the ElectionGuard representation of a specific election
    /// Note: The ElectionGuard Data Spec deviates from the NIST model in that
    /// this object includes fields that are populated in the course of encrypting an election
    /// Specifically, `crypto_base_hash`, `crypto_extended_base_hash` and `elgamal_public_key`
    /// are populated with election-specific information necessary for encrypting the election.
    /// Refer to the [ElectionGuard Specification](https://github.com/microsoft/electionguard) for more information.
    /// </summary>
    public class CiphertextElectionContext : DisposableBase, IEquatable<CiphertextElectionContext>
    {
        /// <summary>
        /// Get a linked list containing the extended data of the election.
        /// </summary>
        public ContextConfiguration Configuration
        {
            get
            {
                var status = NativeInterface.CiphertextElectionContext.GetConfiguration(
                    Handle, out var value);
                status.ThrowIfError();
                return value.IsInvalid ? null : new ContextConfiguration(value);
            }
        }

        /// <summary>
        /// The number of guardians necessary to generate the public key
        /// </summary>
        public ulong NumberOfGuardians
        {
            get
            {
                ulong value = 0;
                var status = NativeInterface.CiphertextElectionContext.GetNumberOfGuardians(
                    Handle, ref value);
                status.ThrowIfError();
                return value;
            }
        }

        /// <summary>
        /// The quorum of guardians necessary to decrypt an election.  Must be less than `number_of_guardians`
        /// </summary>
        public ulong Quorum
        {
            get
            {
                ulong value = 0;
                var status = NativeInterface.CiphertextElectionContext.GetQuorum(
                    Handle, ref value);
                status.ThrowIfError();
                return value;
            }
        }

        /// <summary>
        /// the `joint public key (K)` in the [ElectionGuard Spec](https://github.com/microsoft/electionguard/wiki)
        /// </summary>
        public ElementModP ElGamalPublicKey
        {
            get
            {
                var status = NativeInterface.CiphertextElectionContext.GetElGamalPublicKey(
                    Handle, out var value);
                status.ThrowIfError();
                return value.IsInvalid ? null : new ElementModP(value);
            }
        }

        /// <summary>
        /// the `commitment hash H(K 1,0 , K 2,0 ... , K n,0 )` of the public commitments
        /// guardians make to each other in the [ElectionGuard Spec](https://github.com/microsoft/electionguard/wiki)
        /// </summary>
        public ElementModQ CommitmentHash
        {
            get
            {
                var status = NativeInterface.CiphertextElectionContext.GetCommitmentHash(
                    Handle, out var value);
                status.ThrowIfError();
                return value.IsInvalid ? null : new ElementModQ(value);
            }
        }

        /// <summary>
        /// The hash of the election manifest
        /// </summary>
        public ElementModQ ManifestHash
        {
            get
            {
                var status = NativeInterface.CiphertextElectionContext.GetManifestHash(
                    Handle, out var value);
                status.ThrowIfError();
                return value.IsInvalid ? null : new ElementModQ(value);
            }
        }

        /// <summary>
        /// the `base hash code (𝑄)` in the [ElectionGuard Spec](https://github.com/microsoft/electionguard/wiki)
        /// </summary>
        public ElementModQ CryptoBaseHash
        {
            get
            {
                var status = NativeInterface.CiphertextElectionContext.GetCryptoBaseHash(
                    Handle, out var value);
                status.ThrowIfError();
                return value.IsInvalid ? null : new ElementModQ(value);
            }
        }

        /// <summary>
        /// the `extended base hash code (𝑄')` in the [ElectionGuard Spec](https://github.com/microsoft/electionguard/wiki)
        /// </summary>
        public ElementModQ CryptoExtendedBaseHash
        {
            get
            {
                var status = NativeInterface.CiphertextElectionContext.GetCryptoExtendedBaseHash(
                    Handle, out var value);
                status.ThrowIfError();
                return value.IsInvalid ? null : new ElementModQ(value);
            }
        }

        /// <summary>
        /// Get a linked list containing the extended data of the election.
        /// </summary>
        public LinkedList ExtendedData
        {
            get
            {
                var status = NativeInterface.CiphertextElectionContext.GetExtendedData(
                    Handle, out var value);
                status.ThrowIfError();
                return value.IsInvalid ? null : new LinkedList(value);
            }
        }


        internal NativeInterface.CiphertextElectionContext.CiphertextElectionContextHandle Handle;

        /// <summary>
        /// Creates an <see cref="CiphertextElectionContext">CiphertextElectionContext</see> object from a <see href="https://www.rfc-editor.org/rfc/rfc8259.html#section-8.1">[RFC-8259]</see> UTF-8 encoded JSON string
        /// </summary>
        /// <param name="json">A UTF-8 Encoded JSON data string</param>
        public CiphertextElectionContext(string json)
        {
            var status = NativeInterface.CiphertextElectionContext.FromJson(json, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        ///  Makes a CiphertextElectionContext object.
        ///
        /// <param name="numberOfGuardians"> The number of guardians necessary to generate the public key </param>
        /// <param name="quorum"> The quorum of guardians necessary to decrypt an election.  Must be less than `number_of_guardians` </param>
        /// <param name="publicKey"> the public key of the election </param>
        /// <param name="commitmentHash"> the hash of the commitments the guardians make to each other </param>
        /// <param name="manifestHash"> the hash of the election metadata </param>
        /// </summary>
        public CiphertextElectionContext(
            ulong numberOfGuardians,
            ulong quorum,
            ElementModP publicKey,
            ElementModQ commitmentHash,
            ElementModQ manifestHash)
        {
            var status = NativeInterface.CiphertextElectionContext.Make(
                numberOfGuardians, quorum, publicKey.Handle,
                commitmentHash.Handle, manifestHash.Handle, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        ///  Makes a CiphertextElectionContext object.
        ///
        /// <param name="numberOfGuardians"> The number of guardians necessary to generate the public key </param>
        /// <param name="quorum"> The quorum of guardians necessary to decrypt an election.  Must be less than `number_of_guardians` </param>
        /// <param name="jointKey"> the hash of the commitments the guardians make to each other </param>
        /// <param name="manifestHash"> the hash of the election metadata </param>
        /// </summary>
        public CiphertextElectionContext(
            ulong numberOfGuardians,
            ulong quorum,
            ElectionJointKey jointKey,
            ElementModQ manifestHash)
        {
            var status = NativeInterface.CiphertextElectionContext.Make(
                numberOfGuardians, quorum, jointKey.JointPublicKey.Handle,
                jointKey.CommitmentHash.Handle, manifestHash.Handle, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        ///  Makes a CiphertextElectionContext object.
        ///
        /// <param name="numberOfGuardians"> The number of guardians necessary to generate the public key </param>
        /// <param name="quorum"> The quorum of guardians necessary to decrypt an election.  Must be less than `number_of_guardians` </param>
        /// <param name="publicKey"> the public key of the election </param>
        /// <param name="commitmentHash"> the hash of the commitments the guardians make to each other </param>
        /// <param name="manifestHash"> the hash of the election metadata </param>
        /// <param name="config"> the context configuration</param>
        /// </summary>
        public CiphertextElectionContext(
            ulong numberOfGuardians,
            ulong quorum,
            ElementModP publicKey,
            ElementModQ commitmentHash,
            ElementModQ manifestHash,
            ContextConfiguration config)
        {
            var status = NativeInterface.CiphertextElectionContext.Make(
                numberOfGuardians, quorum, publicKey.Handle,
                commitmentHash.Handle, manifestHash.Handle, config.Handle, out Handle);
            status.ThrowIfError();
        }


        /// <summary>
        ///  Makes a CiphertextElectionContext object.
        ///
        /// <param name="numberOfGuardians"> The number of guardians necessary to generate the public key </param>
        /// <param name="quorum"> The quorum of guardians necessary to decrypt an election.  Must be less than `number_of_guardians` </param>
        /// <param name="publicKey"> the public key of the election </param>
        /// <param name="commitmentHash"> the hash of the commitments the guardians make to each other </param>
        /// <param name="manifestHash"> the hash of the election metadata </param>
        /// <param name="extendedData"> an unordered map of key value strings relevant to the consuming application </param>
        /// </summary>
        public CiphertextElectionContext(ulong numberOfGuardians,
            ulong quorum,
            ElementModP publicKey,
            ElementModQ commitmentHash,
            ElementModQ manifestHash,
            LinkedList extendedData)
        {
            var status = NativeInterface.CiphertextElectionContext.Make(
                numberOfGuardians, quorum, publicKey.Handle,
                commitmentHash.Handle, manifestHash.Handle, extendedData.Handle, out Handle);
            status.ThrowIfError();
        }


        /// <summary>
        ///  Makes a CiphertextElectionContext object.
        ///
        /// <param name="numberOfGuardians"> The number of guardians necessary to generate the public key </param>
        /// <param name="quorum"> The quorum of guardians necessary to decrypt an election.  Must be less than `number_of_guardians` </param>
        /// <param name="publicKey"> the public key of the election </param>
        /// <param name="commitmentHash"> the hash of the commitments the guardians make to each other </param>
        /// <param name="manifestHash"> the hash of the election metadata </param>
        /// <param name="config"> the context configuration</param>
        /// <param name="extendedData"> an unordered map of key value strings relevant to the consuming application </param>
        /// </summary>
        public CiphertextElectionContext(ulong numberOfGuardians,
            ulong quorum,
            ElementModP publicKey,
            ElementModQ commitmentHash,
            ElementModQ manifestHash,
            ContextConfiguration config,
            LinkedList extendedData)
        {
            var status = NativeInterface.CiphertextElectionContext.Make(
                numberOfGuardians, quorum, publicKey.Handle,
                commitmentHash.Handle, manifestHash.Handle, config.Handle, extendedData.Handle, out Handle);
            status.ThrowIfError();
        }

        public CiphertextElectionContext(CiphertextElectionContext other) : this(other.ToJson())
        {
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

        /// <Summary>
        /// Export the representation as JSON
        /// </Summary>
        public string ToJson()
        {
            var status = NativeInterface.CiphertextElectionContext.ToJson(
                Handle, out var pointer, out _);
            status.ThrowIfError();
            var json = pointer.PtrToStringUTF8();
            _ = NativeInterface.Memory.FreeIntPtr(pointer);
            return json;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            _ = sb.AppendLine($"Guardians              : {NumberOfGuardians} ({Quorum})");
            _ = sb.AppendLine($"CommitmentHash         : {CommitmentHash}");
            _ = sb.AppendLine($"ManifestHash           : {ManifestHash}");
            _ = sb.AppendLine($"CryptoExtendedBaseHash : {CryptoExtendedBaseHash}");
            _ = sb.AppendLine($"ElGamalPublicKey       : {ElGamalPublicKey}");

            return sb.ToString();
        }

        #region IEquatable

        public bool Equals(CiphertextElectionContext other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return NumberOfGuardians == other.NumberOfGuardians &&
                   Quorum == other.Quorum &&
                   ElGamalPublicKey.Equals(other.ElGamalPublicKey) &&
                   CommitmentHash.Equals(other.CommitmentHash) &&
                   ManifestHash.Equals(other.ManifestHash) &&
                   CryptoBaseHash.Equals(other.CryptoBaseHash) &&
                   CryptoExtendedBaseHash.Equals(other.CryptoExtendedBaseHash);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || (obj is CiphertextElectionContext other && Equals(other));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                NumberOfGuardians, Quorum, ElGamalPublicKey,
                CommitmentHash, ManifestHash, CryptoBaseHash,
                CryptoExtendedBaseHash);
        }

        #endregion
    }
}
