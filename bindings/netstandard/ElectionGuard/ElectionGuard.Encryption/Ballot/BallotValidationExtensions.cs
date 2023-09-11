using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElectionGuard.Base;

namespace ElectionGuard.Ballot
{
    /// <summary>
    /// The result of a ballot validation operation.
    /// </summary>
    public class BallotValidationResult : IValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }

        public List<BallotValidationResult> Children { get; set; }

        List<IValidationResult> IValidationResult.Children => Children.Select(x => (IValidationResult)x).ToList();

        public BallotValidationResult(
            bool isValid,
            List<BallotValidationResult> children = null)
        {
            IsValid = isValid;
            Message = string.Empty;
            Children = children ?? new List<BallotValidationResult>();
        }

        public BallotValidationResult(
            string message,
            List<BallotValidationResult> children = null)
        {
            IsValid = false;
            Message = message;
            Children = children ?? new List<BallotValidationResult>();
        }

        public BallotValidationResult(
            bool isValid,
            string message,
            List<BallotValidationResult> children = null)
        {
            IsValid = isValid;
            Message = message;
            Children = children ?? new List<BallotValidationResult>();
        }

        public override string ToString()
        {
            if (IsValid)
            {
                return "true";
            }

            var sb = new StringBuilder();
            _ = sb.AppendLine($"failed: {Message}");
            foreach (var child in Children)
            {
                _ = sb.AppendLine($"{child}");
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// extension class for ballot validation
    /// </summary>
    public static class BallotValidationExtensions
    {
        /// <summary>
        /// Validate the selection against the description.
        /// </summary>
        public static BallotValidationResult IsValid(
            this CiphertextBallotSelection selection,
            SelectionDescription description,
             bool isPlaceholder = false)
        {
            return selection.ObjectId != description.ObjectId
                ? new BallotValidationResult(
                    $"Object Ids do not match for selection {selection.ObjectId} description {description.ObjectId}")
                : selection.DescriptionHash != description.CryptoHash()
                ? new BallotValidationResult(
                    $"Description hashes do not match for selection {selection.ObjectId}")
                : selection.IsPlaceholder != isPlaceholder
                ? new BallotValidationResult(
                    $"IsPlaceholder does not match for selection {selection.ObjectId}")
                : new BallotValidationResult(true);
        }

        /// <summary>
        /// Check to see if the contest is valid based on the contest description to use
        /// </summary>
        /// <param name="contest">Contest to check</param>
        /// <param name="description">Description to compare with</param>
        public static BallotValidationResult IsValid(
            this CiphertextBallotContest contest,
            ContestDescriptionWithPlaceholders description)
        {
            // verify the object id matches
            if (contest.ObjectId != description.ObjectId)
            {
                return new BallotValidationResult(
                    $"Object Ids do not match for contest {contest.ObjectId} description {description.ObjectId}");
            }

            // verify the description hash matches
            if (contest.DescriptionHash != description.CryptoHash())
            {
                return new BallotValidationResult(
                    $"Description hashes do not match for contest {contest.ObjectId}");
            }

            if (contest.Selections.Count !=
                (int)description.SelectionsSize)
            {
                return new BallotValidationResult(
                    $"Selection counts do not match for contest {contest.ObjectId}");
            }

            // verify the selections are valid
            var results = new List<BallotValidationResult>();
            foreach (var selectionDescription in description.Selections)
            {
                // validate there's a selection description for the selection id
                var selection = contest.Selections
                    .FirstOrDefault(i => i.ObjectId == selectionDescription.ObjectId);
                if (selection == null)
                {
                    results.Add(new BallotValidationResult(
                        $"Selection {selectionDescription.ObjectId} not found in contest {contest.ObjectId}"
                    ));
                }
                else
                {
                    // validate the selection is valid
                    var result = selection.IsValid(selectionDescription);
                    if (!result.IsValid)
                    {
                        results.Add(result);
                    }
                }
            }


            return new BallotValidationResult(
                results.Count == 0 || results.All(i => i.IsValid), results);
        }

        /// <summary>
        /// Validate the ballot against the manifest.
        /// </summary>
        public static BallotValidationResult IsValid(
            this CiphertextBallot ballot, InternalManifest manifest)
        {
            if (ballot.ManifestHash != manifest.ManifestHash)
            {
                return new BallotValidationResult(
                    $"Manifest hashes do not match for ballot {ballot.ObjectId}"
                );
            }

            var descriptions = manifest.GetContests(ballot.StyleId);
            if (descriptions == null)
            {
                return new BallotValidationResult(
                    $"Style {ballot.StyleId} not found in manifest"
                );
            }

            // verify the contests are valid
            var results = new List<BallotValidationResult>();
            foreach (var contestDescription in descriptions)
            {
                // validate there's a contest description for the contest id
                var contest = ballot.Contests
                    .FirstOrDefault(i => i.ObjectId == contestDescription.ObjectId);
                if (contest == null)
                {
                    results.Add(new BallotValidationResult(
                        $"Contest {contestDescription.ObjectId} not found in ballot {ballot.ObjectId}"
                    ));
                }
                else
                {
                    // validate the contest is valid
                    var result = contest.IsValid(contestDescription);
                    if (!result.IsValid)
                    {
                        results.Add(result);
                    }
                }
            }

            return new BallotValidationResult(
                results.Count == 0 || results.All(i => i.IsValid), results);
        }

        /// <summary>
        /// Validate the ballot against the manifest and the ciphertext election context.
        /// </summary>
        public static BallotValidationResult IsValid(
            this CiphertextBallot ballot, InternalManifest manifest, CiphertextElectionContext context)
        {
            var isValid = ballot.IsValid(manifest);
            if (ballot.ManifestHash != context.ManifestHash)
            {
                isValid.Children.Add(new BallotValidationResult(
                    $"Manifest hashes do not match for ballot {ballot.ObjectId}"
                ));
                isValid.IsValid = false;
            }

            // verify the ballot is valid encryption
            var isValidEncryption = ballot.IsValidEncryption(
                manifest.ManifestHash, context.ElGamalPublicKey, context.CryptoExtendedBaseHash);
            if (!isValidEncryption)
            {
                isValid.Children.Add(new BallotValidationResult(
                    $"Ballot {ballot.ObjectId} is not valid encryption"
                ));
                isValid.IsValid = false;
            }

            return isValid;
        }
    }
}
