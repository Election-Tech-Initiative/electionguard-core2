using System;
using System.Threading;
using ElectionGuard.Encryption.Utils.Generators;

namespace ElectionGuard.Encryption.Bench
{

    public class BenchEncrypt : Fixture
    {
        private const int MaxCompleteDelay = 7000;
        private readonly ElementModQ _nonce;
        private readonly ElGamalKeyPair _keypair;
        private readonly InternalManifest _internalManifest;
        private readonly CiphertextElectionContext _context;
        private readonly EncryptionDevice _device;
        private readonly PlaintextBallot _ballot;

        public BenchEncrypt()
        {
            var secret = new ElementModQ("A9FA69F9686810ED82DAF9020FE80DFE0FC0FDCBF7FA55B93C811F0BA2650101");
            _nonce = new ElementModQ("4BD3231DC17E9E84F5B0A5D2C4160C6A2299EDAE184C291E17709913B8F9CB40");
            _keypair = ElGamalKeyPair.FromSecret(secret);
            var manifest = ManifestGenerator.GetManifestFromFile();
            _internalManifest = new InternalManifest(manifest);
            _context = new CiphertextElectionContext(
                1UL, 1UL, _keypair.PublicKey, Constants.TWO_MOD_Q, _internalManifest.ManifestHash);
            _device = new EncryptionDevice(12345UL, 23456UL, 34567UL, "Location");

            _ballot = BallotGenerator.GetFakeBallot(_internalManifest);
            //Console.WriteLine(ballot.ToJson());
        }

        public override void Run()
        {
            Bench_Encrypt_BallotFull_NoProofCheck();
            Bench_Encrypt_BallotFull_WithProofCheck();
            Bench_Encrypt_Ballot_Compact_NoProofCheck();

            Setup_Precompute_Buffers();
            Bench_Encrypt_BallotFull_NoProofCheck();
            // TODO: Bench_Encrypt_Ballot_Compact_WithProofCheck();
        }

        public void Bench_Encrypt_BallotFull_NoProofCheck()
        {
            Console.WriteLine("Bench_Encrypt_BallotFull_NoProofCheck");
            Run(() =>
            {
                Encrypt.Ballot(_ballot, _internalManifest, _context, _device.GetHash(), _nonce, false);
            });
        }

        public void Bench_Encrypt_BallotFull_WithProofCheck()
        {
            Console.WriteLine("Bench_Encrypt_BallotFull_WithProofCheck");
            Run(() =>
            {
                Encrypt.Ballot(_ballot, _internalManifest, _context, _device.GetHash(), _nonce);
            });
        }

        public void Bench_Encrypt_Ballot_Compact_NoProofCheck()
        {
            Console.WriteLine("Bench_Encrypt_Ballot_Compact_NoProofCheck");
            Run(() =>
            {
                Encrypt.CompactBallot(_ballot, _internalManifest, _context, _device.GetHash(), _nonce, false);
            });
        }

        public void Bench_Encrypt_Ballot_Compact_WithProofCheck()
        {
            Console.WriteLine("Bench_Encrypt_Ballot_Compact_WithProofCheck");
            Run(() =>
            {
                Encrypt.CompactBallot(_ballot, _internalManifest, _context, _device.GetHash(), _nonce);
            });
        }

        public void Setup_Precompute_Buffers()
        {
            Console.WriteLine("Setup_Precompute_Buffers");
            var waitHandle = new AutoResetEvent(false);

            var precompute = new Precompute();
            precompute.CompletedEvent += _ =>
            {
                waitHandle.Set();
            };
            precompute.StartPrecomputeAsync(_keypair.PublicKey, 1000);
            waitHandle.WaitOne(MaxCompleteDelay);
            Run(() =>
            {
                precompute.StopPrecompute();
            });
        }

    }
}
