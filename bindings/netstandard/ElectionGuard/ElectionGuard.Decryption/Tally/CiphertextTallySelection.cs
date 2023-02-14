using ElectionGuard.Encryption;
using ElectionGuard.Encryption.Ballot;

namespace ElectionGuard.Decryption.Tally
{
    public class CiphertextTallySelection : DisposableBase, ICiphertextSelection
    {
        public string ObjectId { get; init; } = default!;
        public ulong SequenceOrder { get; init; }
        public ElementModQ DescriptionHash { get; init; } = default!;
        public ElGamalCiphertext Ciphertext { get; set; } = default!;

        public ElGamalCiphertext ElGamalAccumulate(ElGamalCiphertext ciphertext)
        {
            var newValue = ElGamal.Add(ciphertext);
            Ciphertext.Dispose();
            Ciphertext = newValue;
            return Ciphertext;
        }
    }
}