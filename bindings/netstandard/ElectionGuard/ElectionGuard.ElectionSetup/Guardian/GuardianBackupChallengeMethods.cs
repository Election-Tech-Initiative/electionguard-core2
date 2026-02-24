using ElectionGuard.ElectionSetup.Records;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// methods for challenging a guardian's backup commitment
/// </summary>
public partial class Guardian
{
    // publish_election_backup_challenge
    public ElectionPartialKeyChallenge? PublishElectionBackupChallenge(
        string guardianId)
    {
        _ = BackupsToShare.TryGetValue(guardianId, out var backup);
        // TODO: throw exception instead of returning null
        if (backup is null)
            return null;
        return GenerateElectionPartialKeyChallenge(backup, _myElectionKeys.Polynomial);
    }

    // verify_election_partial_key_challenge
    public ElectionPartialKeyVerificationRecord VerifyElectionPartialKeyChallenge(
        ElectionPartialKeyChallenge challenge)
    {
        return VerifyElectionPartialKeyChallenge(GuardianId, challenge);
    }

    private static ElectionPartialKeyChallenge GenerateElectionPartialKeyChallenge(
        ElectionPartialKeyBackupRecord backup, ElectionPolynomial polynomial)
    {
        return new()
        {
            OwnerId = backup.OwnerId,
            DesignatedId = backup.DesignatedId,
            DesignatedSequenceOrder = backup.DesignatedSequenceOrder,
            Value = polynomial.ComputeCoordinate(backup.DesignatedSequenceOrder),
            CoefficientCommitments = polynomial.Commitments,
            CoefficientProofs = polynomial.Proofs
        };
    }

    private static ElectionPartialKeyVerificationRecord VerifyElectionPartialKeyChallenge(
        string verifierId, ElectionPartialKeyChallenge challenge)
    {
        return new ElectionPartialKeyVerificationRecord(null, challenge.OwnerId, challenge.DesignatedId, verifierId, ElectionPolynomial.VerifyCoordinate(
                challenge.DesignatedSequenceOrder,
                challenge.Value!,
                challenge.CoefficientCommitments!));
        
    }
}
