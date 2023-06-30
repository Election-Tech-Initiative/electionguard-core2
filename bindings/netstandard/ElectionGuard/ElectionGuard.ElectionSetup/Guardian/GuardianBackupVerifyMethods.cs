using System.Diagnostics;
using ElectionGuard.Guardians;
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
            if (_partialVerifications?.Count != required)
            {
                return false;
            }

            foreach (var verification in _partialVerifications.Values)
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
        _partialVerifications[verification.DesignatedId!] = verification;
    }

    // verify_election_partial_key_backup
    public ElectionPartialKeyVerification? VerifyElectionPartialKeyBackup(
        string guardianId, string keyCeremonyId)
    {
        var backup = _partialKeyBackups[guardianId];
        var publicKey = _publicKeys[guardianId];

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
            backup?.DesignatedId!, backup!, publicKey, _myElectionKeys, keyCeremonyId);
    }

    private static ElectionPartialKeyVerification VerifyElectionPartialKeyBackup(
        string receiverGuardianId,
        ElectionPartialKeyBackup senderGuardianBackup,
        ElectionPublicKey senderGuardianPublicKey,
        ElectionKeyPair myKeyPair, string keyCeremonyId)
    {
        using var coordinateData = DecryptBackup(senderGuardianBackup, myKeyPair);
        if (coordinateData is null)
        {
            Debug.WriteLine($"VerifyElectionPartialKeyBackup: {receiverGuardianId} -> {senderGuardianBackup.OwnerId} {senderGuardianBackup.DesignatedSequenceOrder} failed to decrypt");
            return new()
            {
                KeyCeremonyId = keyCeremonyId,
                OwnerId = senderGuardianBackup.OwnerId,
                DesignatedId = senderGuardianBackup.DesignatedId,
                VerifierId = receiverGuardianId,
                Verified = false
            };
        }

        var verified = ElectionPolynomial.VerifyCoordinate(
                senderGuardianBackup.DesignatedSequenceOrder,
                coordinateData,
                senderGuardianPublicKey.CoefficientCommitments
            );
        Debug.WriteLine($"VerifyElectionPartialKeyBackup: {receiverGuardianId} -> {senderGuardianBackup.OwnerId} {senderGuardianBackup.DesignatedSequenceOrder} {verified}");
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
