using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// methods for decrypting
/// </summary>
public partial class Guardian
{
    public ElementModP PartialDecrypt(ElGamalCiphertext ciphertext)
    {
        if (AllGuardianKeysReceived is false)
        {
            throw new InvalidOperationException("All guardian keys must be received before decrypting.");
        }

        // TODO: should we verify?
        // if (AllElectionPartialKeyBackupsVerified() is false)
        // {
        //     throw new InvalidOperationException("All election partial key backups must be verified before decrypting.");
        // }

        if (_partialElectionSecretKey is null)
        {
            _ = CombinePrivateKeyShares();
        }

        return ciphertext.PartialDecrypt(_partialElectionSecretKey);
    }

    // P(i) = ∑ P_j(i) = (P1(i)+P2(i)+···+Pn(i)) mod q.
    private ElementModQ CombinePrivateKeyShares()
    {
        var partialSecretKey = Constants.ZERO_MOD_Q;
        foreach (var item in _otherGuardianPartialKeyBackups!.Values)
        {
            var decryptedKey = DecryptBackup(item);
            partialSecretKey = BigMath.AddModQ(decryptedKey, partialSecretKey);
        }
        return _partialElectionSecretKey = partialSecretKey;
    }

    /// <summary>
    /// Decrypts a compensated partial decryption of an elgamal encryption on behalf of a missing guardian
    /// </summary>
    /// <param name="guardianBackup">Missing guardian's backup</param>
    /// <returns>coordinatedata of the decryption and its proof</returns>
    private ElementModQ? DecryptBackup(ElectionPartialKeyBackup backup)
    {
        return DecryptBackup(backup, _electionKeys);
    }

    /// <summary>
    /// Decrypts a compensated partial decryption of an elgamal encryption on behalf of a missing guardian
    /// </summary>
    /// <param name="guardianBackup">Missing guardian's backup</param>
    /// <param name="keyPair">The present guardian's key pair that will be used to decrypt the backup</param>
    /// <returns>coordinatedata of the decryption and its proof</returns>
    private static ElementModQ? DecryptBackup(ElectionPartialKeyBackup guardianBackup, ElectionKeyPair keyPair)
    {
        var encryptionSeed = GetBackupSeed(
            keyPair.OwnerId,
            keyPair.SequenceOrder
        );

        var bytesOptional = guardianBackup.EncryptedCoordinate?.Decrypt(
            keyPair.KeyPair.SecretKey, encryptionSeed, false);

        if (bytesOptional is null)
        {
            return null;
        }

        var coordinateData = new ElementModQ(bytesOptional);
        return coordinateData;
    }
}
