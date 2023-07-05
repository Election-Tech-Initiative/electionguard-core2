#include "../../src/electionguard/log.hpp"
#include "utils/constants.hpp"

#include <doctest/doctest.h>
#include <electionguard/constants.h>
#include <electionguard/convert.hpp>
#include <electionguard/elgamal.hpp>
#include <electionguard/group.hpp>
#include <electionguard/hash.hpp>
#include <electionguard/nonces.hpp>
#include <electionguard/precompute_buffers.hpp>
#include <stdexcept>

using namespace electionguard;
using namespace std;

using std::wstring;

TEST_CASE("ElGamalKeyPair fromSecret public key is fixed base")
{
    auto secret = rand_q();
    auto keypair = ElGamalKeyPair::fromSecret(*secret);
    auto *publicKey = keypair->getPublicKey();

    CHECK((keypair->getPublicKey()->isFixedBase() == true));
}

TEST_CASE("elgamalEncrypt simple encrypt 0, with nonce 1 then publickey is g_pow_p(2)")
{
    // Arrange
    const auto &nonce = ONE_MOD_Q();
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();
    auto g_pow_p_two = g_pow_p(*secret.toElementModP());

    CHECK((*publicKey == *g_pow_p_two));

    auto plaintext_exp_zero = g_pow_p(ZERO_MOD_P());
    CHECK((*plaintext_exp_zero == ONE_MOD_P())); // g^0 = 1

    // Act
    auto cipherText = elgamalEncrypt(0UL, nonce, *publicKey);

    CHECK((*publicKey == *cipherText->getData()));
    CHECK((const_cast<ElementModP &>(G()) == *cipherText->getPad()));

    // Assert
    auto decryptedData = pow_mod_p(*cipherText->getPad(), *secret.toElementModP());
    auto calculatedData = pow_mod_p(*publicKey, *nonce.toElementModP());
    CHECK((*decryptedData == *calculatedData));
    CHECK((*cipherText->getData() == *calculatedData));

    auto decrypted = cipherText->decrypt(secret, *publicKey);
    CHECK((0UL == decrypted));
}

TEST_CASE("elgamalEncrypt simple encrypt 0, with real nonce decrypts with secret")
{
    // Arrange
    unique_ptr<ElementModQ> nonce = rand_q();
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();

    CHECK((*publicKey < P()));

    // Act
    auto cipherText = elgamalEncrypt(0UL, *nonce, *publicKey);

    // Assert
    auto decrypted = cipherText->decrypt(secret, *publicKey);
    CHECK((0UL == decrypted));
}

TEST_CASE("elgamalEncrypt simple encrypt 0, with real nonce decrypts with nonce")
{
    // Arrange
    unique_ptr<ElementModQ> nonce = rand_q();
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();

    CHECK((*publicKey < P()));

    // Act
    auto cipherText = elgamalEncrypt(0UL, *nonce, *publicKey);

    // Assert
    auto decrypted = cipherText->decrypt(*publicKey, *nonce);
    CHECK((0UL == decrypted));
}

TEST_CASE("elgamalEncrypt simple encrypt 0 compared with elgamalEncrypt_with_precomputed decrypts "
          "with secret")
{
    // Arrange
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();

    CHECK((*publicKey < P()));

    // cause a two triples and a quad to be populated
    PrecomputeBufferContext::initialize(*keypair->getPublicKey(), 1);
    PrecomputeBufferContext::start();
    PrecomputeBufferContext::stop();

    // this function runs off to look in the precomputed values buffer and if
    // it finds what it needs the the returned class will contain those values
    auto precomputedValues = PrecomputeBufferContext::getPrecomputedSelection();

    CHECK(precomputedValues != nullptr);

    auto triple1 = precomputedValues->getPartialEncryption();

    // Act
    auto cipherText1 = elgamalEncrypt(0UL, *triple1->getSecret(), *publicKey);
    auto cipherText2 =
      elgamalEncrypt(0UL, *keypair->getPublicKey(), *precomputedValues->getPartialEncryption());

    CHECK((*cipherText1->getPad() == *cipherText2->getPad()));
    CHECK((*cipherText1->getData() == *cipherText2->getData()));

    // Assert
    auto decrypted1 = cipherText1->decrypt(secret, *publicKey);
    CHECK((0UL == decrypted1));
    auto decrypted2 = cipherText2->decrypt(secret, *publicKey);
    CHECK((0UL == decrypted2));
    PrecomputeBufferContext::clear();
}

