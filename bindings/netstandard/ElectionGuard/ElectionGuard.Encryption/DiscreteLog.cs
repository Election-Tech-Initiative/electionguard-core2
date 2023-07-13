namespace ElectionGuard
{
    /// <summary>
    /// A discrete log is a value tally `t` such that `g^t mod p = M` for some `g` and `M`.
    /// </summary>
    public static class DiscreteLog
    {
        /// <summary>
        /// Compute the discrete log of `element` with respect to `generator`.
        /// </summary>
        public static ulong GetAsync(ElementModP element)
        {
            ulong result = 0;
            var status = NativeInterface.DiscreteLog.GetAsync(element.Handle, ref result);
            status.ThrowIfError();
            return result;
        }
    }
}
