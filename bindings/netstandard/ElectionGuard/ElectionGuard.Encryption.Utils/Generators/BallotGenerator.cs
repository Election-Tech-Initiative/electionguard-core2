using System;
using System.Collections.Generic;

namespace ElectionGuard.Encryption.Utils.Generators
{
    /// <summary>
    /// A class to generate ballots for testing
    /// </summary>
    public class BallotGenerator
    {
        // get a selection for a description using a new random number generator
        public static PlaintextBallotSelection SelectionFrom(
            SelectionDescription description,
            bool isPlaceholder = false)
        {
            return SelectionFrom(description, new Random(), isPlaceholder);
        }

        // get a selection for a description using a defined random number generator
        public static PlaintextBallotSelection SelectionFrom(
            SelectionDescription description,
            Random random,
            bool isPlaceholder = false)
        {
            var isAffirmative = random.Next(0, 2) == 1;
            return new PlaintextBallotSelection(
                description.ObjectId,
                isAffirmative ? 1UL : 0UL, isPlaceholder);
        }

        // get a selection for a description using a vote generator function
        public static PlaintextBallotSelection SelectionFrom(
            SelectionDescription description,
            Func<int, bool> voteGenerator,
            bool isPlaceholder = false)
        {
            var isAffirmative = voteGenerator(
                (int)description.SequenceOrder);
            return new PlaintextBallotSelection(
                description.ObjectId,
                isAffirmative ? 1UL : 0UL, isPlaceholder);
        }

        // get a selection for a description
        public static PlaintextBallotSelection SelectionFrom(
            SelectionDescription description,
            bool isAffirmative,
            bool isPlaceholder = false)
        {
            return new PlaintextBallotSelection(
                description.ObjectId,
                isAffirmative ? 1UL : 0UL, isPlaceholder);
        }

        // get a selection for a description using a new random number generator
        // this overload will not allow overvotes
        public static PlaintextBallotContest ContestFrom(
            ContestDescription contest)
        {
            return ContestFrom(contest, new Random());
        }

        // get a contest for a description using a defined random number generator
        // this overload will not allow overvotes
        public static PlaintextBallotContest ContestFrom(
            ContestDescription contest,
            Random random)
        {
            var selections = new List<PlaintextBallotSelection>();
            var selectionsMade = 0UL;

            foreach (var description in contest.Selections)
            {
                var selection = selectionsMade < contest.VotesAllowed
                    ? SelectionFrom(description, random)
                    : SelectionFrom(description, false, false);
                selectionsMade += selection.Vote;
                selections.Add(selection);
            }

            return new PlaintextBallotContest(contest.ObjectId, selections.ToArray());
        }

        // get a contest for a description using a list of votes
        public static PlaintextBallotContest ContestFrom(
            ContestDescription contest,
            IList<bool> votes)
        {
            var selections = new List<PlaintextBallotSelection>();
            for (var i = 0; i < contest.Selections.Count; i++)
            {
                selections.Add(SelectionFrom(contest.Selections[i], votes[i]));
            }

            return new PlaintextBallotContest(contest.ObjectId, selections.ToArray());
        }

        // get a contest for a description using a vote generator function
        // the generator function is invoked with the selection sequence order
        public static PlaintextBallotContest ContestFrom(
            ContestDescription contest,
            Func<int, bool> voteGenerator)
        {
            var selections = new List<PlaintextBallotSelection>();
            foreach (var description in contest.Selections)
            {
                selections.Add(SelectionFrom(description, voteGenerator));
            }

            return new PlaintextBallotContest(contest.ObjectId, selections.ToArray());
        }

        // get a contest for a description using a vote generator function
        // the generator function is invoked with the contest and selection sequence order
        public static PlaintextBallotContest ContestFrom(
            ContestDescription contest,
            Func<int, int, bool> voteGenerator)
        {
            var selections = new List<PlaintextBallotSelection>();

            foreach (var description in contest.Selections)
            {
                var isAffirmative = voteGenerator(
                    (int)contest.SequenceOrder, (int)description.SequenceOrder);
                selections.Add(SelectionFrom(description, isAffirmative));
            }

            return new PlaintextBallotContest(contest.ObjectId, selections.ToArray());
        }

        // get a selection for a description using a new random number generator
        // this overload will not allow overvotes
        public static PlaintextBallotContest ContestFrom(
            ContestDescriptionWithPlaceholders contest)
        {
            return ContestFrom(contest, new Random());
        }

        // get a contest for a description using a defined random number generator
        // this overload will not allow overvotes
        public static PlaintextBallotContest ContestFrom(
            ContestDescriptionWithPlaceholders contest,
            Random random)
        {
            var selections = new List<PlaintextBallotSelection>();
            var selectionsMade = 0UL;

            foreach (var description in contest.Selections)
            {
                var selection = selectionsMade < contest.VotesAllowed
                    ? SelectionFrom(description, random)
                    : SelectionFrom(description, false, false);
                selectionsMade += selection.Vote;
                selections.Add(selection);
            }

            return new PlaintextBallotContest(contest.ObjectId, selections.ToArray());
        }