TEST_CASE("elgamalEncrypt_with_precomputed simple encrypt 0 decrypts with secret")
{
    // Arrange
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();

    CHECK((*publicKey < P()));

    // cause a two triples and a quad to be populated
    PrecomputeBufferContext::initialize(*keypair->getPublicKey(), 1);
    PrecomputeBufferContext::start();
    PrecomputeBufferContext::stop();

    // this function runs off to look in the precomputed values buffer and if
    // it finds what it needs the the returned class will contain those values
    auto precomputedValues = PrecomputeBufferContext::getPrecomputedSelection();

    CHECK(precomputedValues != nullptr);

    auto triple1 = precomputedValues->getPartialEncryption();
    CHECK(triple1 != nullptr);

    // Act
    auto cipherText =
      elgamalEncrypt(0UL, *keypair->getPublicKey(), *precomputedValues->getPartialEncryption());

    // Assert
    auto decrypted = cipherText->decrypt(secret, *keypair->getPublicKey());
    CHECK((0UL == decrypted));
    PrecomputeBufferContext::clear();
}

TEST_CASE("elgamalEncrypt simple encrypt 1 decrypts with secret")
{
    const auto &nonce = ONE_MOD_Q();
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();

    CHECK((*publicKey < P()));

    auto elem = g_pow_p(nonce);
    CHECK((*elem == G())); // g^1 = g

    auto cipherText = elgamalEncrypt(1UL, nonce, *publicKey);
    CHECK(*elem == *cipherText->getPad());

    auto nonceDecrypted = cipherText->decrypt(*publicKey, nonce);
    CHECK(1UL == nonceDecrypted);

    auto secretDecrypted = cipherText->decrypt(secret, *publicKey);
    CHECK(1UL == secretDecrypted);
}

TEST_CASE("elgamalEncrypt encrypt 1 decrypts with secret")
{
    auto nonce = ElementModQ::fromHex(a_fixed_nonce);
    auto secret = ElementModQ::fromHex(a_fixed_secret);
    auto keypair = ElGamalKeyPair::fromSecret(*secret);
    auto *publicKey = keypair->getPublicKey();

    CHECK((*publicKey < P()));

    auto cipherText = elgamalEncrypt(1UL, *nonce, *publicKey);
    auto cipherText2 = elgamalEncrypt(1UL, *nonce, *publicKey);
    auto decrypted = cipherText->decrypt(*secret, *publicKey);
    CHECK(1UL == decrypted);
}

TEST_CASE("elgamalEncrypt encrypt 1 decrypts with nonce")
{
    auto nonce = ElementModQ::fromHex(a_fixed_nonce);
    auto secret = ElementModQ::fromHex(a_fixed_secret);
    auto keypair = ElGamalKeyPair::fromSecret(*secret);
    auto *publicKey = keypair->getPublicKey();

    CHECK((*publicKey < P()));

    auto cipherText = elgamalEncrypt(1UL, *nonce, *publicKey);
    auto cipherText2 = elgamalEncrypt(1UL, *nonce, *publicKey);
    auto decrypted = cipherText->decrypt(*publicKey, *nonce);
    CHECK(1UL == decrypted);
}

