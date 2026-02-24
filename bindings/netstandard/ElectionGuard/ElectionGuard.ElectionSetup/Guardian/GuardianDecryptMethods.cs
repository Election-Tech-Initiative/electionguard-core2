using ElectionGuard.ElectionSetup.Records;

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

        if (_myPartialSecretKey is null)
        {
            _ = CombinePrivateKeyShares();
        }

        return ciphertext.PartialDecrypt(_myPartialSecretKey);
    }

    public ElementModP PartialDecrypt(HashedElGamalCiphertext ciphertext)
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

        if (_myPartialSecretKey is null)
        {
            _ = CombinePrivateKeyShares();
        }

        return ciphertext.PartialDecrypt(_myPartialSecretKey);
    }

    // P(i) = ∑ P_j(i) = (P1(i)+P2(i)+···+Pn(i)) mod q.
    private ElementModQ CombinePrivateKeyShares()
    {
        var partialSecretKey = Constants.ZERO_MOD_Q;
        foreach (var item in _partialKeyBackups!.Values)
        {
            var decryptedKey = DecryptBackup(item);
            partialSecretKey = BigMath.AddModQ(decryptedKey, partialSecretKey);
        }
        return _myPartialSecretKey = partialSecretKey;
    }

    /// <summary>
    /// Decrypts a compensated partial decryption of an elgamal encryption on behalf of a missing guardian
    /// </summary>
    /// <param name="guardianBackup">Missing guardian's backup</param>
    /// <returns>coordinatedata of the decryption and its proof</returns>
    private ElementModQ? DecryptBackup(ElectionPartialKeyBackupRecord backup)
    {
        return DecryptBackup(backup, _myElectionKeys);
    }

    /// <summary>
    /// Decrypts a compensated partial decryption of an elgamal encryption on behalf of a missing guardian
    /// </summary>
    /// <param name="guardianBackup">Missing guardian's backup</param>
    /// <param name="myKeyPair">The present guardian's key pair that will be used to decrypt the backup</param>
    /// <returns>coordinatedata of the decryption and its proof</returns>
    private static ElementModQ? DecryptBackup(
        ElectionPartialKeyBackupRecord guardianBackup, ElectionKeyPair myKeyPair)
    {
        var encryptionSeed = GetBackupSeed(
            myKeyPair.SequenceOrder,
            guardianBackup.OwnerSequenceOrder
        );

        var decryptedCoordinate = guardianBackup.EncryptedCoordinate.Decrypt(
            myKeyPair.KeyPair.PublicKey,
            myKeyPair.KeyPair.SecretKey,
            Hash.Prefix_GuardianShareSecret, encryptionSeed,
            lookForPadding: false);

        if (decryptedCoordinate is null)
        {
            return null;
        }

        var coordinateData = new ElementModQ(decryptedCoordinate);
        return coordinateData;
    }
}
