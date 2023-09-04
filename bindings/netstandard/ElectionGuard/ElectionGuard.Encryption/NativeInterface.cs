using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace ElectionGuard
{
    internal static unsafe class NativeInterface
    {
        internal const string DllName = "electionguard";

        internal struct CharPtr { };

        #region Collections

        internal static class LinkedList
        {
            internal struct LinkedListType { };

            internal class LinkedListHandle
                : ElectionGuardSafeHandle<LinkedListType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = LinkedList.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"LinkedList Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_linked_list_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(out LinkedListHandle handle);

            [DllImport(DllName, EntryPoint = "eg_linked_list_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(LinkedListType* handle);

            [DllImport(DllName, EntryPoint = "eg_linked_list_append",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Append(
                LinkedListHandle handle,
                [MarshalAs(UnmanagedType.LPStr)] string key,
                [MarshalAs(UnmanagedType.LPStr)] string value);

            [DllImport(DllName, EntryPoint = "eg_linked_list_delete_last",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status DeleteLast(LinkedListHandle handle);

            [DllImport(DllName, EntryPoint = "eg_linked_list_get_count",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetCount(
                LinkedListHandle handle);

            [DllImport(DllName, EntryPoint = "eg_linked_list_get_element_at",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetElementAt(
                LinkedListHandle handle,
                ulong position,
                out IntPtr key,
                out IntPtr value);

            [DllImport(DllName, EntryPoint = "eg_linked_list_get_value_at",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetValueAt(
                LinkedListHandle handle,
                ulong position,
                out IntPtr value);
        }

        #endregion

        #region Group

        internal static class ElementModP
        {
            internal struct ElementModPType { };

            internal class ElementModPHandle
                : ElectionGuardSafeHandle<ElementModPType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = ElementModP.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"ElementModP Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                ulong* in_data, out ElementModPHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_new_unchecked",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status NewUnchecked(
                ulong* in_data, out ElementModPHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_new_bytes",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status NewBytes(
                byte* in_data, ulong length, out ElementModPHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_from_uint64",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromUint64(
                UInt64 uint64,
                out ElementModPHandle handle);


            [DllImport(DllName, EntryPoint = "eg_element_mod_p_from_uint64_unchecked",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromUint64Unchecked(
                UInt64 uint64,
                out ElementModPHandle handle);


            [DllImport(DllName, EntryPoint = "eg_element_mod_p_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(ElementModPType* handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_get_data",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetData(
                ElementModPHandle handle, ulong** out_data, out ulong out_size);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_to_hex",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToHex(
                ElementModPHandle handle, out IntPtr data);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_from_hex_checked",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromHexChecked(
                [MarshalAs(UnmanagedType.LPStr)] string hex,
                out ElementModPHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_from_hex_unchecked",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromHexUnchecked(
                [MarshalAs(UnmanagedType.LPStr)] string hex,
                out ElementModPHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_to_bytes",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToBytes(
                ElementModPHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_is_valid_residue",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status IsValidResidue(ElementModPHandle data, out bool isValid);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_is_in_bounds",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status IsInBounds(ElementModPHandle data, out bool inBounds);
        }

        internal static class ElementModQ
        {
            internal struct ElementModQType { };

            internal class ElementModQHandle
                : ElectionGuardSafeHandle<ElementModQType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = ElementModQ.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"ElementModQ Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                ulong* in_data, out ElementModQHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_new_unchecked",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status NewUnchecked(
                ulong* in_data, out ElementModQHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_new_bytes",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status NewBytes(
                byte* in_data, ulong length, out ElementModQHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(ElementModQType* handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_get_data",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetData(
                ElementModQHandle handle, ulong** out_data, out ulong out_size);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_to_bytes",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToBytes(
                ElementModQHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_to_hex",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToHex(
                ElementModQHandle handle, out IntPtr out_hex);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_from_element_mod_q",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToElementModP(
                ElementModQHandle handle, out ElementModP.ElementModPHandle out_handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_from_hex_checked",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromHexChecked(
                [MarshalAs(UnmanagedType.LPStr)] string hex,
                out ElementModQHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_from_hex_unchecked",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromHexUnchecked(
                [MarshalAs(UnmanagedType.LPStr)] string hex,
                out ElementModQHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_from_uint64",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromUint64(
                UInt64 uint64,
                out ElementModQHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_from_uint64_unchecked",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromUint64Unchecked(
                UInt64 uint64,
                out ElementModQHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_rand_q_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Random(out ElementModQHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_is_in_bounds",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status IsInBounds(ElementModQHandle data, out bool inBounds);


        }

        internal static class Constants
        {
            [DllImport(DllName, EntryPoint = "eg_element_mod_p_constant_g",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status G(out ElementModP.ElementModPHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_constant_p",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status P(out ElementModP.ElementModPHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_constant_r",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status R(out ElementModP.ElementModPHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_constant_zero_mod_p",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ZERO_MOD_P(out ElementModP.ElementModPHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_constant_one_mod_p",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ONE_MOD_P(out ElementModP.ElementModPHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_p_constant_two_mod_p",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status TWO_MOD_P(out ElementModP.ElementModPHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_constant_q",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Q(out ElementModQ.ElementModQHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_constant_zero_mod_q",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ZERO_MOD_Q(out ElementModQ.ElementModQHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_constant_one_mod_q",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ONE_MOD_Q(out ElementModQ.ElementModQHandle handle);

            [DllImport(DllName, EntryPoint = "eg_element_mod_q_constant_two_mod_q",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status TWO_MOD_Q(out ElementModQ.ElementModQHandle handle);


            [DllImport(DllName, EntryPoint = "eg_constant_to_json",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToJson(out IntPtr data, out ulong size);
        }

        #endregion

        #region Discrete Log

        internal static class DiscreteLog
        {
            [DllImport(DllName, EntryPoint = "eg_discrete_log_get_async_base_g",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetAsync(
                ElementModP.ElementModPHandle in_element,
                ref ulong out_result);

            [DllImport(DllName, EntryPoint = "eg_discrete_log_get_async",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetAsync(
                ElementModP.ElementModPHandle in_element,
                ElementModP.ElementModPHandle in_encryption_base,
                ref ulong out_result);
        }
        #endregion

        #region Elgamal

        internal static class ElGamalKeyPair
        {
            internal struct ElGamalKeyPairType { };

            internal class ElGamalKeyPairHandle
                : ElectionGuardSafeHandle<ElGamalKeyPairType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = ElGamalKeyPair.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"ElGamalKeyPair Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_elgamal_keypair_from_secret_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                ElementModQ.ElementModQHandle in_secret_key,
                out ElGamalKeyPairHandle handle);

            [DllImport(DllName, EntryPoint = "eg_elgamal_keypair_from_pair_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                ElementModQ.ElementModQHandle in_secret_key,
                ElementModP.ElementModPHandle in_public_key,
                out ElGamalKeyPairHandle handle);

            [DllImport(DllName, EntryPoint = "eg_elgamal_keypair_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(ElGamalKeyPairType* handle);

            [DllImport(DllName, EntryPoint = "eg_elgamal_keypair_get_public_key",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetPublicKey(
                ElGamalKeyPairHandle handle,
                out ElementModP.ElementModPHandle out_public_key);

            [DllImport(DllName, EntryPoint = "eg_elgamal_keypair_get_secret_key",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetSecretKey(
                ElGamalKeyPairHandle handle,
                out ElementModQ.ElementModQHandle out_secret_key);
        }

        internal static class ElGamalCiphertext
        {
            internal struct ElGamalCiphertextType { };

            internal class ElGamalCiphertextHandle
                : ElectionGuardSafeHandle<ElGamalCiphertextType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = ElGamalCiphertext.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"ElGamalCiphertext Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_elgamal_ciphertext_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                ElementModP.ElementModPHandle in_pad,
                ElementModP.ElementModPHandle in_data,
                out ElGamalCiphertextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_elgamal_ciphertext_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(ElGamalCiphertextType* handle);

            [DllImport(DllName, EntryPoint = "eg_elgamal_ciphertext_get_pad",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetPad(
                ElGamalCiphertextHandle handle,
                out ElementModP.ElementModPHandle elgamal_public_key);

            [DllImport(DllName, EntryPoint = "eg_elgamal_ciphertext_get_data",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetData(
                ElGamalCiphertextHandle handle,
                out ElementModP.ElementModPHandle elgamal_public_key);

            [DllImport(DllName, EntryPoint = "eg_elgamal_ciphertext_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetCryptoHash(
                ElGamalCiphertextHandle handle,
                out ElementModQ.ElementModQHandle crypto_base_hash);

            [DllImport(DllName, EntryPoint = "eg_elgamal_ciphertext_add_collection",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            // ReSharper disable once MemberHidesStaticFromOuterClass
            internal static extern Status Add(
                ElGamalCiphertextHandle self,
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] ciphertexts,
                ulong ciphertextsSize,
                out ElGamalCiphertextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_elgamal_ciphertext_add",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            // ReSharper disable once MemberHidesStaticFromOuterClass
            internal static extern Status Add(
                ElGamalCiphertextHandle self,
                ElGamalCiphertextHandle other,
                out ElGamalCiphertextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_elgamal_ciphertext_decrypt_accumulation",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status DecryptAccumulation(
                ElGamalCiphertextHandle handle,
                ElementModP.ElementModPHandle shareAccumulation,
                ElementModP.ElementModPHandle encryption_base,
                ref ulong plaintext);

            [DllImport(DllName, EntryPoint = "eg_elgamal_ciphertext_decrypt_with_secret",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status DecryptWithSecret(
                ElGamalCiphertextHandle handle,
                ElementModQ.ElementModQHandle secret_key,
                ElementModP.ElementModPHandle encryption_base,
                ref ulong plaintext);

            [DllImport(DllName, EntryPoint = "eg_elgamal_ciphertext_decrypt_known_nonce",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status DecryptKnownNonce(
                ElGamalCiphertextHandle handle,
                ElementModP.ElementModPHandle public_key,
                ElementModQ.ElementModQHandle nonce,
                ref ulong plaintext);

            [DllImport(DllName, EntryPoint = "eg_elgamal_ciphertext_partial_decrypt",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status PartialDecrypt(
                ElGamalCiphertextHandle handle,
                ElementModQ.ElementModQHandle secret_key,
                out ElementModP.ElementModPHandle partial_decryption);

        }

        internal static class ElGamal
        {
            [DllImport(DllName, EntryPoint = "eg_elgamal_add_collection",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            // ReSharper disable once MemberHidesStaticFromOuterClass
            internal static extern Status Add(
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] ciphertexts,
                ulong ciphertextsSize,
                out ElGamalCiphertext.ElGamalCiphertextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_elgamal_add",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            // ReSharper disable once MemberHidesStaticFromOuterClass
            internal static extern Status Add(
                ElGamalCiphertext.ElGamalCiphertextHandle a,
                ElGamalCiphertext.ElGamalCiphertextHandle b,
                out ElGamalCiphertext.ElGamalCiphertextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_elgamal_encrypt",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            // ReSharper disable once MemberHidesStaticFromOuterClass
            internal static extern Status Encrypt(
                ulong plaintext,
                ElementModQ.ElementModQHandle nonce,
                ElementModP.ElementModPHandle public_key,
                out ElGamalCiphertext.ElGamalCiphertextHandle handle);
        }


        internal static class HashedElGamalCiphertext
        {
            internal struct HashedElGamalCiphertextType { };

            internal class HashedElGamalCiphertextHandle
                : ElectionGuardSafeHandle<HashedElGamalCiphertextType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = HashedElGamalCiphertext.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"HashedElGamalCiphertext Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_hashed_elgamal_ciphertext_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(ElementModP.ElementModPHandle in_pad,
                byte* in_data, ulong in_data_length,
                byte* in_mac, ulong in_mac_length,
                out HashedElGamalCiphertextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_hashed_elgamal_ciphertext_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(HashedElGamalCiphertextType* handle);

            [DllImport(DllName, EntryPoint = "eg_hashed_elgamal_ciphertext_get_pad",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetPad(
                HashedElGamalCiphertextHandle handle,
                out ElementModP.ElementModPHandle elgamal_public_key);

            [DllImport(DllName, EntryPoint = "eg_hashed_elgamal_ciphertext_get_data",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetData(
                HashedElGamalCiphertextHandle handle,
                out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_hashed_elgamal_ciphertext_get_mac",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetMac(
                HashedElGamalCiphertextHandle handle,
                out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_hashed_elgamal_ciphertext_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetCryptoHash(
                HashedElGamalCiphertextHandle handle,
                out ElementModQ.ElementModQHandle crypto_base_hash);

            [DllImport(DllName, EntryPoint = "eg_hashed_elgamal_ciphertext_decrypt_with_secret",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Decrypt(
                HashedElGamalCiphertextHandle handle,
                ElementModP.ElementModPHandle publicKey,
                ElementModQ.ElementModQHandle secretKey,
                [MarshalAs(UnmanagedType.LPStr)] string hashPrefix,
                ElementModQ.ElementModQHandle encryptionSeed,
                bool lookForPadding,
                out IntPtr data,
                out ulong size);

            [DllImport(DllName, EntryPoint = "eg_hashed_elgamal_ciphertext_partial_decrypt",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status PartialDecrypt(
                HashedElGamalCiphertextHandle handle,
                ElementModQ.ElementModQHandle secret_key,
                out ElementModP.ElementModPHandle partial_decryption);
        }

        internal static class HashedElGamal
        {
            [DllImport(DllName, EntryPoint = "eg_hashed_elgamal_encrypt",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            // ReSharper disable once MemberHidesStaticFromOuterClass
            internal static extern Status Encrypt(
                byte* message, ulong length,
                ElementModQ.ElementModQHandle nonce,
                [MarshalAs(UnmanagedType.LPStr)] string hashPrefix,
                ElementModP.ElementModPHandle publicKey,
                ElementModQ.ElementModQHandle seed,
                uint maxLength,
                bool allowTruncation,
                bool usePrecompute,
                out HashedElGamalCiphertext.HashedElGamalCiphertextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_hashed_elgamal_encrypt_no_pdding",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            // ReSharper disable once MemberHidesStaticFromOuterClass
            internal static extern Status Encrypt(
                byte* message, ulong length,
                ElementModQ.ElementModQHandle nonce,
                [MarshalAs(UnmanagedType.LPStr)] string hashPrefix,
                ElementModP.ElementModPHandle publicKey,
                ElementModQ.ElementModQHandle seed,
                bool usePrecompute,
                out HashedElGamalCiphertext.HashedElGamalCiphertextHandle handle);
        }

        #endregion

        #region ChaumPedersen

        internal static class DisjunctiveChaumPedersenProof
        {
            internal struct DisjunctiveChaumPedersenProofType { };

            internal class DisjunctiveChaumPedersenProofHandle
                : ElectionGuardSafeHandle<DisjunctiveChaumPedersenProofType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = DisjunctiveChaumPedersenProof.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"DisjunctiveChaumPedersenProof Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_disjunctive_chaum_pedersen_proof_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(DisjunctiveChaumPedersenProofType* handle);

            [DllImport(DllName, EntryPoint = "eg_disjunctive_chaum_pedersen_proof_get_zero_pad",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetZeroPad(
                DisjunctiveChaumPedersenProofHandle handle,
                out ElementModP.ElementModPHandle element);

            [DllImport(DllName, EntryPoint = "eg_disjunctive_chaum_pedersen_proof_get_zero_data",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetZeroData(
                DisjunctiveChaumPedersenProofHandle handle,
                out ElementModP.ElementModPHandle element);

            [DllImport(DllName, EntryPoint = "eg_disjunctive_chaum_pedersen_proof_get_one_pad",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetOnePad(
                DisjunctiveChaumPedersenProofHandle handle,
                out ElementModP.ElementModPHandle element);

            [DllImport(DllName, EntryPoint = "eg_disjunctive_chaum_pedersen_proof_get_one_data",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetOneData(
                DisjunctiveChaumPedersenProofHandle handle,
                out ElementModP.ElementModPHandle element);

            [DllImport(DllName, EntryPoint = "eg_disjunctive_chaum_pedersen_proof_get_zero_challenge",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetZeroChallenge(
                DisjunctiveChaumPedersenProofHandle handle,
                out ElementModQ.ElementModQHandle element);

            [DllImport(DllName, EntryPoint = "eg_disjunctive_chaum_pedersen_proof_get_one_challenge",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetOneChallenge(
                DisjunctiveChaumPedersenProofHandle handle,
                out ElementModQ.ElementModQHandle element);

            [DllImport(DllName, EntryPoint = "eg_disjunctive_chaum_pedersen_proof_get_challenge",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetChallenge(
                DisjunctiveChaumPedersenProofHandle handle,
                out ElementModQ.ElementModQHandle element);

            [DllImport(DllName, EntryPoint = "eg_disjunctive_chaum_pedersen_proof_get_zero_response",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetZeroResponse(
                DisjunctiveChaumPedersenProofHandle handle,
                out ElementModQ.ElementModQHandle element);

            [DllImport(DllName, EntryPoint = "eg_disjunctive_chaum_pedersen_proof_get_one_response",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetOneResponse(
                DisjunctiveChaumPedersenProofHandle handle,
                out ElementModQ.ElementModQHandle element);

            [DllImport(DllName, EntryPoint = "eg_disjunctive_chaum_pedersen_proof_make",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Make(
                ElGamalCiphertext.ElGamalCiphertextHandle message,
                ElementModQ.ElementModQHandle r,
                ElementModP.ElementModPHandle k,
                ElementModQ.ElementModQHandle q,
                ulong plaintext,
                out DisjunctiveChaumPedersenProofHandle handle);

            [DllImport(DllName, EntryPoint = "eg_disjunctive_chaum_pedersen_proof_make_deterministic",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Make(
                ElGamalCiphertext.ElGamalCiphertextHandle message,
                ElementModQ.ElementModQHandle r,
                ElementModP.ElementModPHandle k,
                ElementModQ.ElementModQHandle q,
                ElementModQ.ElementModQHandle seed,
                ulong plaintext,
                out DisjunctiveChaumPedersenProofHandle handle);

            [DllImport(DllName, EntryPoint = "eg_disjunctive_chaum_pedersen_proof_is_valid",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern bool IsValid(
                DisjunctiveChaumPedersenProofHandle handle,
                ElGamalCiphertext.ElGamalCiphertextHandle message,
                ElementModP.ElementModPHandle k,
                ElementModQ.ElementModQHandle q);

        }

        internal static class RangedChaumPedersenProof
        {
            internal struct RangedChaumPedersenProofType { };

            internal class RangedChaumPedersenProofHandle
    : ElectionGuardSafeHandle<RangedChaumPedersenProofType>
            {
                protected override bool Free()
                {
                    if (IsClosed)
                    {
                        return true;
                    }

                    var status = RangedChaumPedersenProof.Free(TypedPtr);
                    return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                        ? throw new ElectionGuardException($"DisjunctiveChaumPedersenProof Error Free: {status}", status)
                        : true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_ranged_chaum_pedersen_proof_make_deterministic",
               CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Make(
               ElGamalCiphertext.ElGamalCiphertextHandle message,
               ElementModQ.ElementModQHandle r,
               ulong selected,
               ulong maxLimit,
               ElementModP.ElementModPHandle k,
               ElementModQ.ElementModQHandle q,
               [MarshalAs(UnmanagedType.LPStr)] string hashPrefix,
               ElementModQ.ElementModQHandle seed,
               out RangedChaumPedersenProofHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ranged_chaum_pedersen_proof_make",
               CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Make(
               ElGamalCiphertext.ElGamalCiphertextHandle message,
               ElementModQ.ElementModQHandle r,
               ulong selected,
               ulong maxLimit,
               ElementModP.ElementModPHandle k,
               ElementModQ.ElementModQHandle q,
               [MarshalAs(UnmanagedType.LPStr)] string hashPrefix,
               out RangedChaumPedersenProofHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ranged_chaum_pedersen_proof_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(RangedChaumPedersenProofType* handle);

            [DllImport(DllName, EntryPoint = "eg_ranged_chaum_pedersen_proof_get_range_limit",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetRangeLimit(RangedChaumPedersenProofHandle handle, out ulong elementRef);

            [DllImport(DllName, EntryPoint = "eg_ranged_chaum_pedersen_proof_get_challenge",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetChallenge(RangedChaumPedersenProofHandle handle, out ElementModQ.ElementModQHandle elementRef);

            [DllImport(DllName, EntryPoint = "eg_ranged_chaum_pedersen_proof_is_valid",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status IsValid(
                RangedChaumPedersenProofHandle handle,
                ElGamalCiphertext.ElGamalCiphertextHandle ciphertext,
                ElementModP.ElementModPHandle k,
                ElementModQ.ElementModQHandle q,
                [MarshalAs(UnmanagedType.LPStr)] string hashPrefix,
                out bool isValid
                );

        }

        internal static class ConstantChaumPedersenProof
        {
            internal struct ConstantChaumPedersenProofType { };

            internal class ConstantChaumPedersenProofHandle
                : ElectionGuardSafeHandle<ConstantChaumPedersenProofType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = ConstantChaumPedersenProof.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"ConstantChaumPedersenProof Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_constant_chaum_pedersen_proof_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(ConstantChaumPedersenProofType* handle);

            [DllImport(DllName, EntryPoint = "eg_constant_chaum_pedersen_proof_get_pad",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetPad(
                ConstantChaumPedersenProofHandle handle,
                out ElementModP.ElementModPHandle element);

            [DllImport(DllName, EntryPoint = "eg_constant_chaum_pedersen_proof_get_data",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetData(
                ConstantChaumPedersenProofHandle handle,
                out ElementModP.ElementModPHandle element);

            [DllImport(DllName, EntryPoint = "eg_constant_chaum_pedersen_proof_get_challenge",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetChallenge(
                ConstantChaumPedersenProofHandle handle,
                out ElementModQ.ElementModQHandle element);

            [DllImport(DllName, EntryPoint = "eg_constant_chaum_pedersen_proof_get_response",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetResponse(
                ConstantChaumPedersenProofHandle handle,
                out ElementModQ.ElementModQHandle element);

            [DllImport(DllName, EntryPoint = "eg_constant_chaum_pedersen_proof_make",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Make(
                ElGamalCiphertext.ElGamalCiphertextHandle message,
                ElementModQ.ElementModQHandle r,
                ElementModP.ElementModPHandle k,
                ElementModQ.ElementModQHandle seed,
                ElementModQ.ElementModQHandle hash_header,
                ulong constant,
                bool shouldUsePrecomputedValues,
                out ConstantChaumPedersenProofHandle handle);

            [DllImport(DllName, EntryPoint = "eg_constant_chaum_pedersen_proof_is_valid",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern bool IsValid(
                ConstantChaumPedersenProofHandle handle,
                ElGamalCiphertext.ElGamalCiphertextHandle message,
                ElementModP.ElementModPHandle k,
                ElementModQ.ElementModQHandle q);

        }


        #endregion

        #region AnnotatedString

        internal static class AnnotatedString
        {
            internal struct AnnotatedStringType { };

            internal class AnnotatedStringHandle
                : ElectionGuardSafeHandle<AnnotatedStringType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = AnnotatedString.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"AnnotatedString Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_annotated_string_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string annotation,
                [MarshalAs(UnmanagedType.LPStr)] string value,
                out AnnotatedStringHandle handle);

            [DllImport(DllName, EntryPoint = "eg_annotated_string_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(AnnotatedStringType* handle);

            [DllImport(DllName, EntryPoint = "eg_annotated_string_get_annotation",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetAnnotation(
                AnnotatedStringHandle handle, out IntPtr language);


            [DllImport(DllName, EntryPoint = "eg_annotated_string_get_value",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetValue(
                AnnotatedStringHandle handle, out IntPtr value);

            [DllImport(DllName, EntryPoint = "eg_annotated_string_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CryptoHash(
                AnnotatedStringHandle handle,
                out ElementModQ.ElementModQHandle crypto_hash);
        }

        #endregion

        #region Language

        internal static class Language
        {
            internal struct LanguageType { };

            internal class LanguageHandle
                : ElectionGuardSafeHandle<LanguageType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = Language.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"Language Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_language_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string value,
                [MarshalAs(UnmanagedType.LPStr)] string language,
                out LanguageHandle handle);

            [DllImport(DllName, EntryPoint = "eg_language_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(LanguageType* handle);

            [DllImport(DllName, EntryPoint = "eg_language_get_value",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetValue(
                LanguageHandle handle, out IntPtr value);

            [DllImport(DllName, EntryPoint = "eg_language_get_language",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetLanguage(
                LanguageHandle handle, out IntPtr language);

            [DllImport(DllName, EntryPoint = "eg_language_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CryptoHash(
                LanguageHandle handle,
                out ElementModQ.ElementModQHandle crypto_hash);
        }

        #endregion

        #region InternationalizedText

        // TODO: get the calling conventions correct

        internal static class InternationalizedText
        {
            internal struct InternationalizedTextType { };

            internal class InternationalizedTextHandle
                : ElectionGuardSafeHandle<InternationalizedTextType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = InternationalizedText.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"InternationalizedText Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_internationalized_text_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                // TODO: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] text,
                ulong textSize,
                out InternationalizedTextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_internationalized_text_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(InternationalizedTextType* handle);

            [DllImport(DllName, EntryPoint = "eg_internationalized_text_get_text_size",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetTextSize(
                InternationalizedTextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_internationalized_text_get_text_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetTextAtIndex(
                InternationalizedTextHandle handle,
                ulong index,
                out Language.LanguageHandle text);

            [DllImport(DllName, EntryPoint = "eg_intertnationalized_text_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CryptoHash(
                InternationalizedTextHandle handle,
                out ElementModQ.ElementModQHandle crypto_hash);
        }

        #endregion

        #region ContactInformation

        internal static class ContactInformation
        {
            internal struct ContactInformationType { };

            internal class ContactInformationHandle
                : ElectionGuardSafeHandle<ContactInformationType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = ContactInformation.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"ContactInformation Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_contact_information_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string name,
                out ContactInformationHandle handle);

            // TODO: add eg_contact_information_new_with_collections

            [DllImport(DllName, EntryPoint = "eg_contact_information_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(ContactInformationType* handle);

            [DllImport(DllName, EntryPoint = "eg_contact_information_get_address_line_size",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetAddressLineSize(
                ContactInformationHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contact_information_get_address_line_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetAddressLineAtIndex(
                ContactInformationHandle handle,
                ulong index,
                out IntPtr address);

            [DllImport(DllName, EntryPoint = "eg_contact_information_get_email_line_size",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetEmailLineSize(
                ContactInformationHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contact_information_get_email_line_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetEmailLineAtIndex(
                ContactInformationHandle handle,
                ulong index,
                out InternationalizedText.InternationalizedTextHandle email);

            [DllImport(DllName, EntryPoint = "eg_contact_information_get_phone_line_size",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetPhoneLineSize(
                ContactInformationHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contact_information_get_email_line_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetPhoneLineAtIndex(
                ContactInformationHandle handle,
                ulong index,
                out InternationalizedText.InternationalizedTextHandle phone);

            [DllImport(DllName, EntryPoint = "eg_contact_information_get_name",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetName(
                ContactInformationHandle handle, out IntPtr value);

            [DllImport(DllName, EntryPoint = "eg_contact_information_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CryptoHash(
                ContactInformationHandle handle,
                out ElementModQ.ElementModQHandle crypto_hash);
        }

        #endregion

        #region GeopoliticalUnit

        internal static class GeopoliticalUnit
        {
            internal struct GeopoliticalUnitType { };

            internal class GeopoliticalUnitHandle
                : ElectionGuardSafeHandle<GeopoliticalUnitType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = GeopoliticalUnit.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"GeopoliticalUnit Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_geopolitical_unit_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                ReportingUnitType reportingUnitType,
                out GeopoliticalUnitHandle handle);

            [DllImport(DllName, EntryPoint = "eg_geopolitical_unit_new_with_contact_info",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                ReportingUnitType reportingUnitType,
                ContactInformation.ContactInformationHandle contactInformation,
                out GeopoliticalUnitHandle handle);

            [DllImport(DllName, EntryPoint = "eg_geopolitical_unit_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(GeopoliticalUnitType* handle);

            [DllImport(DllName, EntryPoint = "eg_geopolitical_unit_get_object_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetObjectId(
                GeopoliticalUnitHandle handle, out IntPtr objectId);

            [DllImport(DllName, EntryPoint = "eg_geopolitical_unit_get_name",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetName(
                GeopoliticalUnitHandle handle, out IntPtr name);

            [DllImport(DllName, EntryPoint = "get_geopolitical_unit_get_type",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ReportingUnitType GetReportingUnitType(
                GeopoliticalUnitHandle handle);

            [DllImport(DllName, EntryPoint = "eg_geopolitical_unit_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CryptoHash(
                GeopoliticalUnitHandle handle,
                out ElementModQ.ElementModQHandle crypto_hash);
        }

        #endregion

        #region BallotStyle

        internal static class BallotStyle
        {
            internal struct BallotStyleType { };

            internal class BallotStyleHandle
                : ElectionGuardSafeHandle<BallotStyleType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = BallotStyle.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"BallotStyle Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_ballot_style_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] gpUnitIds,
                ulong gpUnitIdsSize,
                out BallotStyleHandle handle);

            // TODO eg_ballot_style_new_with_parties

            [DllImport(DllName, EntryPoint = "eg_ballot_style_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(BallotStyleType* handle);

            [DllImport(DllName, EntryPoint = "eg_ballot_style_get_object_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetObjectId(
                BallotStyleHandle handle, out IntPtr objectId);

            [DllImport(DllName, EntryPoint = "eg_ballot_style_get_geopolitical_unit_ids_size",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetGeopoliticalUnitSize(
                BallotStyleHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ballot_style_get_geopolitical_unit_id_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetGeopoliticalInitIdAtIndex(
                BallotStyleHandle handle,
                ulong index,
                out IntPtr gpUnitId);

            [DllImport(DllName, EntryPoint = "eg_ballot_style_get_party_ids_size",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetPartyIdsSize(
                BallotStyleHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ballot_style_get_party_id_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetPartyIdAtIndex(
                BallotStyleHandle handle,
                ulong index,
                out IntPtr partyId);

            [DllImport(DllName, EntryPoint = "eg_ballot_style_get_image_uri",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetImageUri(
                BallotStyleHandle handle, out IntPtr imageUri);

            [DllImport(DllName, EntryPoint = "eg_ballot_style_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CryptoHash(
                BallotStyleHandle handle,
                out ElementModQ.ElementModQHandle crypto_hash);
        }

        #endregion

        #region Party

        internal static class Party
        {
            internal struct PartyType { };

            internal class PartyHandle
                : ElectionGuardSafeHandle<PartyType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = Party.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"Party Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_party_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                out PartyHandle handle);

            [DllImport(DllName, EntryPoint = "eg_party_new_with_extras",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                InternationalizedText.InternationalizedTextHandle name,
                [MarshalAs(UnmanagedType.LPStr)] string abbreviation,
                [MarshalAs(UnmanagedType.LPStr)] string color,
                [MarshalAs(UnmanagedType.LPStr)] string logoUri,
                out PartyHandle handle);

            [DllImport(DllName, EntryPoint = "eg_party_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(PartyType* handle);

            [DllImport(DllName, EntryPoint = "eg_party_get_object_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetObjectId(
                PartyHandle handle, out IntPtr objectId);

            [DllImport(DllName, EntryPoint = "eg_party_get_abbreviation",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetAbbreviation(
                PartyHandle handle, out IntPtr abbreviation);

            [DllImport(DllName, EntryPoint = "eg_party_get_name",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetName(
                PartyHandle handle, out InternationalizedText.InternationalizedTextHandle name);

            [DllImport(DllName, EntryPoint = "eg_party_get_color",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetColor(
                PartyHandle handle, out IntPtr color);

            [DllImport(DllName, EntryPoint = "eg_party_get_logo_uri",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetLogoUri(
                PartyHandle handle, out IntPtr logoUri);

            [DllImport(DllName, EntryPoint = "eg_party_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CryptoHash(
                PartyHandle handle,
                out ElementModQ.ElementModQHandle crypto_hash);
        }

        #endregion

        #region Candidate

        internal static class Candidate
        {
            internal struct CandidateType { };

            internal class CandidateHandle
                : ElectionGuardSafeHandle<CandidateType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = Candidate.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"Candidate Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_candidate_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                bool isWriteIn,
                out CandidateHandle handle);

            [DllImport(DllName, EntryPoint = "eg_candidate_new_with_party",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                [MarshalAs(UnmanagedType.LPStr)] string partyId,
                bool isWriteIn,
                out CandidateHandle handle);

            [DllImport(DllName, EntryPoint = "eg_candidate_new_with_extras",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                InternationalizedText.InternationalizedTextHandle name,
                [MarshalAs(UnmanagedType.LPStr)] string partyId,
                [MarshalAs(UnmanagedType.LPStr)] string imageUri,
                bool isWriteIn,
                out CandidateHandle handle);

            [DllImport(DllName, EntryPoint = "eg_candidate_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(CandidateType* handle);

            [DllImport(DllName, EntryPoint = "eg_candidate_get_object_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetObjectId(
                CandidateHandle handle, out IntPtr objectId);

            [DllImport(DllName, EntryPoint = "eg_candidate_get_candidate_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetCandidateId(
                CandidateHandle handle, out IntPtr candidateId);

            [DllImport(DllName, EntryPoint = "eg_candidate_get_name",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetName(
                CandidateHandle handle, out InternationalizedText.InternationalizedTextHandle name);

            [DllImport(DllName, EntryPoint = "eg_candidate_get_party_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetPartyId(
                CandidateHandle handle, out IntPtr partyId);

            [DllImport(DllName, EntryPoint = "eg_candidate_get_image_uri",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetImageUri(
                CandidateHandle handle, out IntPtr imageUrl);

            [DllImport(DllName, EntryPoint = "eg_candidate_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CryptoHash(
                CandidateHandle handle,
                out ElementModQ.ElementModQHandle crypto_hash);

            [DllImport(DllName, EntryPoint = "eg_candidate_get_is_write_in",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern bool GetIsWriteIn(CandidateHandle handle);
        }

        #endregion

        #region SelectionDescription

        internal static class SelectionDescription
        {
            internal struct SelectionDescriptionType { };

            internal class SelectionDescriptionHandle
                : ElectionGuardSafeHandle<SelectionDescriptionType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = SelectionDescription.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"SelectionDescription Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_selection_description_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                [MarshalAs(UnmanagedType.LPStr)] string candidateId,
                ulong sequenceOrder,
                out SelectionDescriptionHandle handle);

            [DllImport(DllName, EntryPoint = "eg_selection_description_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(SelectionDescriptionType* handle);

            [DllImport(DllName, EntryPoint = "eg_selection_description_get_object_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetObjectId(
                SelectionDescriptionHandle handle, out IntPtr objectId);

            [DllImport(DllName, EntryPoint = "eg_selection_description_get_candidate_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetCandidateId(
                SelectionDescriptionHandle handle, out IntPtr candidateId);

            [DllImport(DllName, EntryPoint = "eg_selection_description_get_sequence_order",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetSequenceOrder(
                SelectionDescriptionHandle handle);

            [DllImport(DllName, EntryPoint = "eg_selection_description_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CryptoHash(
                SelectionDescriptionHandle handle,
                out ElementModQ.ElementModQHandle crypto_hash);
        }

        #endregion

        #region ContestDescription

        internal static class ContestDescription
        {
            internal struct ContestDescriptionType { };

            internal class ContestDescriptionHandle
                : ElectionGuardSafeHandle<ContestDescriptionType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = ContestDescription.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"ContestDescription Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_contest_description_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                [MarshalAs(UnmanagedType.LPStr)] string electoralDistrictId,
                ulong sequenceOrder,
                VoteVariationType voteVariation,
                ulong numberElected,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                // TODO: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] selections,
                ulong selectionsSize,
                out ContestDescriptionHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_new_with_title",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                [MarshalAs(UnmanagedType.LPStr)] string electoralDistrictId,
                ulong sequenceOrder,
                VoteVariationType voteVariation,
                ulong numberElected,
                ulong votesAllowed,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                InternationalizedText.InternationalizedTextHandle ballotTitle,
                InternationalizedText.InternationalizedTextHandle ballotSubTitle,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] selections,
                ulong selectionsSize,
                out ContestDescriptionHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_new_with_parties",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                [MarshalAs(UnmanagedType.LPStr)] string electoralDistrictId,
                ulong sequenceOrder,
                VoteVariationType voteVariation,
                ulong numberElected,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] selections,
                ulong selectionsSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] string[] primaryPartyIds,
                ulong primaryPartyIdsSize,
                out ContestDescriptionHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_new_with_title_and_parties",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                [MarshalAs(UnmanagedType.LPStr)] string electoralDistrictId,
                ulong sequenceOrder,
                VoteVariationType voteVariation,
                ulong numberElected,
                ulong votesAllowed,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                InternationalizedText.InternationalizedTextHandle ballotTitle,
                InternationalizedText.InternationalizedTextHandle ballotSubTitle,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] selections,
                ulong selectionsSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] string[] primaryPartyIds,
                ulong primaryPartyIdsSize,
                out ContestDescriptionHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(ContestDescriptionType* handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_object_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetObjectId(
                ContestDescriptionHandle handle, out IntPtr objectId);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_electoral_district_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetElectoralDistrictId(
                ContestDescriptionHandle handle, out IntPtr electoralDistrictId);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_sequence_order",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetSequenceOrder(
                ContestDescriptionHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_vote_variation",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern VoteVariationType GetVoteVariationType(
                ContestDescriptionHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_number_elected",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetNumberElected(
                ContestDescriptionHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_votes_allowed",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetVotesAllowed(
                ContestDescriptionHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_name",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetName(
                ContestDescriptionHandle handle, out IntPtr name);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_ballot_title",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetBallotTitle(
                ContestDescriptionHandle handle,
                out InternationalizedText.InternationalizedTextHandle name);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_ballot_subtitle",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetBallotSubTitle(
                ContestDescriptionHandle handle,
                out InternationalizedText.InternationalizedTextHandle name);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_selections_size",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetSelectionsSize(
                ContestDescriptionHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_selection_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetSelectionAtIndex(
                ContestDescriptionHandle handle,
                ulong index,
                out SelectionDescription.SelectionDescriptionHandle partyId);

            [DllImport(DllName, EntryPoint = "eg_contest_description_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CryptoHash(
                ContestDescriptionHandle handle,
                out ElementModQ.ElementModQHandle crypto_hash);
        }

        #endregion

        #region ContestDescriptionWithPlaceholders

        internal static class ContestDescriptionWithPlaceholders
        {
            internal struct ContestDescriptionWithPlaceholdersType { };

            internal class ContestDescriptionWithPlaceholdersHandle
                : ElectionGuardSafeHandle<ContestDescriptionWithPlaceholdersType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = ContestDescriptionWithPlaceholders.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"ContestDescriptionWithPlaceholders Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_contest_description_with_placeholders_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                [MarshalAs(UnmanagedType.LPStr)] string electoralDistrictId,
                ulong sequenceOrder,
                VoteVariationType voteVariation,
                ulong numberElected,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] selections,
                ulong selectionsSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] placeholders,
                ulong placeholdersSize,
                out ContestDescriptionWithPlaceholdersHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_with_placeholders_new_with_title",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                [MarshalAs(UnmanagedType.LPStr)] string electoralDistrictId,
                ulong sequenceOrder,
                VoteVariationType voteVariation,
                ulong numberElected,
                ulong votesAllowed,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                InternationalizedText.InternationalizedTextHandle ballotTitle,
                InternationalizedText.InternationalizedTextHandle ballotSubTitle,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] selections,
                ulong selectionsSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] placeholders,
                ulong placeholdersSize,
                out ContestDescriptionWithPlaceholdersHandle handle);

            [DllImport(DllName,
                EntryPoint = "eg_contest_description_with_placeholders_new_with_parties",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                [MarshalAs(UnmanagedType.LPStr)] string electoralDistrictId,
                ulong sequenceOrder,
                VoteVariationType voteVariation,
                ulong numberElected,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] selections,
                ulong selectionsSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] string[] primaryPartyIds,
                ulong primaryPartyIdsSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] placeholders,
                ulong placeholdersSize,
                out ContestDescriptionWithPlaceholdersHandle handle);

            [DllImport(DllName,
                EntryPoint = "eg_contest_description_with_placeholders_new_with_title_and_parties",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                [MarshalAs(UnmanagedType.LPStr)] string electoralDistrictId,
                ulong sequenceOrder,
                VoteVariationType voteVariation,
                ulong numberElected,
                ulong votesAllowed,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                InternationalizedText.InternationalizedTextHandle ballotTitle,
                InternationalizedText.InternationalizedTextHandle ballotSubTitle,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] selections,
                ulong selectionsSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] string[] primaryPartyIds,
                ulong primaryPartyIdsSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] placeholders,
                ulong placeholdersSize,
                out ContestDescriptionWithPlaceholdersHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_with_placeholders_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(ContestDescriptionWithPlaceholdersType* handle);

            #region ContestDescription Methods

            // Since the underlying c++ class inherits from ContestDescription
            // these functions call those methods substituting the 
            // ContestDescriptionWithPlaceholdersHandle opaque pointer type

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_object_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetObjectId(
                ContestDescriptionWithPlaceholdersHandle handle, out IntPtr objectId);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_electoral_district_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetElectoralDistrictId(
                ContestDescriptionWithPlaceholdersHandle handle, out IntPtr electoralDistrictId);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_sequence_order",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetSequenceOrder(
                ContestDescriptionWithPlaceholdersHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_vote_variation",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern VoteVariationType GetVoteVariationType(
                ContestDescriptionWithPlaceholdersHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_number_elected",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetNumberElected(
                ContestDescriptionWithPlaceholdersHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_votes_allowed",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetVotesAllowed(
                ContestDescriptionWithPlaceholdersHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_name",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetName(
                ContestDescriptionWithPlaceholdersHandle handle, out IntPtr name);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_ballot_title",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetBallotTitle(
                ContestDescriptionWithPlaceholdersHandle handle,
                out InternationalizedText.InternationalizedTextHandle name);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_ballot_subtitle",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetBallotSubTitle(
                ContestDescriptionWithPlaceholdersHandle handle,
                out InternationalizedText.InternationalizedTextHandle name);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_selections_size",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetSelectionsSize(
                ContestDescriptionWithPlaceholdersHandle handle);

            [DllImport(DllName, EntryPoint = "eg_contest_description_get_selection_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetSelectionAtIndex(
                ContestDescriptionWithPlaceholdersHandle handle,
                ulong index,
                out SelectionDescription.SelectionDescriptionHandle partyId);

            [DllImport(DllName, EntryPoint = "eg_contest_description_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CryptoHash(
                ContestDescriptionWithPlaceholdersHandle handle,
                out ElementModQ.ElementModQHandle crypto_hash);

            #endregion

            [DllImport(DllName,
                EntryPoint = "eg_contest_description_with_placeholders_get_placeholders_size",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetPlaceholdersSize(
                ContestDescriptionWithPlaceholdersHandle handle);

            [DllImport(DllName,
                EntryPoint = "eg_contest_description_with_placeholders_get_placeholder_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetPlaceholderAtIndex(
                ContestDescriptionWithPlaceholdersHandle handle,
                ulong index,
                out SelectionDescription.SelectionDescriptionHandle partyId);

            [DllImport(DllName,
                EntryPoint = "eg_contest_description_with_placeholders_is_placeholder",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern bool IsPlaceholder(
                ContestDescriptionWithPlaceholdersHandle handle,
                SelectionDescription.SelectionDescriptionHandle selection);

            [DllImport(DllName,
                EntryPoint = "eg_contest_description_with_placeholders_selection_for_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status SelectionForId(
                ContestDescriptionWithPlaceholdersHandle handle,
                [MarshalAs(UnmanagedType.LPStr)] string selectionId,
                SelectionDescription.SelectionDescriptionHandle selection);
        }

        #endregion

        #region Manifest

        internal static class Manifest
        {
            [DllImport(DllName, EntryPoint = "eg_election_manifest_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string electionScopeId,
                [MarshalAs(UnmanagedType.LPStr)] string specVersion,
                ElectionType electionType,
                ulong startDate,
                ulong endDate,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] gpUnits,
                ulong gpUnitsSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] parties,
                ulong partiesSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] candidates,
                ulong candidatesSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] contests,
                ulong contestSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] ballotStyles,
                ulong ballotStylesSize,
                out ElectionGuard.Manifest.External.ManifestHandle handle);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_new_with_contact",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string electionScopeId,
                [MarshalAs(UnmanagedType.LPStr)] string specVersion,
                ElectionType electionType,
                ulong startDate,
                ulong endDate,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] gpUnits,
                ulong gpUnitsSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] parties,
                ulong partiesSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] candidates,
                ulong candidatesSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] contests,
                ulong contestSize,
                // TODO ISSUE #212: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] ballotStyles,
                ulong ballotStylesSize,
                InternationalizedText.InternationalizedTextHandle name,
                ContactInformation.ContactInformationHandle contact,
                out ElectionGuard.Manifest.External.ManifestHandle handle);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_get_start_date",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetStartDate(
                ElectionGuard.Manifest.External.ManifestHandle handle);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_get_geopolitical_unit_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetGeopoliticalUnitAtIndex(
                ElectionGuard.Manifest.External.ManifestHandle handle,
                ulong index,
                out GeopoliticalUnit.GeopoliticalUnitHandle gpUnit);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_get_party_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetPartyAtIndex(
                ElectionGuard.Manifest.External.ManifestHandle handle,
                ulong index,
                out Party.PartyHandle party);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_get_candidate_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetCandidateAtIndex(
                ElectionGuard.Manifest.External.ManifestHandle handle,
                ulong index,
                out Candidate.CandidateHandle candidate);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_get_contest_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetContestAtIndex(
                ElectionGuard.Manifest.External.ManifestHandle handle,
                ulong index,
                out ContestDescription.ContestDescriptionHandle contest);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_get_ballot_style_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetBallotStyleAtIndex(
                ElectionGuard.Manifest.External.ManifestHandle handle,
                ulong index,
                out BallotStyle.BallotStyleHandle ballotStyle);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CryptoHash(
                ElectionGuard.Manifest.External.ManifestHandle handle,
                out ElementModQ.ElementModQHandle crypto_hash);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_is_valid",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern bool IsValid(ElectionGuard.Manifest.External.ManifestHandle handle);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_from_json",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromJson(
                [MarshalAs(UnmanagedType.LPStr)] string data,
                out ElectionGuard.Manifest.External.ManifestHandle handle);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_from_bson",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromBson(
                byte* data, ulong length, out ElectionGuard.Manifest.External.ManifestHandle handle);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_from_msgpack",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromMsgPack(
                byte* data, ulong length, out ElectionGuard.Manifest.External.ManifestHandle handle);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_to_json",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToJson(
                ElectionGuard.Manifest.External.ManifestHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_to_bson",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToBson(
                ElectionGuard.Manifest.External.ManifestHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_election_manifest_to_msgpack",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToMsgPack(
                ElectionGuard.Manifest.External.ManifestHandle handle, out IntPtr data, out ulong size);
        }

        #endregion

        #region InternalManifest

        internal static class InternalManifest
        {
            internal struct InternalManifestType { };

            internal class InternalManifestHandle
                : ElectionGuardSafeHandle<InternalManifestType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = InternalManifest.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"InternalManifest Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_internal_manifest_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                ElectionGuard.Manifest.External.ManifestHandle manifest,
                out InternalManifestHandle handle);

            [DllImport(DllName, EntryPoint = "eg_internal_manifest_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(
                InternalManifestType* handle);

            [DllImport(DllName, EntryPoint = "eg_internal_manifest_get_manifest_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetManifestHash(
                InternalManifestHandle handle,
                out ElementModQ.ElementModQHandle manifest_hash);

            [DllImport(DllName,
                EntryPoint = "eg_internal_manifest_get_geopolitical_units_size",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetGeopoliticalUnitsSize(
                InternalManifestHandle handle);

            [DllImport(DllName,
                EntryPoint = "eg_internal_manifest_get_geopolitical_unit_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetGeopoliticalUnitAtIndex(
                InternalManifestHandle handle,
                ulong index,
                out GeopoliticalUnit.GeopoliticalUnitHandle gpUnit);

            [DllImport(DllName, EntryPoint = "eg_internal_manifest_get_contests_size",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetContestsSize(
                InternalManifestHandle handle);

            [DllImport(DllName, EntryPoint = "eg_internal_manifest_get_contest_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetContestAtIndex(
                InternalManifestHandle handle,
                ulong index,
                out ContestDescriptionWithPlaceholders.ContestDescriptionWithPlaceholdersHandle contest);

            [DllImport(DllName, EntryPoint = "eg_internal_manifest_get_ballot_styles_size",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetBallotStylesSize(
                InternalManifestHandle handle);

            [DllImport(DllName, EntryPoint = "eg_internal_manifest_get_ballot_style_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetBallotStyleAtIndex(
                InternalManifestHandle handle,
                ulong index,
                out BallotStyle.BallotStyleHandle ballotStyle);

            [DllImport(DllName, EntryPoint = "eg_internal_manifest_from_json",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromJson(
                [MarshalAs(UnmanagedType.LPStr)] string data,
                out InternalManifestHandle handle);

            [DllImport(DllName, EntryPoint = "eg_internal_manifest_from_bson",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromBson(
                byte* data, ulong length, out InternalManifestHandle handle);

            [DllImport(DllName, EntryPoint = "eg_internal_manifest_from_msgpack",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromMsgPack(
                byte* data, ulong length, out InternalManifestHandle handle);

            [DllImport(DllName, EntryPoint = "eg_internal_manifest_to_json",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToJson(
                InternalManifestHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_internal_manifest_to_bson",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToBson(
                InternalManifestHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_internal_manifest_to_msgpack",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToMsgPack(
                InternalManifestHandle handle, out IntPtr data, out ulong size);
        }

        #endregion

        #region ContextConfiguration
        internal static class ContextConfiguration
        {
            internal struct ContextConfigurationType { };

            internal class ContextConfigurationHandle
                : ElectionGuardSafeHandle<ContextConfigurationType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = ContextConfiguration.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"ContextConfiguration Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_election_context_config_get_allowed_overvotes",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetAllowedOverVotes(
                ContextConfigurationHandle handle,
                ref bool allowed_overvotes);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_election_context_config_get_max_ballots",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetMaxBallots(
                ContextConfigurationHandle handle,
                ref UInt64 max_ballots);


            [DllImport(DllName, EntryPoint = "eg_ciphertext_election_context_config_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(ContextConfigurationType* handle);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_election_context_config_make",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Make(
                bool allow_overvotes,
                UInt64 max_ballots,
                out ContextConfigurationHandle handle);
        }

        #endregion

        #region CiphertextElectionContext

        internal static class CiphertextElectionContext
        {
            internal struct CiphertextElectionType { };

            internal class CiphertextElectionContextHandle
                : ElectionGuardSafeHandle<CiphertextElectionType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = CiphertextElectionContext.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"CiphertextElectionContext Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_ciphertext_election_context_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(CiphertextElectionType* handle);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_election_context_get_configuration",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetConfiguration(
                CiphertextElectionContextHandle handle,
                out ContextConfiguration.ContextConfigurationHandle context_config);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_election_context_get_number_of_guardians",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetNumberOfGuardians(
                CiphertextElectionContextHandle handle,
                ref ulong number_of_guardians);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_election_context_get_quorum",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetQuorum(
                CiphertextElectionContextHandle handle,
                ref ulong quorum);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_election_context_get_elgamal_public_key",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetElGamalPublicKey(
                CiphertextElectionContextHandle handle,
                out ElementModP.ElementModPHandle elgamal_public_key);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_election_context_get_commitment_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetCommitmentHash(
                CiphertextElectionContextHandle handle,
                out ElementModQ.ElementModQHandle commitment_hash);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_election_context_get_manifest_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetManifestHash(
                CiphertextElectionContextHandle handle,
                out ElementModQ.ElementModQHandle manifest_hash);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_election_context_get_crypto_base_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetCryptoBaseHash(
                CiphertextElectionContextHandle handle,
                out ElementModQ.ElementModQHandle crypto_base_hash);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_election_context_get_crypto_extended_base_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetCryptoExtendedBaseHash(
                CiphertextElectionContextHandle handle,
                out ElementModQ.ElementModQHandle crypto_extended_base_hash);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_election_context_get_extended_data",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetExtendedData(
                CiphertextElectionContextHandle handle,
                out LinkedList.LinkedListHandle extended_data);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_election_context_make",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Make(
                ulong number_of_guardians,
                ulong quorum,
                ElementModP.ElementModPHandle elgamal_public_key,
                ElementModQ.ElementModQHandle commitment_hash,
                ElementModQ.ElementModQHandle manifest_hash,
                out CiphertextElectionContextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_election_context_make_with_configuration",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Make(
                ulong number_of_guardians,
                ulong quorum,
                ElementModP.ElementModPHandle elgamal_public_key,
                ElementModQ.ElementModQHandle commitment_hash,
                ElementModQ.ElementModQHandle manifest_hash,
                ContextConfiguration.ContextConfigurationHandle configuration,
                out CiphertextElectionContextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_election_context_make_with_extended_data",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Make(
                ulong number_of_guardians,
                ulong quorum,
                ElementModP.ElementModPHandle elgamal_public_key,
                ElementModQ.ElementModQHandle commitment_hash,
                ElementModQ.ElementModQHandle manifest_hash,
                LinkedList.LinkedListHandle extended_data,
                out CiphertextElectionContextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_election_context_make_with_configuration_and_extended_data",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Make(
                ulong number_of_guardians,
                ulong quorum,
                ElementModP.ElementModPHandle elgamal_public_key,
                ElementModQ.ElementModQHandle commitment_hash,
                ElementModQ.ElementModQHandle manifest_hash,
                ContextConfiguration.ContextConfigurationHandle configuration,
                LinkedList.LinkedListHandle extended_data,
                out CiphertextElectionContextHandle handle);


            [DllImport(DllName, EntryPoint = "eg_ciphertext_election_context_make_from_hex",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Make(
                ulong number_of_guardians,
                ulong quorum,
                [MarshalAs(UnmanagedType.LPStr)] string hex_elgamal_public_key,
                [MarshalAs(UnmanagedType.LPStr)] string hex_commitment_hash,
                [MarshalAs(UnmanagedType.LPStr)] string hex_manifest_hash,
                out CiphertextElectionContextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_election_context_make_from_hex_with_extended_data",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Make(
                ulong number_of_guardians,
                ulong quorum,
                [MarshalAs(UnmanagedType.LPStr)] string hex_elgamal_public_key,
                [MarshalAs(UnmanagedType.LPStr)] string hex_commitment_hash,
                [MarshalAs(UnmanagedType.LPStr)] string hex_manifest_hash,
                LinkedList.LinkedListHandle extended_data,
                out CiphertextElectionContextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_election_context_from_json",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromJson(
                [MarshalAs(UnmanagedType.LPStr)] string data,
                out CiphertextElectionContextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_election_context_from_bson",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromBson(
                uint* data, ulong length, CiphertextElectionContextHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_election_context_to_json",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToJson(
                CiphertextElectionContextHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_election_context_to_bson",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToBson(
                CiphertextElectionContextHandle handle, out uint* data, out ulong size);
        }

        #endregion

        #region ExtendedData

        internal static class ExtendedData
        {
            internal struct ExtendedDataType { };

            internal class ExtendedDataHandle
                : ElectionGuardSafeHandle<ExtendedDataType>
            {
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = ExtendedData.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"ExtendedData Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_extended_data_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string value,
                ulong length,
                out ExtendedDataHandle handle);

            [DllImport(DllName, EntryPoint = "eg_extended_data_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(ExtendedDataType* handle);

            [DllImport(DllName, EntryPoint = "eg_extended_data_get_value",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetValue(
                ExtendedDataHandle handle, out IntPtr object_id);

            [DllImport(DllName, EntryPoint = "eg_extended_data_get_length",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetLength(ExtendedDataHandle handle);

        }

        #endregion

        #region Ballot

        internal static class PlaintextBallotSelection
        {
            [DllImport(DllName, EntryPoint = "eg_plaintext_ballot_selection_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                ulong vote,
                bool isPlaceholderSelection,
                out ElectionGuard.PlaintextBallotSelection.External.PlaintextBallotSelectionHandle handle);

            [DllImport(DllName,
                EntryPoint = "eg_plaintext_ballot_selection_new_with_extended_data",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                ulong vote,
                bool isPlaceholderSelection,
                [MarshalAs(UnmanagedType.LPStr)] string extendedData,
                ulong extendedDataLength,
                out ElectionGuard.PlaintextBallotSelection.External.PlaintextBallotSelectionHandle handle);
        }

        internal static class PlaintextBallotContest
        {
            [DllImport(DllName, EntryPoint = "eg_plaintext_ballot_contest_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                // TODO: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] selections,
                ulong selectionsSize,
                out ElectionGuard.PlaintextBallotContest.External.PlaintextBallotContestHandle handle);
        }

        internal static class CiphertextBallotContest
        {
            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_get_selection_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetSelectionAtIndex(
                ElectionGuard.CiphertextBallotContest.External.CiphertextBallotContestHandle handle,
                ulong index,
                out CiphertextBallotSelection.External.CiphertextBallotSelectionHandle selection);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_get_ciphertext_accumulation",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetCiphertextAccumulation(
                ElectionGuard.CiphertextBallotContest.External.CiphertextBallotContestHandle handle,
                out ElGamalCiphertext.ElGamalCiphertextHandle nonce);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_get_proof",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetProof(
                ElectionGuard.CiphertextBallotContest.External.CiphertextBallotContestHandle handle,
                out RangedChaumPedersenProof.RangedChaumPedersenProofHandle nonce);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_crypto_hash_with",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CryptoHashWith(
                ElectionGuard.CiphertextBallotContest.External.CiphertextBallotContestHandle handle,
                ElementModQ.ElementModQHandle encryption_seed,
                out ElementModQ.ElementModQHandle crypto_hash);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_contest_nonce",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ContestNonce(
                CiphertextElectionContext.CiphertextElectionContextHandle handle,
                ulong sequence_order,
                ElementModQ.ElementModQHandle nonce_seed,
                out ElementModQ.ElementModQHandle contest_nonce);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_get_extended_data",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetExtendedData(
                ElectionGuard.CiphertextBallotContest.External.CiphertextBallotContestHandle handle,
                out HashedElGamalCiphertext.HashedElGamalCiphertextHandle extendedData);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_aggregate_nonce",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status AggregateNonce(
                ElectionGuard.CiphertextBallotContest.External.CiphertextBallotContestHandle handle,
                out ElementModQ.ElementModQHandle aggregate_nonce);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_elgamal_accumulate",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ElGamalAccumulate(
                ElectionGuard.CiphertextBallotContest.External.CiphertextBallotContestHandle handle,
                out ElGamalCiphertext.ElGamalCiphertextHandle ciphertext_accumulation);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_is_valid_encryption",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern bool IsValidEncryption(
                ElectionGuard.CiphertextBallotContest.External.CiphertextBallotContestHandle handle,
                ElementModQ.ElementModQHandle encryption_seed,
                ElementModP.ElementModPHandle public_key,
                ElementModQ.ElementModQHandle crypto_extended_base_hash);
        }

        internal static class PlaintextBallot
        {
            internal struct PlaintextBallotType { };

            [DllImport(DllName,
                EntryPoint = "eg_plaintext_ballot_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                [MarshalAs(UnmanagedType.LPStr)] string styleId,
                // TODO: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] contests,
                ulong contestsSize,
                out ElectionGuard.PlaintextBallot.External.PlaintextBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_plaintext_ballot_from_json",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromJson(
                [MarshalAs(UnmanagedType.LPStr)] string data,
                out ElectionGuard.PlaintextBallot.External.PlaintextBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_plaintext_ballot_from_bson",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromBson(
                byte* data, ulong length, out ElectionGuard.PlaintextBallot.External.PlaintextBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_plaintext_ballot_from_msgpack",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromMsgPack(
                byte* data, ulong length, out ElectionGuard.PlaintextBallot.External.PlaintextBallotHandle handle);

            // TODO: implement MsgPackFree
        }

        internal static class CompactPlaintextBallot
        {
            internal struct CompactPlaintextBallotType { };

            internal class CompactPlaintextBallotHandle
                : ElectionGuardSafeHandle<CompactPlaintextBallotType>
            {
#if NETSTANDARD
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = CompactPlaintextBallot.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"CompactPlaintextBallot Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_compact_plaintext_ballot_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(CompactPlaintextBallotType* handle);

            [DllImport(DllName, EntryPoint = "eg_compact_plaintext_ballot_from_msgpack",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromMsgPack(
                byte* data, ulong size, out CompactPlaintextBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_compact_plaintext_ballot_to_msgpack",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToMsgPack(
                CompactPlaintextBallotHandle handle, out IntPtr data, out ulong size);
        }

        internal static class CiphertextBallot
        {
            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_ballot_get_state",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern BallotBoxState GetState(
                ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_get_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetCryptoHash(
                ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle,
                out ElementModQ.ElementModQHandle hash_ref);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_crypto_hash_with",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CryptoHashWith(
                ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle,
                ElementModQ.ElementModQHandle manifest_hash,
                out ElementModQ.ElementModQHandle crypto_hash);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_nonce_seed",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status NonceSeed(
                ElementModQ.ElementModQHandle manifestHash,
                [MarshalAs(UnmanagedType.LPStr)] string objectId,
                ElementModQ.ElementModQHandle nonce,
                out ElementModQ.ElementModQHandle nonceSeed);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_ballot_cast",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status Cast(
                ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_ballot_spoil",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status Spoil(
                ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle);

            [DllImport(DllName,
                EntryPoint = "eg_ciphertext_ballot_challenge",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status Challenge(
                ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_from_json",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromJson(
                [MarshalAs(UnmanagedType.LPStr)] string data,
                out ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_from_bson",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromBson(
                byte* data, ulong length, out ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_from_msgpack",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromMsgPack(
                byte* data, ulong length, out ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_to_json",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToJson(
                ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_to_json_with_nonces",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToJsonWithNonces(
                ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_to_bson",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToBson(
                ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_to_bson_with_nonces",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToBsonWithNonces(
                ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_to_msgpack",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToMsgPack(
                ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_to_msgpack_with_nonces",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToMsgPackWithNonces(
                ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle, out IntPtr data, out ulong size);
        }

        internal static class CompactCiphertextBallot
        {
            internal struct CompactCiphertextBallotType { };

            internal class CompactCiphertextBallotHandle
                : ElectionGuardSafeHandle<CompactCiphertextBallotType>
            {
#if NETSTANDARD
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = CompactCiphertextBallot.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"CompactCiphertextBallot Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_compact_ciphertext_ballot_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(CompactCiphertextBallotType* handle);

            [DllImport(DllName, EntryPoint = "eg_compact_ciphertext_ballot_get_object_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetObjectId(
                CompactCiphertextBallotHandle handle, out IntPtr object_id);

            [DllImport(DllName, EntryPoint = "eg_compact_ciphertext_ballot_from_msgpack",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromMsgPack(
                byte* data, ulong size, out CompactCiphertextBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_compact_ciphertext_ballot_to_msgpack",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToMsgPack(
                CompactCiphertextBallotHandle handle, out IntPtr data, out ulong size);

        }

        #endregion

        #region SubmittedBallot

        internal static class SubmittedBallot
        {
            internal struct SubmittedBallotType { };

            internal class SubmittedBallotHandle
                : ElectionGuardSafeHandle<SubmittedBallotType>
            {
#if NETSTANDARD
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
                protected override bool Free()
                {
                    if (IsFreed) return true;

                    var status = SubmittedBallot.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"SubmittedBallot Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_submitted_ballot_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(SubmittedBallotType* handle);

            [DllImport(DllName, EntryPoint = "eg_submitted_ballot_get_state",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern BallotBoxState GetState(SubmittedBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_submitted_ballot_from",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status From(
                ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle ciphertext,
                BallotBoxState state,
                out SubmittedBallotHandle handle);

            #region CiphertextBallot Methods

            // Since the underlying c++ class inherits from CiphertextBallot
            // these functions call those methods subsituting the SubmittedBallot opaque pointer type

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_get_object_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetObjectId(
                SubmittedBallotHandle handle, out IntPtr object_id);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_get_style_id",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetStyleId(
               SubmittedBallotHandle handle, out IntPtr style_id);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_get_manifest_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetManifestHash(
                SubmittedBallotHandle handle,
                out ElementModQ.ElementModQHandle manifest_hash_ref);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_get_ballot_code_seed",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetBallotCodeSeed(
                SubmittedBallotHandle handle,
                out ElementModQ.ElementModQHandle ballot_code_seed_ref);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_get_contests_size",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetContestsSize(SubmittedBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_get_contest_at_index",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetContestAtIndex(
                SubmittedBallotHandle handle,
                ulong index,
                out ElectionGuard.CiphertextBallotContest.External.CiphertextBallotContestHandle contest_ref);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_get_ballot_code",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetBallotCode(
                SubmittedBallotHandle handle,
                out ElementModQ.ElementModQHandle ballot_code_ref);

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_get_timestamp",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern ulong GetTimestamp(
                SubmittedBallotHandle handle);

            // GetNonce is not provided

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_get_crypto_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetCryptoHash(
                SubmittedBallotHandle handle,
                out ElementModQ.ElementModQHandle hash_ref);

            // CryptoHashWith is not provided

            [DllImport(DllName, EntryPoint = "eg_ciphertext_ballot_is_valid_encryption",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern bool IsValidEncryption(
                SubmittedBallotHandle handle,
                ElementModQ.ElementModQHandle manifest_hash,
                ElementModP.ElementModPHandle public_key,
                ElementModQ.ElementModQHandle crypto_extended_base_hash);

            #endregion

            [DllImport(DllName, EntryPoint = "eg_submitted_ballot_from_json",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromJson(
                [MarshalAs(UnmanagedType.LPStr)] string data,
                out SubmittedBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_submitted_ballot_from_bson",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromBson(
                byte* data, ulong length, out SubmittedBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_submitted_ballot_from_msgpack",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromMsgPack(
                byte* data, ulong length, out SubmittedBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_submitted_ballot_to_json",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToJson(
                SubmittedBallotHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_submitted_ballot_to_bson",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToBson(
                SubmittedBallotHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_submitted_ballot_to_msgpack",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToMsgPack(
                SubmittedBallotHandle handle, out IntPtr data, out ulong size);

        }

        #endregion

        #region Encrypt

        internal static class EncryptionDevice
        {
            internal struct EncryptionDeviceType { };

            internal class EncryptionDeviceHandle
                : ElectionGuardSafeHandle<EncryptionDeviceType>
            {
#if NETSTANDARD
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = EncryptionDevice.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"EncryptionDevice Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_encryption_device_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                ulong deviceUuid,
                ulong sessionUuid,
                ulong launchCode,
                [MarshalAs(UnmanagedType.LPStr)] string location,
                out EncryptionDeviceHandle handle);

            [DllImport(DllName, EntryPoint = "eg_encryption_device_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]

            internal static extern Status Free(EncryptionDeviceType* handle);

            [DllImport(DllName, EntryPoint = "eg_encryption_device_get_timestamp",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetTimestamp(
                EncryptionDeviceHandle handle, out ulong timestamp);

            [DllImport(DllName, EntryPoint = "eg_encryption_device_get_uuid",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetDeviceUuid(
                EncryptionDeviceHandle handle, out ulong device_uuid);

            [DllImport(DllName, EntryPoint = "eg_encryption_device_get_session_uuid",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetSessionUuid(
                EncryptionDeviceHandle handle, out ulong session_uuid);

            [DllImport(DllName, EntryPoint = "eg_encryption_device_get_launch_code",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetLaunchCode(
                EncryptionDeviceHandle handle, out ulong launch_code);

            [DllImport(DllName, EntryPoint = "eg_encryption_device_get_location",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetLocation(
                EncryptionDeviceHandle handle, out IntPtr data, out ulong size);

            [DllImport(DllName, EntryPoint = "eg_encryption_device_get_hash",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetHash(
                EncryptionDeviceHandle handle,
                out ElementModQ.ElementModQHandle device_hash);

            //hooking up jsonular calls
            [DllImport(DllName, EntryPoint = "eg_encryption_device_from_json",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FromJson(
                [MarshalAs(UnmanagedType.LPStr)] string data,
                out EncryptionDeviceHandle handle);

            [DllImport(DllName, EntryPoint = "eg_encryption_device_to_json",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status ToJson(
                EncryptionDeviceHandle handle, out IntPtr data, out ulong size);
        }

        internal static class EncryptionMediator
        {
            internal struct EncryptionMediatorType { };

            internal class EncryptionMediatorHandle
                : ElectionGuardSafeHandle<EncryptionMediatorType>
            {
#if NETSTANDARD
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = EncryptionMediator.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"EncryptionMediator Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_encryption_mediator_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                InternalManifest.InternalManifestHandle manifest,
                CiphertextElectionContext.CiphertextElectionContextHandle context,
                EncryptionDevice.EncryptionDeviceHandle device,
                out EncryptionMediatorHandle handle);

            [DllImport(DllName, EntryPoint = "eg_encryption_mediator_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(EncryptionMediatorType* handle);

            [DllImport(DllName, EntryPoint = "eg_encryption_mediator_compact_encrypt_ballot",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CompactEncrypt(
                EncryptionMediatorHandle handle,
                ElectionGuard.PlaintextBallot.External.PlaintextBallotHandle plaintext,
                out CompactCiphertextBallot.CompactCiphertextBallotHandle ciphertext);

            [DllImport(DllName,
                EntryPoint = "eg_encryption_mediator_compact_encrypt_ballot_verify_proofs",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CompactEncryptAndVerify(
                EncryptionMediatorHandle handle,
                ElectionGuard.PlaintextBallot.External.PlaintextBallotHandle plaintext,
                out CompactCiphertextBallot.CompactCiphertextBallotHandle ciphertext);

            [DllImport(DllName, EntryPoint = "eg_encryption_mediator_encrypt_ballot",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            // ReSharper disable once MemberHidesStaticFromOuterClass
            internal static extern Status Encrypt(
                EncryptionMediatorHandle handle,
                ElectionGuard.PlaintextBallot.External.PlaintextBallotHandle plaintext,
                bool usePrecomputedValues,
                out ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle ciphertext);

            [DllImport(DllName,
                EntryPoint = "eg_encryption_mediator_encrypt_ballot_verify_proofs",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status EncryptAndVerify(
                EncryptionMediatorHandle handle,
                ElectionGuard.PlaintextBallot.External.PlaintextBallotHandle plaintext,
                bool usePrecomputedValues,
                out ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle ciphertext);
        }

        internal static class Encrypt
        {
            [DllImport(DllName, EntryPoint = "eg_encrypt_selection",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Selection(
                ElectionGuard.PlaintextBallotSelection.External.PlaintextBallotSelectionHandle plaintext,
                SelectionDescription.SelectionDescriptionHandle description,
                ElementModP.ElementModPHandle publicKey,
                ElementModQ.ElementModQHandle crypto_extended_base_hash,
                ElementModQ.ElementModQHandle nonceSeed,
                bool shouldVerifyProofs,
                bool usePrecomputedValues,
                out CiphertextBallotSelection.External.CiphertextBallotSelectionHandle handle);

            [DllImport(DllName, EntryPoint = "eg_encrypt_contest",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Contest(
                ElectionGuard.PlaintextBallotContest.External.PlaintextBallotContestHandle plaintext,
                ContestDescription.ContestDescriptionHandle description,
                ElementModP.ElementModPHandle publicKey,
                ElementModQ.ElementModQHandle crypto_extended_base_hash,
                ElementModQ.ElementModQHandle nonceSeed,
                bool shouldVerifyProofs,
                bool usePrecomputedValues,
                out ElectionGuard.CiphertextBallotContest.External.CiphertextBallotContestHandle handle);

            [DllImport(DllName, EntryPoint = "eg_encrypt_ballot",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Ballot(
                ElectionGuard.PlaintextBallot.External.PlaintextBallotHandle plaintext,
                InternalManifest.InternalManifestHandle internal_manifest,
                CiphertextElectionContext.CiphertextElectionContextHandle context,
                ElementModQ.ElementModQHandle ballot_code_seed,
                bool shouldVerifyProofs,
                bool usePrecomputedValues,
                out ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_encrypt_ballot_with_nonce",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Ballot(
                ElectionGuard.PlaintextBallot.External.PlaintextBallotHandle plaintext,
                InternalManifest.InternalManifestHandle internal_manifest,
                CiphertextElectionContext.CiphertextElectionContextHandle context,
                ElementModQ.ElementModQHandle ballot_code_seed,
                ElementModQ.ElementModQHandle nonce,
                ulong timestamp,
                bool shouldVerifyProofs,
                out ElectionGuard.CiphertextBallot.External.CiphertextBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_encrypt_compact_ballot",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CompactBallot(
                ElectionGuard.PlaintextBallot.External.PlaintextBallotHandle plaintext,
                InternalManifest.InternalManifestHandle internal_manifest,
                CiphertextElectionContext.CiphertextElectionContextHandle context,
                ElementModQ.ElementModQHandle ballot_code_seed,
                bool shouldVerifyProofs,
                out CompactCiphertextBallot.CompactCiphertextBallotHandle handle);

            [DllImport(DllName, EntryPoint = "eg_encrypt_compact_ballot_with_nonce",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status CompactBallot(
                ElectionGuard.PlaintextBallot.External.PlaintextBallotHandle plaintext,
                InternalManifest.InternalManifestHandle internal_manifest,
                CiphertextElectionContext.CiphertextElectionContextHandle context,
                ElementModQ.ElementModQHandle ballot_code_seed,
                ElementModQ.ElementModQHandle nonce,
                ulong timestamp,
                bool shouldVerifyProofs,
                out CompactCiphertextBallot.CompactCiphertextBallotHandle handle);
        }

        #endregion

        #region Precompute

        internal static class PrecomputeBuffer
        {
            internal struct PrecomputeBufferType { };

            internal class PrecomputeBufferHandle
                : ElectionGuardSafeHandle<PrecomputeBufferType>
            {
#if NETSTANDARD
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
                protected override bool Free()
                {
                    if (IsClosed) return true;

                    var status = PrecomputeBuffer.Free(TypedPtr);
                    if (status != ElectionGuard.Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"EncryptionDevice Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(DllName, EntryPoint = "eg_precompute_buffer_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status New(
                ElementModP.ElementModPHandle publicKey, int maxBufferSize,
                bool shouldAutoPopulate, out PrecomputeBufferHandle handle);

            [DllImport(DllName, EntryPoint = "eg_precompute_buffer_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(PrecomputeBufferType* handle);

            [DllImport(DllName, EntryPoint = "eg_precompute_buffer_start",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Start(PrecomputeBufferHandle handle);

            [DllImport(DllName, EntryPoint = "eg_precompute_buffer_stop",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Stop(PrecomputeBufferHandle handle);

            [DllImport(DllName, EntryPoint = "eg_precompute_buffer_status",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Status(
                PrecomputeBufferHandle handle, out int count, out int queue_size);

        }

        internal static class PrecomputeBufferContext
        {
            [DllImport(DllName, EntryPoint = "eg_precompute_buffer_context_initialize",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Initialize(
                ElementModP.ElementModPHandle publicKey, int maxBufferSize);

            [DllImport(DllName, EntryPoint = "eg_precompute_buffer_context_start",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Start();

            [DllImport(DllName, EntryPoint = "eg_precompute_buffer_context_start_new",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Start(ElementModP.ElementModPHandle publicKey);

            [DllImport(DllName, EntryPoint = "eg_precompute_buffer_context_stop",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Stop();

            [DllImport(DllName, EntryPoint = "eg_precompute_buffer_context_status",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Status(out int count, out int queue_size);
        }

        #endregion

        #region Memory
        internal static class Memory
        {
            [DllImport(DllName, EntryPoint = "eg_free_int_ptr",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status FreeIntPtr(IntPtr data);

            [DllImport(DllName, EntryPoint = "eg_delete_int_ptr",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status DeleteIntPtr(IntPtr data);
        }
        #endregion

        internal static class ExceptionHandler
        {
            [DllImport(DllName, EntryPoint = "eg_exception_data",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status GetData(
                out IntPtr function, out IntPtr message, out ulong size);
        }


    }
}