TEST_CASE("elgamalEncrypt vwith precomputed encrypt 1, decrypts with secret")
{
    //auto nonce = ElementModQ::fromHex(a_fixed_nonce);
    auto secret = ElementModQ::fromHex(a_fixed_secret);
    auto keypair = ElGamalKeyPair::fromSecret(*secret);
    auto *publicKey = keypair->getPublicKey();

    CHECK((*publicKey < P()));

    // cause a two triples and a quad to be populated
    PrecomputeBufferContext::initialize(*keypair->getPublicKey(), 1);
    PrecomputeBufferContext::start();
    PrecomputeBufferContext::stop();

    // this function runs off to look in the precomputed values buffer and if
    // it finds what it needs the the returned class will contain those values
    auto precomputedValues = PrecomputeBufferContext::getPrecomputedSelection();

    auto cipherText = elgamalEncrypt(1UL, *publicKey, *precomputedValues->getPartialEncryption());

    auto decrypted = cipherText->decrypt(*secret, *publicKey);
    CHECK(1UL == decrypted);
    PrecomputeBufferContext::clear();
}

TEST_CASE("elgamalAdd simple decrypts with secret")
{
    const auto &nonce = ONE_MOD_Q();
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();

    auto firstCiphertext = elgamalEncrypt(1UL, nonce, *publicKey);
    auto secondCiphertext = elgamalEncrypt(1UL, nonce, *publicKey);

    const vector<reference_wrapper<ElGamalCiphertext>> ciphertexts = {*firstCiphertext,
                                                                      *secondCiphertext};
    auto result = elgamalAdd(ciphertexts);

    auto decrypted = result->decrypt(secret, *publicKey);
    CHECK(2UL == decrypted);
}

TEST_CASE("HashedElGamalCiphertext encrypt and decrypt data")
{
    uint64_t qwords_to_use[4] = {0x0102030405060708, 0x090a0b0c0d0e0f10, 0x1112131415161718,
                                 0x191a1b1c1d1e1f20};

    uint8_t bytes_to_use[32] = {0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b,
                                0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
                                0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20};

    const auto nonce = make_unique<ElementModQ>(qwords_to_use);
    const auto cryptoExtendedBaseHash = make_unique<ElementModQ>(qwords_to_use);
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();
    vector<uint8_t> plaintext(bytes_to_use, bytes_to_use + sizeof(bytes_to_use));

    std::unique_ptr<HashedElGamalCiphertext> HEGResult = hashedElgamalEncrypt(
      plaintext, *nonce, HashPrefix::get_prefix_05(), *publicKey, *cryptoExtendedBaseHash, false);

    unique_ptr<ElementModQ> hash_of_HEG = HEGResult->crypto_hash();

    unique_ptr<ElementModP> pad = make_unique<ElementModP>(*HEGResult->getPad());
    vector<uint8_t> ciphertext = HEGResult->getData();
    vector<uint8_t> mac = HEGResult->getMac();

    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(pad), HEGResult->getData(), HEGResult->getMac());

    vector<uint8_t> new_plaintext = newHEG->decrypt(*publicKey, secret, HashPrefix::get_prefix_05(),
                                                    *cryptoExtendedBaseHash, false);

    CHECK(plaintext == new_plaintext);
}

TEST_CASE("HashedElGamalCiphertext encrypt and decrypt data with padding but on boundary")
{
    uint64_t qwords_to_use[4] = {0x0102030405060708, 0x090a0b0c0d0e0f10, 0x1112131415161718,
                                 0x191a1b1c1d1e1f20};

    uint8_t bytes_to_use[HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512] = {0x09};

    const auto nonce = make_unique<ElementModQ>(qwords_to_use);
    const auto cryptoExtendedBaseHash = make_unique<ElementModQ>(qwords_to_use);
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();
    vector<uint8_t> plaintext(bytes_to_use, bytes_to_use + sizeof(bytes_to_use));

    std::unique_ptr<HashedElGamalCiphertext> HEGResult = hashedElgamalEncrypt(
      plaintext, *nonce, HashPrefix::get_prefix_05(), *publicKey, *cryptoExtendedBaseHash,
      HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512, false, true);

    unique_ptr<ElementModQ> hash_of_HEG = HEGResult->crypto_hash();

    unique_ptr<ElementModP> pad = make_unique<ElementModP>(*HEGResult->getPad());
    vector<uint8_t> ciphertext = HEGResult->getData();
    vector<uint8_t> mac = HEGResult->getMac();
    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(pad), ciphertext, mac);

    CHECK(ciphertext.size() == (HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512 + sizeof(uint16_t)));
    vector<uint8_t> new_plaintext = newHEG->decrypt(*publicKey, secret, HashPrefix::get_prefix_05(),
                                                    *cryptoExtendedBaseHash, true);

    CHECK(plaintext == new_plaintext);
}

