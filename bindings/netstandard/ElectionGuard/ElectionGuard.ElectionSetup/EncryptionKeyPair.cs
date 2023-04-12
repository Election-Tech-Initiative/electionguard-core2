using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.ElectionSetup;


public class EncryptionKeyPair : DisposableBase
{
    public ElementModP PublicKey { get; set; }

    public ElementModQ SecretKey { get; set; }

    public EncryptionKeyPair()
    {
        PublicKey = new ElementModP(0);
        SecretKey = new ElementModQ(0);
    }
    public EncryptionKeyPair(ElementModQ secretKey, ElementModP publicKey)
    {
        PublicKey = new(publicKey);
        SecretKey = new(secretKey);
    }

    public static implicit operator ElGamalKeyPair(EncryptionKeyPair data)
    {
        return ElGamalKeyPair.FromPair(data.SecretKey, data.PublicKey);
    }
    public static implicit operator EncryptionKeyPair(ElGamalKeyPair data)
    {
        return new ElGamalKeyPair(data.SecretKey, data.PublicKey);
    }
    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        PublicKey.Dispose();
        SecretKey.Dispose();
    }
}