        // get a contest for a description using a list of votes
        // will explicitly assign the vodes to the selections
        // so the number of votes must match the number of selections
        // explicitly allows overvotes and undervotes
        public static PlaintextBallotContest ContestFrom(
            ContestDescriptionWithPlaceholders contest,
            IList<bool> votes)
        {
            var selections = new List<PlaintextBallotSelection>();
            for (var i = 0; i < contest.Selections.Count; i++)
            {
                selections.Add(SelectionFrom(contest.Selections[i], votes[i]));
            }

            return new PlaintextBallotContest(contest.ObjectId, selections.ToArray());
        }

        // get a contest for a description using a vote generator function
        // the generator function is invoked with the selection sequence order
        public static PlaintextBallotContest ContestFrom(
            ContestDescriptionWithPlaceholders contest,
            Func<int, bool> voteGenerator)
        {
            var selections = new List<PlaintextBallotSelection>();
            foreach (var description in contest.Selections)
            {
                selections.Add(SelectionFrom(description, voteGenerator));
            }
            return new PlaintextBallotContest(contest.ObjectId, selections.ToArray());
        }

        // get a contest for a description using a vote generator function
        // the generator function is invoked with the contest and selection sequence order
        public static PlaintextBallotContest ContestFrom(
            ContestDescriptionWithPlaceholders contest,
            Func<int, int, bool> voteGenerator)
        {
            var selections = new List<PlaintextBallotSelection>();
            foreach (var description in contest.Selections)
            {
                var isAffirmative = voteGenerator(
                    (int)contest.SequenceOrder, (int)description.SequenceOrder);
                selections.Add(SelectionFrom(description, isAffirmative));
            }

            return new PlaintextBallotContest(contest.ObjectId, selections.ToArray());
        }

        // get a fake ballot using a new random number generator
        public static PlaintextBallot GetFakeBallot(
            InternalManifest manifest)
        {
            // get a random ballot style
            var random = new Random();
            var index = random.Next(manifest.BallotStyles.Count);
            var styleId = manifest.BallotStyles[index].ObjectId;
            return GetFakeBallot(
                manifest, styleId, random, $"fake-ballot-{Guid.NewGuid()}");
        }

        // get a fake ballot using a new random number generator
        public static PlaintextBallot GetFakeBallot(
            InternalManifest manifest,
            Random random)
        {
            // get a random ballot style
            var index = random.Next(manifest.BallotStyles.Count);
            var styleId = manifest.BallotStyles[index].ObjectId;
            return GetFakeBallot(
                manifest, styleId, random, $"fake-ballot-{Guid.NewGuid()}");
        }

        // get a fake ballot using a new random number generator
        public static PlaintextBallot GetFakeBallot(
            InternalManifest manifest,
            string styleId)
        {
            return GetFakeBallot(
                manifest, styleId, new Random(), $"fake-ballot-{Guid.NewGuid()}");
        }

        public static PlaintextBallot GetFakeBallot(
            InternalManifest manifest,
            string styleId,
            Random random)
        {
            return GetFakeBallot(
                manifest, styleId, random, $"fake-ballot-{Guid.NewGuid()}");
        }

        // get a fake ballot using a defined random number generator
        public static PlaintextBallot GetFakeBallot(
            InternalManifest manifest,
            Random random,
            string ballotId)
        {
            var index = random.Next(manifest.BallotStyles.Count);
            var styleId = manifest.BallotStyles[index].ObjectId;
            return GetFakeBallot(manifest, styleId, random, ballotId);
        }

        // get a fake ballot using a defined random number generator
        public static PlaintextBallot GetFakeBallot(
            InternalManifest manifest,
            string styleId,
            Random random,
            string ballotId)
        {
            var contests = new List<PlaintextBallotContest>();
            foreach (var contest in manifest.GetContests(styleId))
            {
                contests.Add(ContestFrom(contest, random));
            }

            return new PlaintextBallot(ballotId, styleId, contests.ToArray());
        }

        // get a fake ballot using a list of votes
        // the list of votes is expected to be in the same order as the contests
        // and the selections in the contests, including placeholders (if any)
        public static PlaintextBallot GetFakeBallot(
            InternalManifest manifest,
            string styleId,
            IList<IList<bool>> votes)
        {
            return GetFakeBallot(
                manifest, styleId, $"fake-ballot-{Guid.NewGuid()}", votes);
        }

        // get a fake ballot using a list of votes
        // the list of votes is expected to be in the same order as the contests
        // and the selections in the contests, including placeholders (if any)
        public static PlaintextBallot GetFakeBallot(
            InternalManifest manifest,
            string styleId,
            string ballotId,
            IList<IList<bool>> votes)
        {
            var contests = new List<PlaintextBallotContest>();
            var descriptions = manifest.GetContests(styleId);
            for (var i = 0; i < descriptions.Count; i++)
            {
                contests.Add(ContestFrom(descriptions[i], votes[i]));
            }

            return new PlaintextBallot(ballotId, styleId, contests.ToArray());
        }