TEST_CASE("HashedElGamalCiphertext encrypt and decrypt string data with padding")
{
    uint64_t qwords_to_use[4] = {0x0102030405060708, 0x090a0b0c0d0e0f10, 0x1112131415161718,
                                 0x191a1b1c1d1e1f20};

    uint8_t bytes_to_use[HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512] = {0x09};
    const auto nonce = make_unique<ElementModQ>(qwords_to_use);
    const auto cryptoExtendedBaseHash = make_unique<ElementModQ>(qwords_to_use);
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();

    std::wstring plaintext_string(L"ElectionGuard Rocks!");
    vector<uint8_t> plaintext((uint8_t *)&plaintext_string.front(),
                              +(uint8_t *)&plaintext_string.front() +
                                (plaintext_string.size() * 2));

    auto HEGResult = hashedElgamalEncrypt(
      plaintext, *nonce, HashPrefix::get_prefix_05(), *publicKey, *cryptoExtendedBaseHash,
      HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512, false, true);

    unique_ptr<ElementModQ> hash_of_HEG = HEGResult->crypto_hash();

    auto pad = HEGResult->getPad();
    unique_ptr<ElementModP> p_pad = make_unique<ElementModP>(*pad);
    auto ciphertext = HEGResult->getData();
    auto mac = HEGResult->getMac();

    CHECK(ciphertext.size() == (HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512 + sizeof(uint16_t)));

    // now lets decrypt
    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(p_pad), HEGResult->getData(), HEGResult->getMac());

    vector<uint8_t> new_plaintext = newHEG->decrypt(*publicKey, secret, HashPrefix::get_prefix_05(),
                                                    *cryptoExtendedBaseHash, true);

    CHECK(plaintext == new_plaintext);
}

TEST_CASE("HashedElGamalCiphertext encrypt and decrypt string data with padding and truncate")
{
    uint64_t qwords_to_use[4] = {0x0102030405060708, 0x090a0b0c0d0e0f10, 0x1112131415161718,
                                 0x191a1b1c1d1e1f20};

    uint8_t bytes_to_use[HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512 + 20] = {0x1a};
    uint8_t truncated_bytes[HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512] = {0x1a};

    const auto nonce = make_unique<ElementModQ>(qwords_to_use);
    const auto cryptoExtendedBaseHash = make_unique<ElementModQ>(qwords_to_use);
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();

    vector<uint8_t> plaintext(bytes_to_use, bytes_to_use + sizeof(bytes_to_use));

    auto HEGResult = hashedElgamalEncrypt(
      plaintext, *nonce, HashPrefix::get_prefix_05(), *publicKey, *cryptoExtendedBaseHash,
      HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512, true, true);

    unique_ptr<ElementModQ> hash_of_HEG = HEGResult->crypto_hash();

    auto pad = HEGResult->getPad();
    unique_ptr<ElementModP> p_pad = make_unique<ElementModP>(*pad);
    auto ciphertext = HEGResult->getData();
    auto mac = HEGResult->getMac();

    CHECK(ciphertext.size() == (HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512 + sizeof(uint16_t)));

    // now lets decrypt
    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(p_pad), HEGResult->getData(), HEGResult->getMac());

    vector<uint8_t> new_plaintext = newHEG->decrypt(*publicKey, secret, HashPrefix::get_prefix_05(),
                                                    *cryptoExtendedBaseHash, true);

    vector<uint8_t> plaintext_truncated(truncated_bytes, truncated_bytes + sizeof(truncated_bytes));

    CHECK(plaintext_truncated == new_plaintext);
}

