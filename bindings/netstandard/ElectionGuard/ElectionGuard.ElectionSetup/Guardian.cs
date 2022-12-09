using System.Collections.Generic;
using System.Threading.Tasks;
using ElectionGuard.ElectionSetup.Extensions;
using GuardianId = System.String;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// Guardian of election responsible for safeguarding information and decrypting results.
///
/// The first half of the guardian involves the key exchange known as the key ceremony.
/// The second half relates to the decryption process.
/// </summary>
public class Guardian
{
    private readonly ElectionKeyPair _electionKeys;
    private readonly CeremonyDetails _ceremonyDetails;
    private readonly Dictionary<string, ElectionPublicKey> _otherGuardianPublicKeys;
    private readonly Dictionary<string, ElectionPartialKeyBackup> _otherGuardianPartialKeyBackups;

    public string GuardianId { get; set; }
    public Dictionary<int, ElectionPartialKeyBackup> BackupToShare { get; set; } = new();

    /// <summary>
    /// Initialize a guardian with the specified arguments.
    /// </summary>
    /// <param name="keyPair">The key pair the guardian generated during a key ceremony</param>
    /// <param name="ceremonyDetails">The details of the key ceremony</param>
    /// <param name="otherGuardianPublicKeys">The public keys the guardian generated during a key ceremony</param>
    /// <param name="otherGuardianPartialKeyBackups">The partial key backups the guardian generated during a key ceremony</param>
    /// <param name="partialKeyBackup"></param>
    /// <param name="guardianElectionPartialKeyVerifications"></param>
    public Guardian(
        ElectionKeyPair keyPair,
        CeremonyDetails ceremonyDetails,
        Dictionary<GuardianId, ElectionPublicKey> otherGuardianPublicKeys = null,
        Dictionary<GuardianId, ElectionPartialKeyBackup> otherGuardianPartialKeyBackups = null,
        Dictionary<int, ElectionPartialKeyBackup> partialKeyBackup = null,

        Dictionary<GuardianId, ElectionPartialKeyVerification> guardianElectionPartialKeyVerifications = null
        )
    {
        _electionKeys = keyPair;
        _ceremonyDetails = ceremonyDetails;
        _otherGuardianPublicKeys = otherGuardianPublicKeys;
        _otherGuardianPartialKeyBackups = otherGuardianPartialKeyBackups;

        if (partialKeyBackup != null)
        {
            BackupToShare = partialKeyBackup;
        }

        GenerateBackupKeys();
    }

    private void GenerateBackupKeys()
    {
        Parallel.For(0, _ceremonyDetails.numberOfGuardians, (i) =>
        {
            if (i == _electionKeys.SequenceOrder)
            {
                // Don't generate a backup for your own key.
                return;
            }

            BackupToShare.Add(i, GenerateBackupKey((ulong)i));
        });
    }

    /// <summary>
    /// Share a backup key 
    /// </summary>
    /// <param name="sequenceOrder"></param>
    /// <returns></returns>
    public ElectionPartialKeyBackup Share(int sequenceOrder) => BackupToShare[sequenceOrder];

    public static Guardian FromNonce(
        string guardianId,
        int sequenceOrder,
        int numberOfGuardians,
        int quorum,
        ElementModQ nonce = null)
    {
        var keyPair = ElectionKeyPair.GenerateElectionKeyPair(guardianId, sequenceOrder, quorum, nonce);
        var ceremonyDetails = new CeremonyDetails(numberOfGuardians, quorum);
        return new(keyPair, ceremonyDetails);
    }

    private ElectionPartialKeyBackup GenerateBackupKey(ulong sequenceOrder)
    {
        var coordinate = ComputePolynomialCoordinate(sequenceOrder);


        //     coordinate_data = CoordinateData(coordinate)
        //     nonce = rand_q()
        //     seed = get_backup_seed(
        //         receiver_guardian_public_key.owner_id,
        //         receiver_guardian_public_key.sequence_order,
        //     )
        //     encrypted_coordinate = hashed_elgamal_encrypt(
        //         coordinate_data.to_bytes(),
        //         nonce,
        //         receiver_guardian_public_key.key,
        //         seed,
        //     )
        //     return ElectionPartialKeyBackup(
        //         sender_guardian_id,
        //         receiver_guardian_public_key.owner_id,
        //         receiver_guardian_public_key.sequence_order,
        //         encrypted_coordinate,
        //     )
        return new();
    }

    private ElementModQ ComputePolynomialCoordinate(ulong sequenceOrder)
    {
        var sequenceOrderModQ = new ElementModQ(sequenceOrder);
        var coordinate = Constants.ZERO_MOD_Q; // start at 0 mod q.

        foreach (var (coefficient, index) in _electionKeys.Polynomial.Coefficients.WithIndex())
        {
            coordinate = GetCoordinate(sequenceOrderModQ, coordinate, coefficient, index);
        }

        return coordinate;
    }

    // TODO: This is a candidate for a Math class... or a Polynomial class. This does not belong to a guardian.
    private static ElementModQ GetCoordinate(
        ElementModQ initialState,
        ElementModQ baseElement,
        Coefficient coefficient,
                    int index)
    {
        var exponent = new ElementModQ((ulong)index);
        var factor = BigMath.PowModQ(baseElement, exponent);
        var coordinateShift = BigMath.MultModQ(coefficient.Value, factor);

        return BigMath.AddModQ(initialState, coordinateShift);
    }

    public CeremonyDetails CeremonyDetails { get; set; }

    public ElectionPublicKey ShareKey()
    {
        return _electionKeys.Share();
    }
}
