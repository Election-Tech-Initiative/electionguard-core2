using ElectionGuard.Decryption;
using ElectionGuard.Decryption.Challenge;
using ElectionGuard.Decryption.ChallengeResponse;
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Extensions;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.Converters;
using ElectionGuard.Extensions;
using ElectionGuard.Guardians;
using MongoDB.Driver;
using Newtonsoft.Json;
using ElectionGuard.ElectionSetup;

namespace ElectionGuard.UI.Services
{
    public class TallyManager : DisposableBase
    {

        private const string _adminUser = "admin";

        private readonly TallyMediator _tallyMediator;
        private readonly ContextService _contextService;
        private readonly ManifestService _manifestService;
        private readonly GuardianPublicKeyService _guardianPublicKeyService;
        private readonly BallotService _ballotService;
        private readonly ChallengedBallotService _challengedBallotService;
        private readonly KeyCeremonyService _keyCeremonyService;
        private readonly DecryptionShareService _decryptionShareService;
        private readonly CiphertextTallyService _ciphertextTallyService;
        private readonly ChallengeResponseService _challengeResponseService;
        private readonly ChallengeService _challengeService;
        private readonly PlaintextTallyService _plaintextTallyService;
        private readonly GuardianBackupService _guardianBackupService;
        private readonly LagrangeCoefficientsService _lagrangeCoefficientsService;

        public TallyManager(
            TallyMediator tallyMediator,
            ContextService contextService,
            ManifestService manifestService,
            BallotService ballotService,
            ChallengedBallotService challengedBallotService,
            KeyCeremonyService keyCeremonyService,
            DecryptionShareService decryptionShareService,
            GuardianPublicKeyService guardianPublicKeyService,
            ChallengeService challengeService,
            ChallengeResponseService challengeResponseService,
            PlaintextTallyService plaintextTallyService,
            CiphertextTallyService ciphertextTallyService,
            GuardianBackupService guardianBackupService,
            LagrangeCoefficientsService lagrangeCoefficientsService)
        {
            // TODO: ABC order
            _tallyMediator = tallyMediator;
            _contextService = contextService;
            _manifestService = manifestService;
            _guardianPublicKeyService = guardianPublicKeyService;
            _challengeService = challengeService;
            _challengeResponseService = challengeResponseService;
            _ciphertextTallyService = ciphertextTallyService;
            _ballotService = ballotService;
            _challengedBallotService = challengedBallotService;
            _keyCeremonyService = keyCeremonyService;
            _decryptionShareService = decryptionShareService;
            _plaintextTallyService = plaintextTallyService;
            _guardianBackupService = guardianBackupService;
            _lagrangeCoefficientsService = lagrangeCoefficientsService;

        }

        #region Admin Steps