TEST_CASE("HashedElGamalCiphertext encrypt and decrypt no data")
{
    uint64_t qwords_to_use[4] = {0x0102030405060708, 0x090a0b0c0d0e0f10, 0x1112131415161718,
                                 0x191a1b1c1d1e1f20};

    const auto nonce = make_unique<ElementModQ>(qwords_to_use);
    const auto cryptoExtendedBaseHash = make_unique<ElementModQ>(qwords_to_use);
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();

    vector<uint8_t> plaintext; // no data in plaintext
    CHECK(plaintext.size() == 0);

    auto HEGResult = hashedElgamalEncrypt(
      plaintext, *nonce, HashPrefix::get_prefix_05(), *publicKey, *cryptoExtendedBaseHash,
      HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512, false, true);

    unique_ptr<ElementModQ> hash_of_HEG = HEGResult->crypto_hash();

    auto pad = HEGResult->getPad();
    unique_ptr<ElementModP> p_pad = make_unique<ElementModP>(*pad);
    auto ciphertext = HEGResult->getData();
    auto mac = HEGResult->getMac();
    CHECK(ciphertext.size() == (HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512 +
                                sizeof(uint16_t))); // two more bytes than max_len input to encrypt

    // now lets decrypt
    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(p_pad), HEGResult->getData(), HEGResult->getMac());

    vector<uint8_t> new_plaintext = newHEG->decrypt(*publicKey, secret, HashPrefix::get_prefix_05(),
                                                    *cryptoExtendedBaseHash, true);
    CHECK(new_plaintext.size() == 0);

    CHECK(plaintext == new_plaintext);
}

TEST_CASE("HashedElGamalCiphertext encrypt and decrypt data failure different nonce")
{
    uint64_t qwords_to_use[4] = {0x0102030405060708, 0x090a0b0c0d0e0f10, 0x1112131415161718,
                                 0x191a1b1c1d1e1f20};
    // one bit difference in the nonce
    uint64_t different_qwords_to_use[4] = {0x0202030405060708, 0x090a0b0c0d0e0f10,
                                           0x1112131415161718, 0x191a1b1c1d1e1f20};
    uint8_t bytes_to_use[32] = {0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b,
                                0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
                                0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20};

    const auto nonce = make_unique<ElementModQ>(qwords_to_use);
    const auto different_secret = make_unique<ElementModQ>(different_qwords_to_use);
    const auto cryptoExtendedBaseHash = make_unique<ElementModQ>(qwords_to_use);
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();
    vector<uint8_t> plaintext(bytes_to_use, bytes_to_use + sizeof(bytes_to_use));
    bool decrypt_failed = false;

    std::unique_ptr<HashedElGamalCiphertext> HEGResult = hashedElgamalEncrypt(
      plaintext, *nonce, HashPrefix::get_prefix_05(), *publicKey, *cryptoExtendedBaseHash, false);

    unique_ptr<ElementModP> pad = make_unique<ElementModP>(*HEGResult->getPad());
    vector<uint8_t> ciphertext = HEGResult->getData();
    vector<uint8_t> mac = HEGResult->getMac();

    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(pad), HEGResult->getData(), HEGResult->getMac());

    try {
        vector<uint8_t> new_plaintext =
          newHEG->decrypt(*publicKey, *different_secret, HashPrefix::get_prefix_05(),
                          *cryptoExtendedBaseHash, false);
    } catch (std::runtime_error &e) {
        decrypt_failed = true;
    }

    CHECK(decrypt_failed);
}

