using ElectionGuard.Decryption.Challenge;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.ElectionSetup;

namespace ElectionGuard.Decryption.ChallengeResponse;

/// <summary>
/// Decryption extension methods for the <see cref="Guardian" /> class
/// </summary>
public static class GuardianChallengeResponseExtensions
{

    /// <summary>
    /// Compute a challenge response for a specific guardian challenge that was issued by an election administrator.
    /// </summary>
    public static GuardianChallengeResponse ComputeChallengeResponse(
        this Guardian guardian,
        GuardianShare share, GuardianChallenge challenge)
    {
        if (guardian.CommitmentOffset is null)
        {
            throw new AggregateException(
                $"Guardian {guardian.GuardianId} does not have a commitment offset");
        }
        var tallyResponse = guardian.ComputeChallengeResponse(share.Tally, challenge.Tally);
        var ballotResponses = guardian.ComputeChallengeResponse(share.Ballots, challenge.Ballots);

        return new GuardianChallengeResponse(
            guardian,
            guardian.CommitmentOffset,
            tallyResponse,
            ballotResponses!.Values.ToList());
    }

    /// <summary>
    /// Compute a challenge response for a specific tally challenge that was issued by an election administrator.
    /// </summary>
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

    /// <summary>
    /// Compute a challenge response for a specific collection of ballots that was issued by an election administrator.
    /// </summary>
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


    /// <summary>
    /// Compute a challenge response for a specific ballot.
    /// </summary>
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

    /// <summary>
    /// Compute a challenge response for a specific selection.
    /// </summary>
    public static SelectionChallengeResponse ComputeChallengeResponse(
        this Guardian guardian,
        SelectionShare share,
        SelectionChallenge challenge)
    {
        if (challenge.ObjectId != share.ObjectId)
        {
            throw new KeyNotFoundException(
                $"Selection challenge {challenge.ObjectId} does not match selection {share.ObjectId}");
        }

        if (challenge.GuardianId != guardian.GuardianId)
        {
            throw new KeyNotFoundException(
                $"Selection challenge {challenge.GuardianId} does not match guardian {guardian.GuardianId}");
        }

        if (share.GuardianId != guardian.GuardianId)
        {
            throw new KeyNotFoundException(
                $"Selection share {share.GuardianId} does not match guardian {guardian.GuardianId}");
        }

        var response = guardian.CreateResponse(share, challenge.Challenge);
        return new SelectionChallengeResponse(
            guardian, challenge, response
        );
    }
}
