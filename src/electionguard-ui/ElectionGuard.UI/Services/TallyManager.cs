using ElectionGuard.Decryption;
using ElectionGuard.Decryption.Challenge;
using ElectionGuard.Decryption.ChallengeResponse;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.Extensions;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace ElectionGuard.UI.Services
{
    public class TallyManager : DisposableBase
    {

        private const string _adminUser = "admin";

        private readonly TallyMediator _tallyMediator;
        private readonly ContextService _contextService;
        private readonly ManifestService _manifestService;
        private readonly ElectionService _electionService;
        private readonly GuardianPublicKeyService _GuardianPublicKeyService;
        private readonly BallotService _ballotService;
        private readonly KeyCeremonyService _keyCeremonyService;
        private readonly DecryptionShareService _decryptionShareService;
        private readonly CiphertextTallyService _ciphertextTallyService;
        private readonly ChallengeResponseService _challengeResponseService;
        private readonly ChallengeService _challengeService;
        private readonly PlaintextTallyService _plaintextTallyService;

        public TallyManager(
            TallyMediator tallyMediator,
            ContextService contextService,
            ManifestService manifestService,
            ElectionService electionService,
            BallotService ballotService,
            KeyCeremonyService keyCeremonyService,
            DecryptionShareService decryptionShareService,
            GuardianPublicKeyService guardianPublicKeyService,
            ChallengeService challengeService,
            ChallengeResponseService challengeResponseService,
            PlaintextTallyService plaintextTallyService,
            CiphertextTallyService ciphertextTallyService)
        {
            // TODO: ABC order
            _tallyMediator = tallyMediator;
            _contextService = contextService;
            _manifestService = manifestService;
            _electionService = electionService;
            _GuardianPublicKeyService = guardianPublicKeyService;
            _challengeService = challengeService;
            _challengeResponseService = challengeResponseService;
            _ciphertextTallyService = ciphertextTallyService;
            _ballotService = ballotService;
            _keyCeremonyService = keyCeremonyService;
            _decryptionShareService = decryptionShareService;
            _plaintextTallyService = plaintextTallyService;
        }

        #region Admin Steps

        /// <summary>
        /// First Step in the tally process. To be run by Election Administrator.
        /// </summary>
        /// <param name="electionId"></param>
        /// <param name="tallyId"></param>
        /// <returns>an awaitable task</returns>
        /// <exception cref="ElectionGuardException"></exception>
        public async Task AccumulateAllUploadTallies(string electionId, string tallyId)
        {
            var contextRecord = await _contextService.GetByElectionIdAsync(electionId);
            if (contextRecord is null || string.IsNullOrEmpty(contextRecord.ToString()))
            {
                throw new ElectionGuardException(nameof(contextRecord));
            }

            using CiphertextElectionContext context = new(contextRecord);

            var manifestRecord = await _manifestService.GetByElectionIdAsync(electionId);
            if (manifestRecord is null || string.IsNullOrEmpty(manifestRecord.ToString()))
            {
                throw new ElectionGuardException(nameof(manifestRecord));
            }

            using InternalManifest manifest = new(manifestRecord.ToString());


            var tallies = await _ciphertextTallyService.GetAllByElectionIdAsync(electionId);
            if (tallies.IsNullOrEmpty())
            {
                throw new ElectionGuardException(nameof(tallies));
            }

            var ciphertextTally = _tallyMediator.CreateTally(tallyId, context, manifest);

            foreach (var tallyRecord in tallies)
            {
                using var tally = tallyRecord.ToString().ToCiphertextTally();

                // TODO: make this threadsafe
                _ = await ciphertextTally.AccumulateAsync(tally);
            }

            // save ciphertextTally as bit T tally
            _ = _ciphertextTallyService.SaveAsync(
                new()
                {
                    IsExportable = true,
                    CiphertextTallyData = ciphertextTally.ToJson(),
                    ElectionId = electionId,
                    TallyId = ciphertextTally.TallyId,
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="electionId"></param>
        /// <param name="tallyId"></param>
        /// <returns></returns>
        public async Task CreateChallenge(string electionId, string tallyId)
        {
            var mediator = await CreateDecryptionMediator(_adminUser, electionId, tallyId);
            await LoadAllShares(mediator, tallyId, electionId);

            var challenges = mediator
                .CreateChallenge(tallyId)
                .Select(ch =>
                    new ChallengeRecord()
                    {
                        TallyId = tallyId,
                        GuardianId = ch.Key,
                        ChallengeData = JsonConvert.SerializeObject(ch.Value),
                    });

            _ = await _challengeService.SaveManyAsync(challenges);
        }

        public async Task ValidateChallengeResponse(string electionId, string tallyId)
        {
            var mediator = await CreateDecryptionMediator(_adminUser, electionId, tallyId);
            var challengeBallots = await GetBallotsByState(electionId, BallotBoxState.Challenged);

            mediator.AddBallots(tallyId, challengeBallots!);
            await LoadAllShares(mediator, tallyId, electionId);

            mediator.CreateChallenge(tallyId);

            var responses = await _challengeResponseService.GetAllByTallyIdAsync(tallyId);
            foreach (var responseRecord in responses)
            {
                var response = JsonConvert.DeserializeObject<GuardianChallengeResponse>(responseRecord)!;
                mediator.SubmitResponse(tallyId, response);
            }

            if (!mediator.ValidateResponses(tallyId))
            { 
                return;
            }

            var result = mediator.Decrypt(tallyId);
            var plaintextTallyRecord = new PlaintextTallyRecord()
            {
                TallyId = tallyId,
                PlaintextTallyData = JsonConvert.SerializeObject(result.Tally),
            };

            _ = await _plaintextTallyService.SaveAsync(plaintextTallyRecord);

        }

        #endregion

        #region Guardian Steps
        private async Task<DecryptionMediator> CreateDecryptionMediator(string userId, string electionId, string tallyId)
        {
            var tallyRecord = await _ciphertextTallyService.GetByTallyIdAsync(tallyId);
            var electionRecord = await _electionService.GetByElectionIdAsync(electionId);
            var publicKeys = (await _GuardianPublicKeyService
                .GetAllByKeyCeremonyIdAsync(electionRecord!.KeyCeremonyId!))
                .Select(gpk => gpk.PublicKey!).ToList();

            if (tallyRecord is null || !tallyRecord.IsExportable || publicKeys.IsNullOrEmpty())
            {
                // TODO: clean this mess
                throw new ArgumentException("all of them");
            }

            using var tally = tallyRecord.ToString().ToCiphertextTally();

            var mediator = new DecryptionMediator(userId, tally, publicKeys);
            await AddSpoiledBallots(mediator, electionId, tallyId);

            return mediator;
        }

        /// <summary>
        /// First step a Guardian needs to accomplish. Must happen after Admin creates the big T tally
        /// </summary>
        /// <param name="guardianId"></param>
        /// <param name="electionId"></param>
        /// <param name="tallyId"></param>
        public async Task DecryptShare(string guardianId, string electionId, string tallyId)
        {
            ArgumentException.ThrowIfNullOrEmpty(guardianId);
            ArgumentException.ThrowIfNullOrEmpty(electionId);
            ArgumentException.ThrowIfNullOrEmpty(tallyId);

            var mediator = await CreateDecryptionMediator(guardianId, electionId, tallyId);


            using var guardian = await HydrateGuardian(guardianId, electionId);

            var challengeBallots = await GetBallotsByState(electionId, BallotBoxState.Challenged);
            var decryptionShares = guardian.ComputeDecryptionShares(mediator.Tallies[tallyId], challengeBallots);

            mediator.SubmitShares(decryptionShares, challengeBallots);

            await SaveShare(guardianId, tallyId, decryptionShares);
        }

        public async Task ComputeChallengeResponse(string guardianId, string electionId, string tallyId)
        {
            using var guardian = await HydrateGuardian(guardianId, electionId);
            var challengeRecord = await _challengeService.GetByGuardianIdAsync(tallyId, guardianId) ?? throw new ArgumentException(nameof(guardianId));
            var shareRecord = await _decryptionShareService.GetByGuardianIdAsync(tallyId, guardianId) ?? throw new ArgumentException(nameof(guardianId));

            var challenge = JsonConvert.DeserializeObject<GuardianChallenge>(challengeRecord.ChallengeData) ?? throw new ArgumentException(nameof(guardianId)); ;
            var share = JsonConvert.DeserializeObject<GuardianShare>(shareRecord.ShareData) ?? throw new ArgumentException(nameof(guardianId)); ;

            var response = guardian.ComputeChallengeResponse(share, challenge);
            var responseRecord = new ChallengeResponseRecord()
            {
                TallyId = tallyId,
                GuardianId = guardianId,
                ResponseData = JsonConvert.SerializeObject(response),
            };

            _ = await _challengeResponseService.SaveAsync(responseRecord);
        }

        private async Task SaveShare(string guardianId, string tallyId, DecryptionShare decryptionShare)
        {
            var shareRecord = new DecryptionShareRecord()
            {
                TallyId = tallyId,
                GuardianId = guardianId,
                ShareData = JsonConvert.SerializeObject(decryptionShare),
            };

            await _decryptionShareService.SaveAsync(shareRecord);
        }

        private async Task LoadShare(DecryptionMediator mediator, string guardianId, string tallyId, string electionId)
        {
            var share = await _decryptionShareService.GetByGuardianIdAsync(tallyId, guardianId) ?? throw new Exception(nameof(tallyId));

            var shareData = JsonConvert.DeserializeObject<DecryptionShare>(share.ShareData)!;
            var challengeBallots = await GetBallotsByState(electionId, BallotBoxState.Challenged);
            mediator.SubmitShares(shareData, challengeBallots);
        }

        private async Task LoadAllShares(DecryptionMediator mediator, string tallyId, string electionId)
        {
            var challengeBallots = await GetBallotsByState(electionId, BallotBoxState.Challenged);
            var shares = await _decryptionShareService.GetAllByTallyIdAsync(tallyId);

            foreach (var share in shares)
            {
                var shareData = JsonConvert.DeserializeObject<DecryptionShare>(share.ShareData)!;
                mediator.SubmitShares(shareData, challengeBallots);
            }
        }

        private async Task AddSpoiledBallots(DecryptionMediator mediator, string electionId, string tallyId)
        {
            // TODO: You would think we should dispose this. The mediator will handle that, eventually.
            var spoiledBallots = await GetBallotsByState(electionId, BallotBoxState.Spoiled);

            mediator.AddBallots(tallyId, spoiledBallots);
        }

        private async Task<List<CiphertextBallot>> GetBallotsByState(string electionId, BallotBoxState state)
        {
            var spoiledBallotCursor = await _ballotService.GetCursorBallotsByElectionIdStateAsync(electionId, state);
            return spoiledBallotCursor.ToList().Select(br => new CiphertextBallot(br.ToString())).ToList();
        }

        private async Task<Guardian> HydrateGuardian(string userId, string electionId)
        {
            Election electionRecord = await _electionService.GetByElectionIdAsync(electionId)
                ?? throw new ArgumentException(nameof(electionId));
            KeyCeremonyRecord keyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(electionRecord.KeyCeremonyId!)
                ?? throw new ArgumentException(nameof(electionId));

            return GuardianStorageExtensions.Load(userId, keyCeremony) ?? throw new ElectionGuardException(nameof(userId));
        }
        #endregion
    }
}
