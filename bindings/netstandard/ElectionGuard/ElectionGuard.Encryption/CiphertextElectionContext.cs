using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
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
    public class CiphertextElectionContext : DisposableBase
    {
        /// <summary>
        /// the `joint public key (K)` in the [ElectionGuard Spec](https://github.com/microsoft/electionguard/wiki)
        /// </summary>
        public ElementModP ElGamalPublicKey
        {
            get
            {
                var status = NativeInterface.CiphertextElectionContext.GetElGamalPublicKey(
                    Handle, out NativeInterface.ElementModP.ElementModPHandle value);
                status.ThrowIfError();
                return new ElementModP(value);
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
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
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
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
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
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
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
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
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
                    Handle, out NativeInterface.LinkedList.LinkedListHandle value);
                status.ThrowIfError();
                return new LinkedList(value);
            }
        }

        /// <summary>
        /// Get a linked list containing the extended data of the election.
        /// </summary>
        public ContextConfiguration Configuration
        {
            get
            {
                var status = NativeInterface.CiphertextElectionContext.GetConfiguration(
                    Handle, out NativeInterface.ContextConfiguration.ContextConfigurationHandle value);
                status.ThrowIfError();
                return new ContextConfiguration(value);
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
        public CiphertextElectionContext(ulong numberOfGuardians,
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
        /// <param name="publicKey"> the public key of the election </param>
        /// <param name="commitmentHash"> the hash of the commitments the guardians make to each other </param>
        /// <param name="manifestHash"> the hash of the election metadata </param>
        /// <param name="config"> the context configuration</param>
        /// </summary>
        public CiphertextElectionContext(ulong numberOfGuardians,
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
                Handle, out IntPtr pointer, out _);
            status.ThrowIfError();
            var json = pointer.PtrToStringUTF8();
            NativeInterface.Memory.FreeIntPtr(pointer);
            return json;
        }
    }
}
