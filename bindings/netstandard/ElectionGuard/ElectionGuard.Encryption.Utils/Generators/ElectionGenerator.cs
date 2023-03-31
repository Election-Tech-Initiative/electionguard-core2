
namespace ElectionGuard.Encryption.Utils.Generators
{
    public class TestElectionData : DisposableBase
    {
        public Manifest Manifest { get; set; }
        public InternalManifest InternalManifest { get; set; }
        public CiphertextElectionContext Context { get; set; }
        public ElGamalKeyPair KeyPair { get; set; }

        public EncryptionDevice Device { get; set; }

        protected override void DisposeUnmanaged()
        {
            Manifest?.Dispose();
            InternalManifest?.Dispose();
            Context?.Dispose();
            KeyPair?.Dispose();
            Device?.Dispose();
        }
    }
    public class ElectionGenerator
    {
        public static ElGamalKeyPair GenerateFakeKeyPair(bool useRandom = false)
        {
            var secretKey = useRandom ? BigMath.RandQ() : Constants.TWO_MOD_Q;
            return ElGamalKeyPair.FromSecret(secretKey);
        }
        public static ElGamalKeyPair GenerateFakeKeyPair(ElementModQ secretKey)
        {
            return ElGamalKeyPair.FromSecret(secretKey);
        }

        public static CiphertextElectionContext GetFakeContext(
            ElGamalKeyPair keyPair,
            ElementModQ manifestHash)
        {
            return new CiphertextElectionContext(
                1UL, 1UL,
                keyPair.PublicKey,
                Constants.TWO_MOD_Q,
                manifestHash);
        }

        public static CiphertextElectionContext GetFakeContext(
            ulong guardians, ulong quorum,
            ElGamalKeyPair keyPair,
            ElementModQ manifestHash)
        {
            return new CiphertextElectionContext(
                guardians, quorum,
                keyPair.PublicKey,
                Constants.TWO_MOD_Q,
                manifestHash);
        }

        public static CiphertextElectionContext GetFakeContext(
            ulong guardians, ulong quorum,
            ElementModP publicKey,
            ElementModQ manifestHash)
        {
            return new CiphertextElectionContext(
                guardians, quorum,
                publicKey,
                Constants.TWO_MOD_Q,
                manifestHash);
        }

        public static EncryptionDevice GetFakeEncryptionDevice()
        {
            return new EncryptionDevice(12345UL, 23456UL, 34567UL, "Location");
        }

        /// <summary>
        /// Generates fake election data
        /// </summary>
        public static TestElectionData GenerateFakeElectionData(
            ulong numberOfGuardians = 1,
            ulong quorum = 1,
            bool useRandom = false)
        {
            var keyPair = GenerateFakeKeyPair(useRandom);
            var manifest = ManifestGenerator.GetManifestFromFile();
            var internalManifest = new InternalManifest(manifest);
            var context = GetFakeContext(
                numberOfGuardians, quorum,
                keyPair,
                internalManifest.ManifestHash);
            var device = GetFakeEncryptionDevice();
            return new TestElectionData
            {
                Manifest = manifest,
                InternalManifest = internalManifest,
                Context = context,
                KeyPair = keyPair,
                Device = device
            };
        }
    }
}
