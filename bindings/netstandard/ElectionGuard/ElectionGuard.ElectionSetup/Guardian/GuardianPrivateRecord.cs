
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// A guardian is a person or entity that is responsible for decrypting a share of the election record.
/// The GuardianPrivateRecord is a record of the private information that a guardian needs to decrypt.
/// This record contains sensitive information and should be protected.
/// </summary>
public record GuardianPrivateRecord : DisposableRecordBase
{
    public string GuardianId { get; init; }

    public ElectionKeyPair ElectionKeys { get; init; }

    public Dictionary<string, ElectionPartialKeyBackup>? BackupsToShare { get; init; }

    public Dictionary<string, ElectionPublicKey>? GuardianElectionPublicKeys { get; init; }

    public Dictionary<string, ElectionPartialKeyBackup>? GuardianElectionPartialKeyBackups { get; init; }

    public Dictionary<string, ElectionPartialKeyVerification>? GuardianElectionPartialKeyVerifications { get; init; }

    public GuardianPrivateRecord(
        string guardianId,
        ElectionKeyPair electionKeys,
        Dictionary<string, ElectionPartialKeyBackup>? backupsToShare,
        Dictionary<string, ElectionPublicKey>? guardianElectionPublicKeys,
        Dictionary<string, ElectionPartialKeyBackup>? guardianElectionPartialKeyBackups,
        Dictionary<string, ElectionPartialKeyVerification>? guardianElectionPartialKeyVerifications)
    {
        GuardianId = guardianId;
        ElectionKeys = new(electionKeys);
        BackupsToShare = backupsToShare;
        GuardianElectionPublicKeys = guardianElectionPublicKeys ?? new();
        GuardianElectionPartialKeyBackups = guardianElectionPartialKeyBackups ?? new();
        GuardianElectionPartialKeyVerifications = guardianElectionPartialKeyVerifications ?? new();
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        ElectionKeys.Dispose();
        BackupsToShare?.Dispose();
        GuardianElectionPublicKeys?.Dispose();
        GuardianElectionPartialKeyBackups?.Dispose();
    }
}
