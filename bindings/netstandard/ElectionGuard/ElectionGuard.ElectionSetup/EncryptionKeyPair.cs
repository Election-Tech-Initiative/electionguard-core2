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

    public EncryptionKeyPair(EncryptionKeyPair keyPair)
    {
        PublicKey = new(keyPair.PublicKey);
        SecretKey = new(keyPair.SecretKey);
    }

    public EncryptionKeyPair(ElementModQ secretKey, ElementModP publicKey)
    {
        PublicKey = new(publicKey);
        SecretKey = new(secretKey);
    }

    public EncryptionKeyPair(ElGamalKeyPair keyPair)
    {
        PublicKey = new(keyPair.PublicKey);
        SecretKey = new(keyPair.SecretKey);
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        PublicKey.Dispose();
        SecretKey.Dispose();
    }
}
