using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// methods for verifying a guardian's backup commitment
/// </summary>
public partial class Guardian
{
    /// <summary>
    /// Returns true if all election partial key backups have been verified.
    /// </summary>
    public bool AllElectionPartialKeyBackupsVerified
    {
        get
        {
            var required = CeremonyDetails.NumberOfGuardians - 1;
            if (_otherGuardianPartialKeyVerification?.Count != required)
            {
                return false;
            }

            foreach (var verification in _otherGuardianPartialKeyVerification.Values)
            {
                if (verification.Verified is false)
                {
                    return false;
                }
            }
            return true;
        }
    }

    // save_election_partial_key_verification
    public void SaveElectionPartialKeyVerification(ElectionPartialKeyVerification verification)
    {
        _otherGuardianPartialKeyVerification ??= new();
        _otherGuardianPartialKeyVerification[verification.DesignatedId!] = verification;
    }

    // verify_election_partial_key_backup
    public ElectionPartialKeyVerification? VerifyElectionPartialKeyBackup(
        string guardianId, string keyCeremonyId)
    {
        var backup = _otherGuardianPartialKeyBackups?[guardianId];
        var publicKey = _otherGuardianPublicKeys?[guardianId];

        // TODO: throw exception instead of returning null
        if (backup is null)
        {
            return null;
        }
        if (publicKey is null)
        {
            return null;
        }
        return VerifyElectionPartialKeyBackup(
            backup?.DesignatedId!, backup, publicKey, _electionKeys, keyCeremonyId);
    }

    private static ElectionPartialKeyVerification VerifyElectionPartialKeyBackup(
        string receiverGuardianId,
        ElectionPartialKeyBackup? senderGuardianBackup,
        ElectionPublicKey? senderGuardianPublicKey,
        ElectionKeyPair electionKeys, string keyCeremonyId)
    {
        using var encryptionSeed = GetBackupSeed(
                receiverGuardianId,
                senderGuardianBackup?.DesignatedSequenceOrder
            );

        var secretKey = electionKeys.KeyPair.SecretKey;
        var data = senderGuardianBackup?.EncryptedCoordinate?.Decrypt(
                secretKey, encryptionSeed, false);

        using var coordinateData = new ElementModQ(data);

        var verified = ElectionPolynomial.VerifyCoordinate(
                senderGuardianBackup!.DesignatedSequenceOrder,
                coordinateData,
                senderGuardianPublicKey!.CoefficientCommitments
            );
        return new()
        {
            KeyCeremonyId = keyCeremonyId,
            OwnerId = senderGuardianBackup.OwnerId,
            DesignatedId = senderGuardianBackup.DesignatedId,
            VerifierId = receiverGuardianId,
            Verified = verified
        };
    }
}