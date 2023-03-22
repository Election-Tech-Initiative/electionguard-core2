using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectionGuard.Encryption.Ballot
{
    public struct BallotValidationResult
    {
        public bool IsValid;
        public string Message;

        public List<BallotValidationResult> Children;

        public BallotValidationResult(
            string message = null,
            List<BallotValidationResult> children = null)
        {
            IsValid = false;
            Message = message;
            Children = children ?? new List<BallotValidationResult>();
        }

        public BallotValidationResult(
            bool isValid = true,
            string message = null,
            List<BallotValidationResult> children = null)
        {
            IsValid = isValid;
            Message = message;
            Children = children ?? new List<BallotValidationResult>();
        }

        public static implicit operator BallotValidationResult(bool isValid)
        {
            return new BallotValidationResult() { IsValid = isValid };
        }

        public static implicit operator bool(BallotValidationResult result)
        {
            return result.IsValid;
        }

        public override string ToString()
        {
            if (IsValid)
            {
                return "true";
            }

            var sb = new StringBuilder();
            _ = sb.AppendLine($"false: {Message}");
            foreach (var child in Children)
            {
                _ = sb.AppendLine(child.ToString());
            }
            return sb.ToString();
        }
    }
    public static class BallotValidationExtensions
    {
        /// <summary>
        /// Validate the selection against the description.
        /// </summary>
        public static BallotValidationResult IsValid(
            this CiphertextBallotSelection selection, SelectionDescription description)
        {
            return selection.ObjectId != description.ObjectId
                ? new BallotValidationResult()
                {
                    IsValid = false,
                    Message = $"Object Ids do not match for selection {selection.ObjectId} description {description.ObjectId}"
                }
                : selection.DescriptionHash != description.CryptoHash()
                ? new BallotValidationResult()
                {
                    IsValid = false,
                    Message = $"Description hashes do not match for selection {selection.ObjectId}"
                }
                : new BallotValidationResult() { IsValid = true };
        }

        public static BallotValidationResult IsValid(
            this CiphertextBallotContest contest,
            ContestDescriptionWithPlaceholders description)
        {
            // verify the object id matches
            if (contest.ObjectId != description.ObjectId)
            {
                return new BallotValidationResult()
                {
                    IsValid = false,
                    Message = $"Object Ids do not match for contest {contest.ObjectId} description {description.ObjectId}"
                };
            }

            // verify the description hash matches
            if (contest.DescriptionHash != description.CryptoHash())
            {
                return new BallotValidationResult()
                {
                    IsValid = false,
                    Message = $"Description hashes do not match for contest {contest.ObjectId}"
                };
            }

            if (contest.Selections.Count() !=
                (int)description.SelectionsSize + (int)description.PlaceholdersSize)
            {
                return new BallotValidationResult()
                {
                    IsValid = false,
                    Message = $"Selection counts do not match for contest {contest.ObjectId}"
                };
            }

            // verify the selections are valid
            var results = new List<BallotValidationResult>();
            foreach (var selectionDescription in description.Selections)
            {
                // validate there's a selection description for the selection id
                var selection = contest.Selections
                    .FirstOrDefault(i => i.ObjectId == description.ObjectId);
                if (selection == null)
                {
                    results.Add(new BallotValidationResult()
                    {
                        IsValid = false,
                        Message = $"Selection {description.ObjectId} not found in contest {contest.ObjectId}"
                    });
                }

                // validate the selection is valid
                var result = selection.IsValid(selectionDescription);
                if (!result.IsValid)
                {
                    results.Add(result);
                }
            }

            return new BallotValidationResult() { IsValid = !results.Any(), Children = results };
        }

        /// <summary>
        /// Validate the ballot against the manifest.
        /// </summary>
        public static BallotValidationResult IsValid(
            this CiphertextBallot ballot, InternalManifest manifest)
        {
            if (ballot.ManifestHash != manifest.ManifestHash)
            {
                return new BallotValidationResult()
                {
                    IsValid = false,
                    Message = $"Manifest hashes do not match for ballot {ballot.ObjectId}"
                };
            }

            var descriptions = manifest.GetContests(ballot.StyleId);
            if (descriptions == null)
            {
                return new BallotValidationResult()
                {
                    IsValid = false,
                    Message = $"Style {ballot.StyleId} not found in manifest"
                };
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
                    results.Add(new BallotValidationResult()
                    {
                        IsValid = false,
                        Message = $"Contest {contestDescription.ObjectId} not found in ballot {ballot.ObjectId}"
                    });
                }

                // validate the contest is valid
                var result = contest.IsValid(contestDescription);
                if (!result.IsValid)
                {
                    results.Add(result);
                }
            }

            return new BallotValidationResult() { IsValid = !results.Any(), Children = results };
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
                isValid.Children.Add(new BallotValidationResult()
                {
                    IsValid = false,
                    Message = $"Manifest hashes do not match for ballot {ballot.ObjectId}"
                });
                isValid.IsValid = false;
            }

            // verify the ballot is valid encryption
            var isValidEncryption = ballot.IsValidEncryption(
                manifest.ManifestHash, context.ElGamalPublicKey, context.CryptoExtendedBaseHash);
            if (!isValidEncryption)
            {
                isValid.Children.Add(new BallotValidationResult()
                {
                    IsValid = false,
                    Message = $"Ballot {ballot.ObjectId} is not valid encryption"
                });
                isValid.IsValid = false;
            }

            return isValid;
        }
    }
}