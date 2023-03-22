using System;
using System.Collections.Generic;
using ElectionGuard.Encryption.Utils.Generators;
using NUnit.Framework;

namespace ElectionGuard.Encryption.Tests
{
    [TestFixture]
    public class TestManifest
    {

        [Test]
        public void Test_Votes_Allowed_On_Create_Contest()
        {
            var selections = Array.Empty<SelectionDescription>();

            const int sequenceOrder = 1;
            const string electoralDistrictId = "district-id";
            const string contestId = "contest-id";
            const int numberElected = 3;
            const string contestName = "test election";

            var contestThreeVotes = new ContestDescription(
                contestId,
                electoralDistrictId,
                sequenceOrder,
                VoteVariationType.NOfM,
                numberElected,
                contestName,
                selections);

            // Assert
            Assert.AreEqual(3, contestThreeVotes.VotesAllowed);
        }

        [Test]
        public void Test_Can_Construct_Internationalized_Text()
        {
            // Arrange
            var language1 = new Language("some words", "en");
            var language2 = new Language("algunas palabras", "es");
            var languages = new[] { language1, language2 };

            // Act
            var subject = new InternationalizedText(languages);

            // Assert
            var actual = subject.GetTextAt(0);
            Assert.That(actual.Value == "some words");
        }

        [Test]
        public void Test_Can_Party()
        {
            // Act
            var party = new Party("new party");

            // Assert
            Assert.IsNotNull(party.Name);
        }

        [Test]
        public void Test_Can_Construct_Ballot_style()
        {
            var gpUnitIds = new[] { "gp-unit-1", "gp-unit-2" };

            var subject = new BallotStyle("some-object-id", gpUnitIds);

            var actual = subject.GetGeopoliticalUnitIdAtIndex(0);
            Assert.That(actual == "gp-unit-1");
        }

        [Test]
        public void Test_Can_Construct_InternalManifest_From_Generated_Manifest()
        {
            // Get a slightly more complex manifest that shows including multiple ballot styles
            var manifest = ManifestGenerator.GetFakeManifest();
            var internalManifest = new InternalManifest(manifest);

            Assert.That(manifest.CryptoHash().ToHex() == internalManifest.ManifestHash.ToHex());
            Assert.That(manifest.IsValid());
        }

        [Test]
        public void Test_Can_Construct_InternalManifest_From_Sample_Manifest_From_File()
        {
            var manifest = ManifestGenerator.GetManifestFromFile();
            var internalManifest = new InternalManifest(manifest);

            Assert.That(manifest.CryptoHash().ToHex() == internalManifest.ManifestHash.ToHex());
            Assert.That(manifest.IsValid());
        }

        [Test]
        public void Test_Can_Serialize_Generated_Manifest()
        {
            var manifest = ManifestGenerator.GetFakeManifest();
            var json = manifest.ToJson();

            var result = new Manifest(json);

            Assert.That(manifest.CryptoHash().ToHex() == result.CryptoHash().ToHex());
            Assert.That(result.IsValid());
        }

        [Test]
        public void Test_Can_Serialize_Sample_Manifest_From_File()
        {
            var subject = ManifestGenerator.GetManifestFromFile();
            Assert.That(subject.IsValid);
        }

        [Test]
        public void Test_Can_Create_Manifest_With_Missing_Party_Name()
        {
            // Arrange
            var language = new Language(
                string.Format("{0},{1}", "my jurisdiction", "here"), "en");
            var gpUnits = new List<GeopoliticalUnit>();
            var parties = new List<Party>();
            var candidates = new List<Candidate>();
            var contests = new List<ContestDescription>();
            var ballotStyles = new List<BallotStyle>();

            gpUnits.Add(new GeopoliticalUnit("mydistrict", "first unit", ReportingUnitType.City));
            candidates.Add(new Candidate("mycandidate", false));
            var selections = new List<SelectionDescription>
            {
                new SelectionDescription("selection1", "mycandidate", 1)
            };
            contests.Add(new ContestDescription(
                "firstcontest", "mydistrict",
                1, VoteVariationType.NOfM, 1,
                "mrmayor", selections.ToArray()));
            string[] gps = { "mydistrict" };
            ballotStyles.Add(new BallotStyle("style1", gps));

            parties.Add(new Party("myparty"));

            var result = new Manifest(
                "test-manifest",
                ElectionType.General,
                DateTime.Now.AddDays(1),
                DateTime.Now.AddDays(1).AddDays(1),
                gpUnits.ToArray(),
                parties.ToArray(),
                candidates.ToArray(),
                contests.ToArray(),
                ballotStyles.ToArray(),
                new InternationalizedText(new[] { language }),
                new ContactInformation("na"));

            var json = result.ToJson();

            Assert.IsTrue(result.IsValid());

            // check to make sure the party name serialized correctly
            Assert.IsFalse(json.Contains("\"name\":{\"text\":null"));
        }

        [Test]
        public void Test_Unicode_CandidateNames()
        {
            var candidateName = new Language("Raúl", "en");
            var candidate = new Candidate(
                "candidate-1",
                new InternationalizedText(new[] { candidateName }),
                string.Empty,
                string.Empty,
                false);

            var candidates = new List<Candidate>
            {
                candidate
            };

            var result = new Manifest(
                "test-manifest",
                ElectionType.General,
                DateTime.Now,
                DateTime.Now,
                new GeopoliticalUnit[] { },
                new Party[] { },
                candidates.ToArray(),
                new ContestDescription[] { },
                new BallotStyle[] { },
                new InternationalizedText(new Language[] { }),
                new ContactInformation("na"));

            var json = result.ToJson();
            Assert.IsTrue(json.Contains("\"value\":\"Ra\\u00fal\""));
        }
    }
}
