using System;

namespace ElectionGuard.Base
{
    /// <summary>
    /// A base interface for types that can be hashed
    /// </summary>
    public interface ICryptoHashableType
    {

    }

    /// <summary>
    /// A base class for types that can be hashed
    /// </summary>
    public abstract class CryptoHashableBase : DisposableBase, ICryptoHashableType
    {
        internal abstract IntPtr Ptr { get; }
    }

}
