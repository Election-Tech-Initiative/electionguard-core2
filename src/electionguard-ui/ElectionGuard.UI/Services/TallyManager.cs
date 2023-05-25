﻿using ElectionGuard.Decryption;
using ElectionGuard.Decryption.Challenge;
using ElectionGuard.Decryption.ChallengeResponse;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.Encryption.Utils.Converters;
using ElectionGuard.Extensions;
using ElectionGuard.Guardians;
using ElectionGuard.UI.Lib.Models;
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
        private readonly GuardianPublicKeyService _guardianPublicKeyService;
        private readonly BallotService _ballotService;
        private readonly KeyCeremonyService _keyCeremonyService;
        private readonly DecryptionShareService _decryptionShareService;
        private readonly CiphertextTallyService _ciphertextTallyService;
        private readonly ChallengeResponseService _challengeResponseService;
        private readonly ChallengeService _challengeService;
        private readonly PlaintextTallyService _plaintextTallyService;
        private readonly GuardianBackupService _guardianBackupService;

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
            CiphertextTallyService ciphertextTallyService,
            GuardianBackupService guardianBackupService)
        {
            // TODO: ABC order
            _tallyMediator = tallyMediator;
            _contextService = contextService;
            _manifestService = manifestService;
            _electionService = electionService;
            _guardianPublicKeyService = guardianPublicKeyService;
            _challengeService = challengeService;
            _challengeResponseService = challengeResponseService;
            _ciphertextTallyService = ciphertextTallyService;
            _ballotService = ballotService;
            _keyCeremonyService = keyCeremonyService;
            _decryptionShareService = decryptionShareService;
            _plaintextTallyService = plaintextTallyService;
            _guardianBackupService = guardianBackupService;
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
            var electionId = tally.ElectionId;
            var tallyId = tally.TallyId;

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

            using Manifest tempManifest = new(manifestRecord.ToString());
            using InternalManifest manifest = new(tempManifest);


            var tallies = await _ciphertextTallyService.GetAllByElectionIdAsync(electionId);
            if (tallies.IsNullOrEmpty())
            {
                throw new ElectionGuardException(nameof(tallies));
            }

            var ciphertextTally = _tallyMediator.CreateTally(tallyId, tally.Name!, context, manifest);

            foreach (var tallyRecord in tallies)
            {
                try
                {
                    using var partialTally = tallyRecord.ToString().ToCiphertextTally();

                    // TODO: make this threadsafe
                    _ = await ciphertextTally.AccumulateAsync(partialTally);
                }
                catch (Exception ex) 
                {
                    throw;
                }
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
        public async Task CreateChallenge(TallyRecord tally)
        {
            var electionId = tally.ElectionId!;
            var tallyId = tally.TallyId!;

            var mediator = await CreateDecryptionMediator(_adminUser, tally);
            await LoadAllShares(mediator, tallyId, electionId);

            var challenges = mediator
                .CreateChallenge(tallyId)
                .Select(ch =>
                    new ChallengeRecord()
                    {
                        TallyId = tallyId,
                        GuardianId = ch.Key,
                        ChallengeData = JsonConvert.SerializeObject(ch.Value, SerializationSettings.NewtonsoftSettings()),
                    });

            _ = await _challengeService.SaveManyAsync(challenges);
        }

        public async Task ValidateChallengeResponse(TallyRecord tally)
        {
            var electionId = tally.ElectionId!;
            var tallyId = tally.TallyId!;

            var mediator = await CreateDecryptionMediator(_adminUser, tally);
            await LoadAllShares(mediator, tallyId, electionId);

            mediator.AccumulateShares(tallyId);
            // mediator.CreateChallenge(tallyId);
            var challengeRecords = await _challengeService.GetAllByTallyIdAsync(tallyId) ?? throw new ArgumentException(nameof(tallyId));
            foreach (var challengeRecord in challengeRecords)
            {
                var challenge = JsonConvert.DeserializeObject<GuardianChallenge>(challengeRecord.ChallengeData, SerializationSettings.NewtonsoftSettings()) ?? throw new ArgumentException(nameof(tallyId)); ;
                mediator.LoadChallenge(tallyId, challenge.GuardianId, challenge);
            }

            var responses = await _challengeResponseService.GetAllByTallyIdAsync(tallyId);
            foreach (var responseRecord in responses)
            {
                var response = JsonConvert.DeserializeObject<GuardianChallengeResponse>(responseRecord, SerializationSettings.NewtonsoftSettings())!;
                mediator.SubmitResponse(tallyId, response);
            }

            if (!mediator.ValidateResponses(tallyId))
            {
                throw new ElectionGuardException("did not validate");
            }

            var result = mediator.Decrypt(tallyId);
            var plaintextTallyRecord = new PlaintextTallyRecord()
            {
                TallyId = tallyId,
                PlaintextTallyData = JsonConvert.SerializeObject(result.Tally, SerializationSettings.NewtonsoftSettings()),
            };

            _ = await _plaintextTallyService.SaveAsync(plaintextTallyRecord);

        }

        #endregion

        #region Guardian Steps
        private async Task<DecryptionMediator> CreateDecryptionMediator(string userId, TallyRecord tally)
        {
            var electionId = tally.ElectionId!;
            var tallyId = tally.TallyId!;

            var ciphertextTallyRecord = await _ciphertextTallyService.GetByTallyIdAsync(tallyId);
            var electionRecord = await _electionService.GetByElectionIdAsync(electionId);
            var publicKeys = (await _guardianPublicKeyService
                .GetAllByKeyCeremonyIdAsync(electionRecord!.KeyCeremonyId!))
                .Select(gpk => gpk.PublicKey!).ToList();

            if (ciphertextTallyRecord is null || !ciphertextTallyRecord.IsExportable || publicKeys.IsNullOrEmpty())
            {
                // TODO: clean this mess
                throw new ArgumentException("all of them");
            }

            using var ciphertextTally = ciphertextTallyRecord.ToString().ToCiphertextTally();

            var mediator = new DecryptionMediator(userId, ciphertextTally, publicKeys);
            await AddSpoiledBallots(mediator, electionId, tallyId);
            await AddChallengedBallots(mediator, electionId, tallyId);

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
            var electionId = tally.ElectionId!;
            var tallyId = tally.TallyId;

            ArgumentException.ThrowIfNullOrEmpty(guardianId);

            var mediator = await CreateDecryptionMediator(guardianId, tally);
            using var guardian = await HydrateGuardian(guardianId, electionId, mediator.Guardians);

            var challengeBallots = await GetBallotsByState(electionId, BallotBoxState.Challenged);
            var decryptionShares = guardian.ComputeDecryptionShares(mediator.Tallies[tallyId], challengeBallots);

            if (challengeBallots.Count > 0)
            {
                mediator.SubmitShares(decryptionShares, challengeBallots);
            }

            await SaveShare(guardianId, tallyId, decryptionShares);
        }

        public async Task ComputeChallengeResponse(string guardianId, TallyRecord tally)
        {
            var electionId = tally.ElectionId!;
            var tallyId = tally.TallyId!;

            var mediator = await CreateDecryptionMediator(guardianId, tally);
            await LoadAllShares(mediator, tallyId, electionId);

            using var guardian = await HydrateGuardian(guardianId, electionId, mediator.Guardians);
            var challengeRecord = await _challengeService.GetByGuardianIdAsync(tallyId, guardianId) ?? throw new ArgumentException(nameof(guardianId));

            var challenge = JsonConvert.DeserializeObject<GuardianChallenge>(challengeRecord.ChallengeData, SerializationSettings.NewtonsoftSettings()) ?? throw new ArgumentException(nameof(guardianId)); ;
            var share = mediator.GetShare(tallyId, guardianId)!;

            var response = guardian.ComputeChallengeResponse(share, challenge);
            var responseRecord = new ChallengeResponseRecord()
            {
                TallyId = tallyId,
                GuardianId = guardianId,
                ResponseData = JsonConvert.SerializeObject(response, SerializationSettings.NewtonsoftSettings()),
            };

            _ = await _challengeResponseService.SaveAsync(responseRecord);
        }

        private async Task SaveShare(string guardianId, string tallyId, DecryptionShare decryptionShare)
        {
            var shareRecord = new DecryptionShareRecord()
            {
                TallyId = tallyId,
                GuardianId = guardianId,
                ShareData = JsonConvert.SerializeObject(decryptionShare, SerializationSettings.NewtonsoftSettings()),
            };

            await _decryptionShareService.SaveAsync(shareRecord);
        }

        private async Task LoadShare(DecryptionMediator mediator, string guardianId, string tallyId, string electionId)
        {
            var share = await _decryptionShareService.GetByGuardianIdAsync(tallyId, guardianId) ?? throw new Exception(nameof(tallyId));

            var shareData = JsonConvert.DeserializeObject<DecryptionShare>(share.ShareData, SerializationSettings.NewtonsoftSettings())!;
            var challengeBallots = await GetBallotsByState(electionId, BallotBoxState.Challenged);
            mediator.SubmitShares(shareData, challengeBallots);
        }

        private async Task LoadAllShares(DecryptionMediator mediator, string tallyId, string electionId)
        {
            var challengeBallots = await GetBallotsByState(electionId, BallotBoxState.Challenged);
            var shares = await _decryptionShareService.GetAllByTallyIdAsync(tallyId);

            foreach (var share in shares)
            {
                var shareData = JsonConvert.DeserializeObject<DecryptionShare>(share.ShareData, SerializationSettings.NewtonsoftSettings())!;
                mediator.SubmitShares(shareData, challengeBallots);
            }
        }

        private async Task AddSpoiledBallots(DecryptionMediator mediator, string electionId, string tallyId)
        {
            // letting the mediator dispose of this data
            var spoiledBallots = await GetBallotsByState(electionId, BallotBoxState.Spoiled);

            mediator.AddBallots(tallyId, spoiledBallots);
        }

        private async Task AddChallengedBallots(DecryptionMediator mediator, string electionId, string tallyId)
        {
            // letting the mediator dispose of this data
            var challengedBallots = await GetBallotsByState(electionId, BallotBoxState.Challenged);

            mediator.AddBallots(tallyId, challengedBallots);
        }

        private async Task<List<CiphertextBallot>> GetBallotsByState(string electionId, BallotBoxState state)
        {
            var spoiledBallotCursor = await _ballotService.GetCursorBallotsByElectionIdStateAsync(electionId, state);
            return spoiledBallotCursor.ToList().Select(br => new CiphertextBallot(br.ToString())).ToList();
        }

        private async Task<Guardian> HydrateGuardian(
            string userId,
            string electionId,
            Dictionary<string, ElectionPublicKey>? publicKeys = null)
        {
            Election electionRecord = await _electionService.GetByElectionIdAsync(electionId)
                ?? throw new ArgumentException(nameof(electionId));
            KeyCeremonyRecord keyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(electionRecord.KeyCeremonyId!)
                ?? throw new ArgumentException(nameof(electionId));

            Dictionary<string, ElectionPartialKeyBackup>? otherBackups = new();
            var backups = await _guardianBackupService.GetByGuardianIdAsync(keyCeremony.KeyCeremonyId!, userId);
            foreach (var backup in backups!)
            {
                otherBackups.Add(backup.GuardianId!, backup.Backup!);
            }

            var guardian = GuardianStorageExtensions.Load(userId, keyCeremony, publicKeys, otherBackups) ?? throw new ElectionGuardException(nameof(userId));
            return guardian;
        }
        #endregion
    }
}