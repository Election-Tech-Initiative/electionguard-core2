using ElectionGuard.Decryption.Challenge;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.ElectionSetup;

namespace ElectionGuard.Decryption.ChallengeResponse;

// TODO: all of the iterator functions for al;l of these types should be consoldaited somewhere
// in fact we can probably define a single base class collection lookup
// that takes a generic and iterates over the collectiosn and does zips and comaprisons

/// <summary>
/// Decryption extension methods for the <see cref="Guardian" /> class
/// </summary>
public static class GuardianChallengeResponseExtensions
{

    public static GuardianChallengeResponse ComputeChallengeResponse(
        this Guardian guardian,
        GuardianShare share, GuardianChallenge challenge)
    {
        var tallyResponse = guardian.ComputeChallengeResponse(share.Tally, challenge.Tally);

        var ballotResponses = guardian.ComputeChallengeResponse(share.Ballots, challenge.Ballots);

        return new GuardianChallengeResponse(
            guardian,
            tallyResponse,
            ballotResponses!.Values.ToList());
    }

    // compute a challenge response for a specific ballot
    public static TallyChallengeResponse ComputeChallengeResponse(
        this Guardian guardian,
        TallyShare share, TallyChallenge challenge)
    {
        var response = new TallyChallengeResponse(challenge);

        foreach (var (contestId, contest) in share.Contests)
        {
            if (!challenge.Contests.ContainsKey(contestId))
            {
                throw new KeyNotFoundException(
                    $"Contest challenge does not contain selection {contestId}");
            }

            var contestChallenge = challenge.Contests[contestId];

            foreach (var (selectionId, selection) in contest.Selections)
            {
                if (!contestChallenge.Selections.ContainsKey(selectionId))
                {
                    throw new KeyNotFoundException(
                        $"Contest challenge does not contain selection {selectionId}");
                }

                var selectionChallenge = contestChallenge.Selections[selectionId];

                response.Add(
                    contest,
                    guardian.ComputeChallengeResponse(selection, selectionChallenge));
            }
        }
        return response;
    }

    public static Dictionary<string, BallotChallengeResponse> ComputeChallengeResponse(
        this Guardian guardian,
        List<BallotShare> shares, List<BallotChallenge> challenges)
    {
        var responses = new Dictionary<string, BallotChallengeResponse>();

        foreach (var share in shares)
        {
            var challenge = challenges.FirstOrDefault(x => x.ObjectId == share.BallotId)
                ?? throw new KeyNotFoundException(
                    $"Challenge not found for ballot {share.BallotId}");
            responses.Add(
                share.BallotId,
                guardian.ComputeChallengeResponse(share, challenge));
        }

        return responses;
    }


    // compute a challenge response for a specific ballot
    public static BallotChallengeResponse ComputeChallengeResponse(
        this Guardian guardian,
        BallotShare share, BallotChallenge challenge)
    {
        var response = new BallotChallengeResponse(challenge);

        foreach (var (contestId, contest) in share.Contests)
        {
            if (!challenge.Contests.ContainsKey(contestId))
            {
                throw new KeyNotFoundException(
                    $"Contest challenge does not contain selection {contestId}");
            }

            var contestChallenge = challenge.Contests[contestId];

            foreach (var (selectionId, selection) in contest.Selections)
            {
                if (!contestChallenge.Selections.ContainsKey(selectionId))
                {
                    throw new KeyNotFoundException(
                        $"Contest challenge does not contain selection {selectionId}");
                }

                var selectionChallenge = contestChallenge.Selections[selectionId];

                response.Add(
                    contest,
                    guardian.ComputeChallengeResponse(selection, selectionChallenge));
            }
        }
        return response;
    }

    // compute a challenge response for a specific selection
    public static SelectionChallengeResponse ComputeChallengeResponse(
        this Guardian guardian,
        SelectionShare share,
        SelectionChallenge challenge)
    {
        if (challenge.ObjectId != share.ObjectId)
        {
            throw new KeyNotFoundException(
                $"Selection challenge does not match selection {share.ObjectId}");
        }

        if (challenge.GuardianId != guardian.GuardianId)
        {
            throw new KeyNotFoundException(
                $"Selection challenge does not match guardian {guardian.GuardianId}");
        }

        if (share.GuardianId != guardian.GuardianId)
        {
            throw new KeyNotFoundException(
                $"Selection challenge does not match guardian {guardian.GuardianId}");
        }

        var response = guardian.CreateResponse(share, challenge.Challenge);
        return new SelectionChallengeResponse(
            guardian, challenge, response
        );
    }

    // dont think these are used since the admin can now producxe the proof
    // public static ChaumPedersenProof ComputeDecryptionShareProof(
    //     this Guardian guardian,
    //     SelectionShare share, SelectionChallenge challenge)
    // {
    //     return guardian.ComputeDecryptionShareProof(share, challenge.Challenge);
    // }

    // public static ChaumPedersenProof ComputeDecryptionShareProof(
    //     this Guardian guardian,
    //     SelectionShare share, ElementModQ challenge)
    // {
    //     var response = guardian.CreateResponse(share, challenge);
    //     var proof = new ChaumPedersenProof(
    //         share.Commitment,
    //         challenge,
    //         response
    //     );
    //     return proof;
    // }
}
