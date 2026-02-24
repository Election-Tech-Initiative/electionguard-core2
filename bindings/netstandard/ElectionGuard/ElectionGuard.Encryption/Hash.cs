using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ElectionGuard.Base;

namespace ElectionGuard
{

    /// <summary>
    /// Class for wrapping hashing methods
    /// </summary>
    public static class Hash
    {
        /// <Summary>
        /// HP = H(HV ;00,p,q,g). Parameter Hash 3.1.2
        /// </Summary>
        public static string Prefix_ParameterHash
        {
            get
            {
                var ptr = External.GetPrefix_ParameterHash();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// HM = H(HP;01,manifest). Manifest Hash 3.1.4
        /// </Summary>
        public static string Prefix_ManifestHash
        {
            get
            {
                var ptr = External.GetPrefix_ManifestHash();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// HB = (HP;02,n,k,date,info,HM). Election Base Hash 3.1.5
        /// </Summary>
        public static string Prefix_BaseHash
        {
            get
            {
                var ptr = External.GetPrefix_BaseHash();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        ///  H = (HP ; 10, i, j, Ki,j , hi,j ). Guardin Share proof challenge 3.2.2
        /// </Summary>
        public static string Prefix_GuardianShareChallenge
        {
            get
            {
                var ptr = External.GetPrefix_GuardianShareChallenge();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// ki,l = H(HP;11,i,l,Kl,αi,l,βi,l). (14) Guardain Share Encryption Secret Key 3.2.2
        /// </Summary>
        public static string Prefix_GuardianShareSecret
        {
            get
            {
                var ptr = External.GetPrefix_GuardianShareSecret();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// HE = H(HB;12,K,K1,0,K1,1,...,K1,k−1,K2,0,...,Kn,k−2,Kn,k−1). Extended Base Hash 3.2.3
        /// </Summary>
        public static string Prefix_ExtendedHash
        {
            get
            {
                var ptr = External.GetPrefix_ExtendedHash();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// ξi,j = H(HE;20,ξB,Λi,λj). encryption nonce 3.3.2
        /// </Summary>
        public static string Prefix_SelectionNonce
        {
            get
            {
                var ptr = External.GetPrefix_SelectionNonce();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// c = H(HE;21,K,α,β,a0,b0,a1,b1,...,aL,bL). Ballot Selection Encryption Proof (ranged) 3.3.5
        /// c = H(HE;21,K,α,β,a0,b0,a1,b1). Ballot Selection Encryption Proof (unselected) 3.3.5
        /// c = H(HE;21,K,α,β,a0,b0,a1,b1). Ballot Selection Encryption Proof (selected) 3.3.5
        /// </Summary>
        public static string Prefix_SelectionProof
        {
            get
            {
                var ptr = External.GetPrefix_SelectionProof();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// ξ = H (HE ; 20, ξB , Λ, ”contest data”). Ballot Contest Data Nonce 3.3.6
        /// </Summary>
        public static string Prefix_ContestDataNonce
        {
            get
            {
                var ptr = External.GetPrefix_ContestDataNonce();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// k = H(HE;22,K,α,β). Ballot ContestData Secret Key 3.3.6
        /// </Summary>
        public static string Prefix_ContestDataSecret
        {
            get
            {
                var ptr = External.GetPrefix_ContestDataSecret();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// c = H(HE;21,K,α ̄,β ̄,a0,b0,a1,b1,...,aL,bL). Ballot Contest Limit Encryption Proof 3.3.8
        /// </Summary>
        public static string Prefix_ContestProof
        {
            get
            {
                var ptr = External.GetPrefix_ContestProof();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// χl = H(HE;23,Λl,K,α1,β1,α2,β2 ...,αm,βm). Contest Hash 3.4.1
        /// </Summary>
        public static string Prefix_ContestHash
        {
            get
            {
                var ptr = External.GetPrefix_ContestHash();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// H(B) = H(HE;24,χ1,χ2,...,χmB ,Baux). Confirmation Code 3.4.2
        /// </Summary>
        public static string Prefix_BallotCode
        {
            get
            {
                var ptr = External.GetPrefix_BallotCode();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// H0 = H(HE;24,Baux,0), Ballot Chaining for Fixed Device 3.4.3
        /// H = H(HE;24,Baux), Ballot chaining closure
        /// </Summary>
        public static string Prefix_BallotChain
        {
            get
            {
                var ptr = External.GetPrefix_BallotChain();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// c = H(HE;30,K,A,B,a,b,M). Ballot Selection Decryption Proof 3.6.3
        /// c = H(HE;30,K,α,β,a,b,M). Challenge Ballot Selection Decryption Proof 3.6.5
        /// </Summary>
        public static string Prefix_DecryptSelectionProof
        {
            get
            {
                var ptr = External.GetPrefix_DecryptSelectionProof();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// c = H(HE;31,K,C0,C1,C2,a,b,β). Ballot Contest Decryption of Contest Data 3.6.4
        /// </Summary>
        public static string Prefix_DescryptContestData
        {
            get
            {
                var ptr = External.GetPrefix_DescryptContestData();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <summary>
        /// Hash together the values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        public static ElementModQ HashElems(string first)
        {
            var status = External.HashElems(first,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        public static ElementModQ HashElems(ulong first)
        {
            var status = External.HashElems(first,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        public static ElementModQ HashElems(string first, ulong second)
        {
            var status = External.HashElems(first, second,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        public static ElementModQ HashElems(ulong first, ulong second)
        {
            var status = External.HashElems(first, second,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        /// <param name="third">third value for the hash</param>
        public static ElementModQ HashElems(string first, ulong second, ElementModP third)
        {
            var status = External.HashElems(first, second, third.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        /// <param name="third">third value for the hash</param>
        public static ElementModQ HashElems(string first, ulong second, ElementModQ third)
        {
            var status = External.HashElems(first, second, third.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        public static ElementModQ HashElems(ElementModP first, ElementModP second)
        {
            var status = External.HashElems(first.Handle, second.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        public static ElementModQ HashElems(ElementModP first, ElementModQ second)
        {
            var status = External.HashElems(first.Handle, second.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        public static ElementModQ HashElems(ElementModQ first, ElementModQ second)
        {
            var status = External.HashElems(first.Handle, second.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        public static ElementModQ HashElems(ElementModQ first, ElementModP second)
        {
            var status = External.HashElems(first.Handle, second.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        public static ElementModQ HashElems(ElementModQ first, ulong second)
        {
            var status = External.HashElems(first.Handle, second,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="data">List of ElementModP to hash together</param>
        public static ElementModQ HashElems(List<ElementModP> data)
        {
            var dataPointers = new IntPtr[data.Count];
            for (var i = 0; i < data.Count; i++)
            {
                dataPointers[i] = data[i].Handle.Ptr;
                // TODO: Do we really want to dispose here?
                data[i].Dispose();
            }

            var status = External.HashElems(dataPointers, (ulong)data.Count,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ICryptoHashableType values
        /// </summary>
        public static ElementModQ HashElems(
            ElementModQ seed, params ICryptoHashableType[] data)
        {
            // TODO: ISSUE #239 we should define a clear method for 
            // passing hashable elements back and forth
            // between C# and C++ so we don't have to do this
            foreach (var item in data)
            {
                if (item is CryptoHashableBase)
                {
                    if (item is ElementModP p)
                    {
                        using (var combined = HashElems(seed, p))
                        {
                            seed.Reassign(combined);
                        }

                    }
                    else if (item is ElementModQ q)
                    {

                        using (var combined = HashElems(seed, q))
                        {
                            seed.Reassign(combined);
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("data must be of type CryptoHashableBase");
                }
            }
            return seed;
        }

        /// <summary>
        /// Hash together the ICryptoHashableType values
        /// </summary>
        public static ElementModQ HashElems(
            string prefix, params ICryptoHashableType[] data)
        {
            var hashed = HashElems(prefix);
            return HashElems(hashed, data);
        }

        public static ElementModQ HashElems(
            ElementModQ header, string prefix, params ICryptoHashableType[] data)
        {
            var hashedPrefix = HashElems(prefix);
            var hashed = HashElems(header, hashedPrefix);
            return HashElems(hashed, data);
        }

        internal static class External
        {
            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_parameter_hash",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_ParameterHash();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_manifest_hash",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_ManifestHash();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_base_hash",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_BaseHash();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_guardian_share_challenge",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]

            public static extern IntPtr GetPrefix_GuardianShareChallenge();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_guardian_share_secret",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_GuardianShareSecret();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_extended_hash",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_ExtendedHash();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_selection_nonce",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_SelectionNonce();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_selection_proof",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_SelectionProof();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_contest_data_nonce",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_ContestDataNonce();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_contest_data_secret",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_ContestDataSecret();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_contest_proof",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_ContestProof();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_contest_hash",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_ContestHash();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_ballot_code",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_BallotCode();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_ballot_chain",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_BallotChain();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_decrypt_selection_proof",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_DecryptSelectionProof();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_decrypt_contest_data",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix_DescryptContestData();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_string",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                [MarshalAs(UnmanagedType.LPStr)] string a,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_strings",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] a,
                ulong length,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_int",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                ulong a,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_string_int",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                [MarshalAs(UnmanagedType.LPStr)] string a,
                ulong b,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_int_int",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                ulong a,
                ulong b,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_string_int_modp",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                [MarshalAs(UnmanagedType.LPStr)] string a,
                ulong b, NativeInterface.ElementModP.ElementModPHandle c,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_string_int_modq",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                [MarshalAs(UnmanagedType.LPStr)] string a,
                ulong b, NativeInterface.ElementModQ.ElementModQHandle c,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_modp_modp",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                NativeInterface.ElementModP.ElementModPHandle a,
                NativeInterface.ElementModP.ElementModPHandle b,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_modp_modq",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                NativeInterface.ElementModP.ElementModPHandle a,
                NativeInterface.ElementModQ.ElementModQHandle b,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_modq_modq",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                NativeInterface.ElementModQ.ElementModQHandle a,
                NativeInterface.ElementModQ.ElementModQHandle b,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_modq_int",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                NativeInterface.ElementModQ.ElementModQHandle a,
                ulong b,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_modq_modp",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                NativeInterface.ElementModQ.ElementModQHandle a,
                NativeInterface.ElementModP.ElementModPHandle b,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_array",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                // TODO: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] inData,
                ulong inDataSize,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );
        }
    }
}