TEST_CASE("HashedElGamalCiphertext encrypt and decrypt data failure - tampered with ciphertext")
{
    uint64_t qwords_to_use[4] = {0x0102030405060708, 0x090a0b0c0d0e0f10, 0x1112131415161718,
                                 0x191a1b1c1d1e1f20};
    uint8_t bytes_to_use[32] = {0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b,
                                0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
                                0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20};

    const auto nonce = make_unique<ElementModQ>(qwords_to_use);
    const auto cryptoExtendedBaseHash = make_unique<ElementModQ>(qwords_to_use);
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();
    vector<uint8_t> plaintext(bytes_to_use, bytes_to_use + sizeof(bytes_to_use));
    bool decrypt_failed = false;

    std::unique_ptr<HashedElGamalCiphertext> HEGResult = hashedElgamalEncrypt(
      plaintext, *nonce, HashPrefix::get_prefix_05(), *publicKey, *cryptoExtendedBaseHash, false);

    unique_ptr<ElementModP> pad = make_unique<ElementModP>(*HEGResult->getPad());
    vector<uint8_t> ciphertext = HEGResult->getData();
    vector<uint8_t> mac = HEGResult->getMac();

    // change a byte of ciphertext
    if (ciphertext[ciphertext.size() / 2] == 0x00) {
        ciphertext[ciphertext.size() / 2] = 0xff;
    } else {
        ciphertext[ciphertext.size() / 2] = 0x00;
    }

    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(pad), ciphertext, HEGResult->getMac());

    try {
        vector<uint8_t> new_plaintext = newHEG->decrypt(
          *publicKey, secret, HashPrefix::get_prefix_05(), *cryptoExtendedBaseHash, false);
    } catch (std::runtime_error &e) {
        decrypt_failed = true;
    }

    CHECK(decrypt_failed);
}

TEST_CASE("HashedElGamalCiphertext encrypt failure length cases")
{
    uint64_t qwords_to_use[4] = {0x0102030405060708, 0x090a0b0c0d0e0f10, 0x1112131415161718,
                                 0x191a1b1c1d1e1f20};
    uint8_t bytes_to_use[32] = {0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b,
                                0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
                                0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20};
    uint8_t longer_bytes_to_use[513] = {0x01};
    const auto nonce = make_unique<ElementModQ>(qwords_to_use);
    const auto cryptoExtendedBaseHash = make_unique<ElementModQ>(qwords_to_use);
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();
    vector<uint8_t> plaintext(bytes_to_use, bytes_to_use + sizeof(bytes_to_use));
    vector<uint8_t> longer_plaintext(longer_bytes_to_use,
                                     longer_bytes_to_use + sizeof(longer_bytes_to_use));
    bool encrypt_longer_plaintext_failed = false;
    bool encrypt_no_pad_not_block_length_failed = false;

    try {
        std::unique_ptr<HashedElGamalCiphertext> HEGResult = hashedElgamalEncrypt(
          longer_plaintext, *nonce, HashPrefix::get_prefix_05(), *publicKey,
          *cryptoExtendedBaseHash, HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512, false, false);
    } catch (std::invalid_argument &e) {
        encrypt_longer_plaintext_failed = true;
    }
    CHECK(encrypt_longer_plaintext_failed);

    try {
        std::unique_ptr<HashedElGamalCiphertext> HEGResult =
          hashedElgamalEncrypt(longer_plaintext, *nonce, HashPrefix::get_prefix_05(), *publicKey,
                               *cryptoExtendedBaseHash, false);
    } catch (std::invalid_argument &e) {
        encrypt_no_pad_not_block_length_failed = true;
    }
    CHECK(encrypt_no_pad_not_block_length_failed);
}