        // get a fake ballot using an external function to determine the vote result
        // the function expects the contest id and the selection sequence order
        public static PlaintextBallot GetFakeBallot(
            InternalManifest manifest,
            string styleId,
            Func<int, int, bool> voteResultFunc)
        {
            return GetFakeBallot(
                manifest, styleId, $"fake-ballot-{Guid.NewGuid()}", voteResultFunc);
        }

        // get a fake ballot using an external function to determine the vote result
        // the function expects the contest id and the selection sequence order
        public static PlaintextBallot GetFakeBallot(
            InternalManifest manifest,
            string styleId,
            string ballotId,
            Func<int, int, bool> voteResultFunc)
        {
            var contests = new List<PlaintextBallotContest>();
            foreach (var contest in manifest.GetContests(styleId))
            {
                contests.Add(ContestFrom(contest, voteResultFunc));
            }

            return new PlaintextBallot(ballotId, styleId, contests.ToArray());
        }

        public static List<PlaintextBallot> GetFakeBallots(
            InternalManifest manifest,
            int count)
        {
            Console.WriteLine($"GetFakeBallots {count}");
            var ballots = new List<PlaintextBallot>();
            for (var i = 0; i < count; i++)
            {
                ballots.Add(GetFakeBallot(manifest, $"fake-ballot-{i}"));
            }

            return ballots;
        }

        public static List<PlaintextBallot> GetFakeBallots(
            InternalManifest manifest,
            Random random,
            int count
            )
        {
            Console.WriteLine($"GetFakeBallots {count}");
            var ballots = new List<PlaintextBallot>();
            for (var i = 0; i < count; i++)
            {
                ballots.Add(GetFakeBallot(manifest, random, $"fake-ballot-{i}"));
            }

            return ballots;
        }

        // get encrypted ballots using production values for the seed and nonce
        public static List<CiphertextBallot> GetFakeCiphertextBallots(
            InternalManifest manifest,
            CiphertextElectionContext context,
            EncryptionDevice device,
            int count)
        {
            Console.WriteLine($"GetFakeCiphertextBallots {count}");
            var ballots = new List<CiphertextBallot>();
            for (var i = 0; i < count; i++)
            {
                var ballot = GetFakeBallot(manifest);
                var ciphertext = Encrypt.Ballot(
                    ballot, manifest, context,
                    device.GetHash(), shouldVerifyProofs: false);
                ballots.Add(ciphertext);
            }

            return ballots;
        }

        // get encrypted ballots using production values for the seed and nonce
        public static List<CiphertextBallot> GetFakeCiphertextBallots(
           InternalManifest manifest,
            CiphertextElectionContext context,
            EncryptionDevice device,
            List<PlaintextBallot> palintext)
        {
            Console.WriteLine($"GetFakeCiphertextBallots {palintext.Count}");
            var ballots = new List<CiphertextBallot>();
            for (var i = 0; i < palintext.Count; i++)
            {
                var ballot = palintext[i];
                var ciphertext = Encrypt.Ballot(
                    ballot, manifest, context,
                    device.GetHash(), shouldVerifyProofs: false);
                ballots.Add(ciphertext);
            }

            return ballots;
        }

        // get encrypted ballots using the same seed and nonce for all ballots
        public static List<CiphertextBallot> GetFakeCiphertextBallots(
           InternalManifest manifest,
            CiphertextElectionContext context,
            List<PlaintextBallot> plaintext,
            ElementModQ seed,
            ElementModQ nonce)
        {
            Console.WriteLine($"GetFakeCiphertextBallots: withNonces {plaintext.Count}");
            ulong timestamp = 1;
            var ballots = new List<CiphertextBallot>();
            for (var i = 0; i < plaintext.Count; i++)
            {
                var ballot = plaintext[i];
                var ciphertext = Encrypt.Ballot(
                    ballot,
                    manifest,
                    context,
                    seed,
                    nonce,
                    timestamp,
                    shouldVerifyProofs: false,
                    usePrecomputedValues: false);
                ballots.Add(ciphertext);
            }

            return ballots;
        }

        // get encrypted ballots using a predefined random number generator
        public static List<CiphertextBallot> GetFakeCiphertextBallots(
           InternalManifest manifest,
            CiphertextElectionContext context,
            List<PlaintextBallot> plaintext,
            Random random)
        {
            Console.WriteLine($"GetFakeCiphertextBallots: withRandom {plaintext.Count}");
            var ballots = new List<CiphertextBallot>();
            for (var i = 0; i < plaintext.Count; i++)
            {
                var ballot = plaintext[i];
                var ciphertext = Encrypt.Ballot(
                    ballot,
                    manifest,
                    context,
                    random.NextElementModQ(),
                    random.NextElementModQ(),
                    shouldVerifyProofs: false,
                    usePrecomputedValues: false);
                ballots.Add(ciphertext);
            }

            return ballots;
        }
    }
}