        /// <summary>
        /// First Step in the tally process. To be run by Election Administrator.
        /// </summary>
        /// <param name="electionId"></param>
        /// <param name="tallyId"></param>
        /// <returns>an awaitable task</returns>
        /// <exception cref="ElectionGuardException"></exception>
        public async Task AccumulateAllUploadTallies(TallyRecord tally)
        {
            ArgumentException.ThrowIfNullOrEmpty(tally.ElectionId);
            ArgumentException.ThrowIfNullOrEmpty(tally.Name);

            var contextRecord = await _contextService.GetByElectionIdAsync(tally.ElectionId);
            if (contextRecord is null || string.IsNullOrEmpty(contextRecord.ToString()))
            {
                throw new ElectionGuardException(nameof(contextRecord));
            }

            using CiphertextElectionContext context = new(contextRecord);

            var manifestRecord = await _manifestService.GetByElectionIdAsync(tally.ElectionId);
            if (manifestRecord is null || string.IsNullOrEmpty(manifestRecord.ToString()))
            {
                throw new ElectionGuardException(nameof(manifestRecord));
            }

            using Manifest tempManifest = new(manifestRecord.ToString());
            using InternalManifest manifest = new(tempManifest);

            var tallies = await _ciphertextTallyService.GetAllByElectionIdAsync(tally.ElectionId);
            if (tallies.IsNullOrEmpty())
            {
                throw new ElectionGuardException(nameof(tallies));
            }

            var ciphertextTally = _tallyMediator.CreateTally(tally.TallyId, tally.Name, context, manifest);

            foreach (var tallyRecord in tallies)
            {
                using var partialTally = tallyRecord.ToString().ToCiphertextTally();

                // TODO: make this threadsafe
                _ = await ciphertextTally.AccumulateAsync(partialTally);
            }

            // save ciphertextTally as bit T tally
            _ = _ciphertextTallyService.SaveAsync(
                new()
                {
                    IsExportable = true,
                    CiphertextTallyData = ciphertextTally.ToJson(),
                    ElectionId = tally.ElectionId,
                    TallyId = ciphertextTally.TallyId,
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="electionId"></param>
        /// <param name="tallyId"></param>
        /// <returns></returns>
        public async Task CreateChallenge(TallyRecord tally)
        {
            ArgumentException.ThrowIfNullOrEmpty(tally.ElectionId);

            var mediator = await CreateDecryptionMediator(_adminUser, tally);
            await LoadAllShares(mediator, tally);

            var challenges = mediator
                .CreateChallenge(tally.TallyId)
                .Select(ch =>
                    new ChallengeRecord()
                    {
                        TallyId = tally.TallyId,
                        GuardianId = ch.Key,
                        ChallengeData = JsonConvert.SerializeObject(ch.Value),
                    });

            _ = await _challengeService.SaveManyAsync(challenges);
        }

        public async Task ValidateChallengeResponse(TallyRecord tally)
        {
            ArgumentException.ThrowIfNullOrEmpty(tally.ElectionId);

            var mediator = await CreateDecryptionMediator(_adminUser, tally);
            await LoadAllShares(mediator, tally);

            mediator.AccumulateShares(tally.TallyId, false);
            // mediator.CreateChallenge(tallyId);
            var challengeRecords = await _challengeService.GetAllByTallyIdAsync(tally.TallyId) ?? throw new ArgumentException(nameof(tally.TallyId));
            foreach (var challengeRecord in challengeRecords)
            {
                var challenge = JsonConvert.DeserializeObject<GuardianChallenge>(
                    challengeRecord.ChallengeData) ??
                     throw new ArgumentException(nameof(tally.TallyId));

                mediator.LoadChallenge(tally.TallyId, challenge.GuardianId, challenge);
            }

            var responses = await _challengeResponseService.GetAllByTallyIdAsync(tally.TallyId);
            foreach (var responseRecord in responses)
            {
                var response = JsonConvert.DeserializeObject<GuardianChallengeResponse>(responseRecord)!;
                mediator.SubmitResponse(tally.TallyId, response);
            }

            if (!mediator.ValidateResponses(tally.TallyId))
            {
                throw new ElectionGuardException("did not validate");
            }

            using var result = mediator.Decrypt(tally.TallyId, false);
            var plaintextTallyRecord = new PlaintextTallyRecord()
            {
                TallyId = tally.TallyId,
                PlaintextTallyData = JsonConvert.SerializeObject(result.Tally).Replace("\r\n", string.Empty),
            };
            _ = await _plaintextTallyService.SaveAsync(plaintextTallyRecord);

            if (result.ChallengedBallots is not null)
            {
                await SaveChallengedBallots(tally, result);
            }

            await SaveCoefficients(tally, mediator);
        }

        private async Task SaveCoefficients(TallyRecord tally, DecryptionMediator mediator)
        {
            var publicKeys = mediator.Guardians.Values.ToList();
            var coefficients = publicKeys.ComputeLagrangeCoefficients().ToDictionary(c => c.Key, c => c.Value.Coefficient.ToHex());

            var coefficientsRecord = new LagrangeCoefficientsRecord()
            {
                TallyId = tally.TallyId,
                LagrangeCoefficientsData = JsonConvert.SerializeObject(coefficients)
            };
            _ = await _lagrangeCoefficientsService.SaveAsync(coefficientsRecord);
        }

        private async Task SaveChallengedBallots(TallyRecord tally, DecryptionResult result)
        {
            ArgumentException.ThrowIfNullOrEmpty(tally.ElectionId);

            foreach (var challenged in result.ChallengedBallots!)
            {
                var record = new ChallengedBallotRecord()
                {
                    TallyId = tally.TallyId,
                    ElectionId = tally.ElectionId,
                    BallotCode = challenged.Name,
                    BallotData = JsonConvert.SerializeObject(challenged)
                };
                _ = await _challengedBallotService.SaveAsync(record);
            }
        }

        #endregion

        #region Guardian Steps
        private async Task<DecryptionMediator> CreateDecryptionMediator(string userId, TallyRecord tally)
        {
            ArgumentException.ThrowIfNullOrEmpty(tally.KeyCeremonyId);
            ArgumentException.ThrowIfNullOrEmpty(tally.ElectionId);

            var ciphertextTallyRecord = await _ciphertextTallyService.GetByTallyIdAsync(tally.TallyId);
            var publicKeys = (await _guardianPublicKeyService
                .GetAllByKeyCeremonyIdAsync(tally.KeyCeremonyId))
                .Select(gpk => gpk.PublicKey!).ToList();

            if (ciphertextTallyRecord is null || !ciphertextTallyRecord.IsExportable || publicKeys.IsNullOrEmpty())
            {
                throw new ArgumentException("Bad Ciphertext Tally");
            }

            using var ciphertextTally = ciphertextTallyRecord.ToString().ToCiphertextTally();

            var mediator = new DecryptionMediator(userId, ciphertextTally, publicKeys);
            await AddSpoiledBallots(mediator, tally);
            await AddChallengedBallots(mediator, tally);

            return mediator;
        }

        /// <summary>
        /// First step a Guardian needs to accomplish. Must happen after Admin creates the big T tally
        /// </summary>
        /// <param name="guardianId"></param>
        /// <param name="electionId"></param>
        /// <param name="tallyId"></param>
        public async Task DecryptShare(string guardianId, TallyRecord tally)
        {
            ArgumentException.ThrowIfNullOrEmpty(guardianId);
            ArgumentException.ThrowIfNullOrEmpty(tally.ElectionId);

            using var mediator = await CreateDecryptionMediator(guardianId, tally);
            using var guardian = await HydrateGuardian(guardianId, tally, mediator.Guardians);

            var challengeBallots = (await GetBallotsByState(tally.ElectionId, BallotBoxState.Challenged)).ToList();
            using var decryptionShares = guardian.ComputeDecryptionShares(mediator.Tallies[tally.TallyId], challengeBallots);

            mediator.SubmitShares(decryptionShares, challengeBallots);

            await SaveShare(guardianId, tally, decryptionShares);
        }

        public async Task ComputeChallengeResponse(string guardianId, TallyRecord tally)
        {
            ArgumentException.ThrowIfNullOrEmpty(tally.ElectionId);

            using var mediator = await CreateDecryptionMediator(guardianId, tally);
            await LoadAllShares(mediator, tally);

            using var guardian = await HydrateGuardian(guardianId, tally, mediator.Guardians);
            var challengeRecord = await _challengeService.GetByGuardianIdAsync(tally.TallyId, guardianId) ?? throw new ArgumentException(nameof(guardianId));

            using var challenge = JsonConvert.DeserializeObject<GuardianChallenge>(challengeRecord.ChallengeData) ?? throw new ArgumentException(nameof(guardianId)); ;
            var share = mediator.GetShare(tally.TallyId, guardianId)!;

            using var response = guardian.ComputeChallengeResponse(share, challenge);

            var responseRecord = new ChallengeResponseRecord()
            {
                TallyId = tally.TallyId,
                GuardianId = guardianId,
                ResponseData = JsonConvert.SerializeObject(response),
            };

            _ = await _challengeResponseService.SaveAsync(responseRecord);
        }

        private async Task SaveShare(string guardianId, TallyRecord tally, DecryptionShare decryptionShare)
        {
            var shareRecord = new DecryptionShareRecord()
            {
                TallyId = tally.TallyId,
                GuardianId = guardianId,
                ShareData = JsonConvert.SerializeObject(decryptionShare),
            };

            _ = await _decryptionShareService.SaveAsync(shareRecord);
        }

        private async Task LoadAllShares(DecryptionMediator mediator, TallyRecord tally)
        {
            ArgumentException.ThrowIfNullOrEmpty(tally.ElectionId);

            var challengeBallots = await GetBallotsByState(tally.ElectionId, BallotBoxState.Challenged);
            var shares = await _decryptionShareService.GetAllByTallyIdAsync(tally.TallyId);

            foreach (var share in shares)
            {
                var shareData = JsonConvert.DeserializeObject<DecryptionShare>(share.ShareData)!;
                mediator.SubmitShares(shareData, challengeBallots.ToList());
            }
        }

        private async Task AddSpoiledBallots(DecryptionMediator mediator, TallyRecord tally)
        {
            await AddBallots(mediator, tally, BallotBoxState.Spoiled);
        }

        private async Task AddChallengedBallots(DecryptionMediator mediator, TallyRecord tally)
        {
            await AddBallots(mediator, tally, BallotBoxState.Challenged);
        }

        private async Task AddBallots(DecryptionMediator mediator, TallyRecord tally, BallotBoxState ballotBoxState)
        {
            ArgumentException.ThrowIfNullOrEmpty(tally.ElectionId);

            if (ballotBoxState is not BallotBoxState.Spoiled and not BallotBoxState.Challenged)
            {
                throw new ArgumentOutOfRangeException(nameof(ballotBoxState));
            }

            // letting the mediator dispose of this data
            var challengedBallots = await GetBallotsByState(tally.ElectionId, ballotBoxState);

            mediator.AddBallots(tally.TallyId, challengedBallots.ToList());
        }

        private async Task<IEnumerable<CiphertextBallot>> GetBallotsByState(string electionId, BallotBoxState state)
        {
            var spoiledBallotCursor = await _ballotService.GetCursorBallotsByElectionIdStateAsync(electionId, state);
            return spoiledBallotCursor.ToList().Select(br => new CiphertextBallot(br.ToString()));
        }

        private async Task<Guardian> HydrateGuardian(
            string userId,
            TallyRecord tally,
            Dictionary<string, ElectionPublicKey>? publicKeys = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(tally.KeyCeremonyId);
            ArgumentException.ThrowIfNullOrEmpty(tally.ElectionId);


            var keyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(tally.KeyCeremonyId) ??
                throw new ArgumentException(nameof(tally.ElectionId));

            var backups = await _guardianBackupService.GetByGuardianIdAsync(keyCeremony.KeyCeremonyId!, userId) ??
                throw new ArgumentException(nameof(keyCeremony.KeyCeremonyId));

            try
            {
                var guardian = GuardianStorageExtensions.Load(
                    userId,
                    keyCeremony,
                    publicKeys,
                    backups.ToDictionary(k => k.GuardianId!, v => v.Backup!.ToRecord())) ?? throw new ElectionGuardException(nameof(userId));
                return guardian;
            }
            catch (Exception ex)
            {
                throw new ElectionGuardException(AppResources.ErrorLoadingGuardian, ex);
            }
        }
        #endregion
    }
}