TEST_CASE("HashedElGamalCiphertext encrypt and decrypt string data with padding and precompute")
{
    uint64_t qwords_to_use[4] = {0x0102030405060708, 0x090a0b0c0d0e0f10, 0x1112131415161718,
                                 0x191a1b1c1d1e1f20};

    uint8_t bytes_to_use[32] = {0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b,
                                0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
                                0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20};

    const auto nonce = make_unique<ElementModQ>(qwords_to_use);
    const auto cryptoExtendedBaseHash = make_unique<ElementModQ>(qwords_to_use);
    const auto &secret = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(secret, false);
    auto *publicKey = keypair->getPublicKey();

    std::wstring plaintext_string(L"ElectionGuard Rocks!");
    vector<uint8_t> plaintext((uint8_t *)&plaintext_string.front(),
                              +(uint8_t *)&plaintext_string.front() +
                                (plaintext_string.size() * 2));

    // cause precomputed entries that will be used by the selection
    // encryptions, that should be more than enough and on teardown
    // the rest will be removed.
    PrecomputeBufferContext::initialize(*keypair->getPublicKey(), 3);
    PrecomputeBufferContext::start();
    PrecomputeBufferContext::stop();

    auto HEGResult = hashedElgamalEncrypt(
      plaintext, *nonce, HashPrefix::get_prefix_05(), *publicKey, *cryptoExtendedBaseHash,
      HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512, true, true);

    auto pad = HEGResult->getPad();
    unique_ptr<ElementModP> p_pad = make_unique<ElementModP>(*pad);
    auto ciphertext = HEGResult->getData();
    auto mac = HEGResult->getMac();

    CHECK(ciphertext.size() == (HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512 + sizeof(uint16_t)));

    // now lets decrypt
    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(p_pad), HEGResult->getData(), HEGResult->getMac());

    vector<uint8_t> new_plaintext = newHEG->decrypt(*publicKey, secret, HashPrefix::get_prefix_05(),
                                                    *cryptoExtendedBaseHash, true);

    CHECK(plaintext == new_plaintext);
    PrecomputeBufferContext::clear();
}

