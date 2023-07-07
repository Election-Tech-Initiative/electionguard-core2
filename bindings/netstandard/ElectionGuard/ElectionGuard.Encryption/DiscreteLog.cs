namespace ElectionGuard
{
    /// <summary>
    /// A discrete log is a value tally `t` such that `g^t mod p = M` for some `g` and `M`.
    /// </summary>
    public static class DiscreteLog
    {
        /// <Summary>
        /// Get the discrete log value for the given element.
        /// This override assumes that the generator G() is being used as the base for exponentiations.
        ///
        /// In Electionguard 2.0 this method is deprecated in favor of the override that
        /// allows for the caller to specify the base for exponentiations.
        /// </Summary>
        public static ulong GetAsync(ElementModP element)
        {
            ulong result = 0;
            var status = NativeInterface.DiscreteLog.GetAsync(element.Handle, ref result);
            status.ThrowIfError();
            return result;
        }

        /// <Summary>
        /// Get the discrete log value for the given element using the specified base
        /// This override allows for the caller to specify the base for exponentiations.
        /// This is useful for cases where the caller is using a different generator than G()
        /// or when encrypting ballots using the base-K ElGamal method
        ///
        /// Since this class is a singleton, the base is cached and the cache is cleared
        /// when the base changes.
        /// </Summary>
        public static ulong GetAsync(ElementModP element, ElementModP encryptionBase)
        {
            ulong result = 0;
            var status = NativeInterface.DiscreteLog.GetAsync(
                element.Handle, encryptionBase.Handle, ref result);
            status.ThrowIfError();
            return result;
        }
    }
}
