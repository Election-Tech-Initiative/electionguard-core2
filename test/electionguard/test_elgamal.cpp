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

TEST_CASE("elgamalEncrypt encrypt 1 decrypts with nonce for E.G. 1.0 Compatible Elections (Base G)")
{
    auto nonce = ElementModQ::fromHex(a_fixed_nonce);
    auto secret = ElementModQ::fromHex(a_fixed_secret);
    auto keypair = ElGamalKeyPair::fromSecret(*secret);
    auto *publicKey = keypair->getPublicKey();
    auto encryptionBase = G(); // *publicKey;

    CHECK((*publicKey < P()));

    auto cipherText = elgamalEncrypt(1UL, *nonce, *publicKey, encryptionBase);
    auto cipherText2 = elgamalEncrypt(1UL, *nonce, *publicKey, encryptionBase);
    auto decrypted = cipherText->decrypt(*publicKey, *nonce, encryptionBase);
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

    std::unique_ptr<HashedElGamalCiphertext> HEGResult =
      hashedElgamalEncrypt(plaintext, *nonce, HashPrefix::get_prefix_contest_data_secret(),
                           *publicKey, *cryptoExtendedBaseHash, false);

    unique_ptr<ElementModQ> hash_of_HEG = HEGResult->crypto_hash();

    unique_ptr<ElementModP> pad = make_unique<ElementModP>(*HEGResult->getPad());
    vector<uint8_t> ciphertext = HEGResult->getData();
    vector<uint8_t> mac = HEGResult->getMac();

    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(pad), HEGResult->getData(), HEGResult->getMac());

    vector<uint8_t> new_plaintext =
      newHEG->decrypt(*publicKey, secret, HashPrefix::get_prefix_contest_data_secret(),
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
      plaintext, *nonce, HashPrefix::get_prefix_contest_data_secret(), *publicKey,
      *cryptoExtendedBaseHash, HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512, false, true);

    unique_ptr<ElementModQ> hash_of_HEG = HEGResult->crypto_hash();

    unique_ptr<ElementModP> pad = make_unique<ElementModP>(*HEGResult->getPad());
    vector<uint8_t> ciphertext = HEGResult->getData();
    vector<uint8_t> mac = HEGResult->getMac();
    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(pad), ciphertext, mac);

    CHECK(ciphertext.size() == (HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512 + sizeof(uint16_t)));
    vector<uint8_t> new_plaintext =
      newHEG->decrypt(*publicKey, secret, HashPrefix::get_prefix_contest_data_secret(),
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
      plaintext, *nonce, HashPrefix::get_prefix_contest_data_secret(), *publicKey,
      *cryptoExtendedBaseHash, HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512, false, true);

    unique_ptr<ElementModQ> hash_of_HEG = HEGResult->crypto_hash();

    auto pad = HEGResult->getPad();
    unique_ptr<ElementModP> p_pad = make_unique<ElementModP>(*pad);
    auto ciphertext = HEGResult->getData();
    auto mac = HEGResult->getMac();

    CHECK(ciphertext.size() == (HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512 + sizeof(uint16_t)));

    // now lets decrypt
    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(p_pad), HEGResult->getData(), HEGResult->getMac());

    vector<uint8_t> new_plaintext =
      newHEG->decrypt(*publicKey, secret, HashPrefix::get_prefix_contest_data_secret(),
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
      plaintext, *nonce, HashPrefix::get_prefix_contest_data_secret(), *publicKey,
      *cryptoExtendedBaseHash, HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512, true, true);

    unique_ptr<ElementModQ> hash_of_HEG = HEGResult->crypto_hash();

    auto pad = HEGResult->getPad();
    unique_ptr<ElementModP> p_pad = make_unique<ElementModP>(*pad);
    auto ciphertext = HEGResult->getData();
    auto mac = HEGResult->getMac();

    CHECK(ciphertext.size() == (HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512 + sizeof(uint16_t)));

    // now lets decrypt
    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(p_pad), HEGResult->getData(), HEGResult->getMac());

    vector<uint8_t> new_plaintext =
      newHEG->decrypt(*publicKey, secret, HashPrefix::get_prefix_contest_data_secret(),
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
      plaintext, *nonce, HashPrefix::get_prefix_contest_data_secret(), *publicKey,
      *cryptoExtendedBaseHash, HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512, false, true);

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

    vector<uint8_t> new_plaintext =
      newHEG->decrypt(*publicKey, secret, HashPrefix::get_prefix_contest_data_secret(),
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

    std::unique_ptr<HashedElGamalCiphertext> HEGResult =
      hashedElgamalEncrypt(plaintext, *nonce, HashPrefix::get_prefix_contest_data_secret(),
                           *publicKey, *cryptoExtendedBaseHash, false);

    unique_ptr<ElementModP> pad = make_unique<ElementModP>(*HEGResult->getPad());
    vector<uint8_t> ciphertext = HEGResult->getData();
    vector<uint8_t> mac = HEGResult->getMac();

    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(pad), HEGResult->getData(), HEGResult->getMac());

    try {
        vector<uint8_t> new_plaintext = newHEG->decrypt(
          *publicKey, *different_secret, HashPrefix::get_prefix_contest_data_secret(),
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

    std::unique_ptr<HashedElGamalCiphertext> HEGResult =
      hashedElgamalEncrypt(plaintext, *nonce, HashPrefix::get_prefix_contest_data_secret(),
                           *publicKey, *cryptoExtendedBaseHash, false);

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
        vector<uint8_t> new_plaintext =
          newHEG->decrypt(*publicKey, secret, HashPrefix::get_prefix_contest_data_secret(),
                          *cryptoExtendedBaseHash, false);
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
          longer_plaintext, *nonce, HashPrefix::get_prefix_contest_data_secret(), *publicKey,
          *cryptoExtendedBaseHash, HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512, false, false);
    } catch (std::invalid_argument &e) {
        encrypt_longer_plaintext_failed = true;
    }
    CHECK(encrypt_longer_plaintext_failed);

    try {
        std::unique_ptr<HashedElGamalCiphertext> HEGResult = hashedElgamalEncrypt(
          longer_plaintext, *nonce, HashPrefix::get_prefix_contest_data_secret(), *publicKey,
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
      plaintext, *nonce, HashPrefix::get_prefix_contest_data_secret(), *publicKey,
      *cryptoExtendedBaseHash, HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512, true, true);

    auto pad = HEGResult->getPad();
    unique_ptr<ElementModP> p_pad = make_unique<ElementModP>(*pad);
    auto ciphertext = HEGResult->getData();
    auto mac = HEGResult->getMac();

    CHECK(ciphertext.size() == (HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512 + sizeof(uint16_t)));

    // now lets decrypt
    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(p_pad), HEGResult->getData(), HEGResult->getMac());

    vector<uint8_t> new_plaintext =
      newHEG->decrypt(*publicKey, secret, HashPrefix::get_prefix_contest_data_secret(),
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
      plaintext, *nonce, HashPrefix::get_prefix_contest_data_secret(), *publicKey,
      *cryptoExtendedBaseHash, HASHED_CIPHERTEXT_PADDED_DATA_SIZE::BYTES_512, true, false);

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
      "4E537F58F45BF5C134912F08CFE638826545EB174ECCF15A744A45FDD09237827AFF896AD25A6DF2D7816F4A309D"
      "2E8D84DBE84A39386E199284B0F5D7F7B018794CF3ED9FFD87F7D8AD5E59EF78421A9CCFFCCB41301A3127C74361"
      "8644E66F29B65E1ADF9D723E683EF5C30FC4F322CF4DBBBDE27A0394F4D51C27CC869ADCCC7B4E90DF9FDA1E52DA"
      "8A7F89ECF52ECF174C6E10525B00B999509F0A1C94AA79100C8CB51BFFBC357B0E26A41CB40DD176949E45B19750"
      "3475B262DC279FEFD4938CD606A720C272CDBFA87D2D9AD9B8008AA75ED5EE89260882268981164BE549F289C961"
      "AF396E277009B8B9BCBFBAE86B93091EE84AAA1F84836E8602F40A0429C821CC7BFA896153D9732BB872D84163BB"
      "B18238141A5214A837023B3028CBE9158FCDD043C9F8205D7764BAE0F32089C3B79911615D9590543CDE7771ED55"
      "ABAF04759EBE09784C421B14F2A6A15E869F8CB6A3D86C11C256FEA5EB54AE671E5D532FD8471A0AEB5D34129D71"
      "5AF4DE3D1FBE745617DF4879A96DB8B335529ED8161AE3CF8FBF832ABFA032267879AE9803E20F8400CD76B78C31"
      "A2A8A961AF9ECEE7147C4B9BCDF245001996B87B5864140B8F5C082515B3BE8FC8B6E4BDD0D2DA4BE5387413C32B"
      "7A73621B1BF83095D80123F00F58F30BBE672DBEFD3F45DD064401392FBB2851F98A7A87BE90217AA256D2090C41"
      "D376B3DC9DA1");
    CHECK(bytes_to_hex(data) == hard_coded_data_string);

    string hard_coded_mac_string(
      "EDC26AA36514C40290F1B78D33A09685A7DEA344C93B71FCFC5C3B135BD0BDDD");
    CHECK(bytes_to_hex(mac) == hard_coded_mac_string);

    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(pad), HEGResult->getData(), HEGResult->getMac());

    vector<uint8_t> new_plaintext =
      newHEG->decrypt(*publicKey, *secret, HashPrefix::get_prefix_contest_data_secret(),
                      *cryptoExtendedBaseHash, true);

    CHECK(plaintext == new_plaintext);
}