TEST_CASE("HashedElGamalCiphertext encrypt and decrypt with hard coded data for interop")
{
    unique_ptr<ElementModQ> secret =
      ElementModQ::fromHex("094CDA6CEB3332D62438B6D37BBA774D23C420FA019368671AD330AD50456603");
    auto keypair = ElGamalKeyPair::fromSecret(*secret, false);
    auto *publicKey = keypair->getPublicKey();
    string plaintext_string("{\"error\":\"overvote\",\"error_data\":[\"john-adams-selection\","
                            "\"benjamin-franklin-selection\",\"write-in-selection\"],\"write_ins\""
                            ":{\"write-in-selection\":\"Susan B. Anthony\"}}");

    vector<uint8_t> plaintext(&plaintext_string.front(),
                              &plaintext_string.front() + plaintext_string.size());
    const auto nonce =
      ElementModQ::fromHex("46B6532A1AD7B2AFCDDAA30EEE464884883804B46058DB38E76FCDC79EE5C702");
    const auto cryptoExtendedBaseHash =
      ElementModQ::fromHex("6E418518C6C244CA58399C0F47A9C761BAE7B876F8F5360D8D15FCFF26A42BAA");

    std::unique_ptr<HashedElGamalCiphertext> HEGResult = hashedElgamalEncrypt(
      plaintext, *nonce, HashPrefix::get_prefix_05(), *publicKey, *cryptoExtendedBaseHash,
      HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512, true, false);

    unique_ptr<ElementModQ> hash_of_HEG = HEGResult->crypto_hash();

    unique_ptr<ElementModP> pad = make_unique<ElementModP>(*HEGResult->getPad());
    vector<uint8_t> data = HEGResult->getData();
    vector<uint8_t> mac = HEGResult->getMac();

    string hard_coded_pad_string(
      "C102BAB526517D74FE5D5C249E"
      "7F422993C0306C40A9398FBAD01A0D3547B50BDFD77C6EFC187C7B1FD7918A0B3C2A2FB0A3776A7240F9A"
      "75410569379B3D16877B547F52E79542C1129F6E369F2D006D0A1AA3919F0228CA07F5C9A4DFD1118A606"
      "AA4B7000F9EDC65963F130663FD4F7246F7CFE7A38F1E1DC9BC0698CAB881DCD5A75E6D7165B329C28D80"
      "B719D7A2ED50031A2448A4528275FF161F541CFE304A28CBE7193A4BF8676B2D4F2DE68F175C5B4BFD14B"
      "4B1D9868D00E0BD95B6491C96460159DEABF85239B10A7C86B3D975EF58BBF833C6ABFFF223DAF78C1AE4"
      "C6F64D084C4118F3B5A2618628FA18852BAB55DCE95C04FFCBBAF582D75C7B8B830424C74A8F8EACD1543"
      "00FD67CF753EE14FCE94DDED95F1DD2C1386D92B3FF03A9D6EDEE0F67EC80C72E6425B4EA1C17D7B9CC5B"
      "2165905373A4E304496462CE2BA077F195302A39C52F0077CA682BC718074F928040D1A36F585AC187A74"
      "1F51C843C5ED88BC5FB8B86ED96C42BCF84EDF833489D7D3AC407C6D0740CC94BA1D5B885EB430CE8C601"
      "7F8660A6C72F4378BF133AA663DBA36CAB967AAC0F7738478110ECEABAE3E914CB7A796C5394F7DF15094"
      "0BEA43264765B34851ADE4E5F1F213C25DCF66D35BE92611555D8C05ACFDF1AC5CA82B7D7F0D9BE49596F"
      "8B7F3269D9887F40B4BAB5C3D2BA7049B6D2119C3D0D01501836203412869E0");
    CHECK(pad->toHex() == hard_coded_pad_string);

    string hard_coded_data_string(
      "A3C7CDFAEB85EE51ECC8A832AD02F72035A24A3B1514DC5AD68A636122AE22016E6377923191496999EBF1CAA13A"
      "8CBAD60FBE8B44C59E9DFDBD47241933864C7A25FB9ED522B367CEBA765C3FD15D127D3D16199AAC20048FFD4180"
      "6B112F5B288A68B3659AE48AF1F6D2DC164AA94F6CCD1223427194530B950280B6D5666A812A476A9C484B02FF88"
      "2182014D5DF3683BF805A92CA0A574040BEF1649C4100428AD8B49F191E2808343A5E2D3DA8B23BFFA9808D4A33E"
      "F4DD501F0E31B73C2334DD90A56881355CEEF5A5CB8E5CAF710C94B070CB33E77564181587EAD374C74421F32DF4"
      "12C1D90E4A07E12ABE096FDBAE0981716505B014C6A3E9C1C8F6C88C34389FB6C2F042AF4BA394C47560B8179AFB"
      "C655628AD7E7D0DF24E7591DEA47DF86762029BD4E046384B48863D5A5E98FCCA0123CDD4439E2A37C9CE8218184"
      "D1EB4C88B90DB1A9C54A8F0568B87926AD6437C1196622B5E85E7A6F826A1CE9D53F82C3BA89B0B64D4C035DAB5F"
      "F442A656F3E3EC5817B9ACE0B5C0A9896FB0B6F9BF62C78080E022DE152C7EB24D78DA2C4B88D07DC6D3C35FA984"
      "03B52962530370D7A99B93C159265A55A9E27D0321B2B6377F6DB8F2F391AF2B27850CB2C2D694F87C796E9D9857"
      "DA0F11882C45D3A661EEAA765E94C12BDC90A83A7CAE3D3109A29CDE251EC02C96562CC985EBFB2AA0BC868B1A6F"
      "8B1CBB57ABFD");
    CHECK(bytes_to_hex(data) == hard_coded_data_string);

    string hard_coded_mac_string(
      "74C8D7660225A4146D05F6CDB3751B5A2502240CD6B0978A52A6344D61A20219");
    CHECK(bytes_to_hex(mac) == hard_coded_mac_string);

    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(pad), HEGResult->getData(), HEGResult->getMac());

    vector<uint8_t> new_plaintext = newHEG->decrypt(
      *publicKey, *secret, HashPrefix::get_prefix_05(), *cryptoExtendedBaseHash, true);

    CHECK(plaintext == new_plaintext);
}
