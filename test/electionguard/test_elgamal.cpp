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
      "C499CD74FA9A1CDC3EC683ACA36C5D191C388E7FC6C7A3EB1C73605F6C0F114E51F2935BBD7EF6F597376B28A08B"
      "46872538E2338ACA15AD8561EC278EF5A5508D3CCA0EE118B617D5AC1BE3486959945BBCF2EEE71797DE100FEECB"
      "3E3709EC8448BC5E2FEF88E5F004075A822D0C4D12F22A474FE3A898FE38FF7D08CCCAACC08D698F2B1B77F32A3A"
      "5DEB71FBD169D210A426ED385ADE3C544B9CF6AFC4DE4AA2144EE891030487C72ED5A457437CDAC36FAB9888AB85"
      "84E290E7FEDBBACA520C56FDB27FEFA937A3019750DF8AE926B13C70EB8280D3095E74EC87DD1C8232FDE275D646"
      "69CE8CD9C13467F0AA0BA7DF85E32263196798B4E85A3D946F296EB55ACA5852101422D4EE48CD501994B98C1057"
      "23A44F8A6C082AE4C6F819700EB139211BF368F2C1A20BBB82C65242FA097ABBE888EFF54566F21BF1A2E3635E94"
      "A87EAEE8DC25638523190CCFFB4B003CF985C23E5A395CF5A0C6EF356342A901A3D476123784D3D9598C0437824E"
      "B30D280E683315FC8E734B6ECDEF4DB080427C13BE16A9D7011A21A70BAA60A2F8EC8EE524110C7FE812CC1F4A8F"
      "917CB41C25343647B42C73EC513340EAADD44BA33BA1F0E3AE4D7D98D8562B6F2E1FCC14ECF420C92AA5DDA73FDB"
      "DF3FBEF7A9AFE38F3E2853103DEE4624983BDA08FB64F156BCD75C38DA7B1BE1B180E75482DE6E0EB43D3B2A3DB4"
      "BB3727881CA8");
    CHECK(pad->toHex() == hard_coded_pad_string);

    string hard_coded_data_string(
      "17BED71AD217DBD7863752C088FD6BA2B5064097C28B8C58BBB88FFD939B0BD26AA39DA239E4E6E8AA60A9761A73"
      "179A7D45C57AE234D31642F05AD2A940E6B45B3D3D55211A25A6AB8068085C1492810E4631F5BC38D3555B415D2D"
      "A83E48251122C1F642FA9AA2484F4A4A426C7C5D8A65FBF3D45AA004FE853640D22EBF5014C9E8131F24D0B4B898"
      "E75B5EFCA4495CE85DB8353CAB499F16489F90C2F055BF3B16DEB483634B7E5E3B961CE5279FB32FC5D279B28AED"
      "D5175BD218C77D3F65D54987E17CA3B5F4AECB5ADA688652E423F65794AA3584D1F100F96F77652474686D03F780"
      "85EC6B1858A96C2F8A88B5221417B4C0F7A6108DEA2B38EE8379B17032148E6CCE53C3B29555D803EA621CDC2D74"
      "BF591E221CC81C6D113C03061279E61A73063EEB9EE8F3378ED8CB7F9F8152C60B8DBB048A50AA084C663B1A6160"
      "3BDA31FD37545509B1A969B0717D245C33D56444CCF26BB4B6D350A9C777AB5289BA46E347107042B2BE37D87B11"
      "41D7B5FAE8967AF3CDE11D52D732DCCFB4E9DC432776761EE70C139C24A256503E3C563CCBC616ACD68615D1BD13"
      "1478A6C5AF4A590C9943207D20FAA525FCCEAB8D95F74473F0D5037ECAC8D9D963668A108413D5C40AC130A6BDB7"
      "0ADBDBE89A9C78CC45A2E4F29341579DDEA8132B51CE7CA31DCCAD699C8050454F7FA130DF5C854D90D6D7D127B0"
      "353C401A7C63");
    CHECK(bytes_to_hex(data) == hard_coded_data_string);

    string hard_coded_mac_string(
      "AE1AC6404D0CD0D921EEB786B78D31D612E00A8708A87C72D81D1F83F3CBC919");
    CHECK(bytes_to_hex(mac) == hard_coded_mac_string);

    unique_ptr<HashedElGamalCiphertext> newHEG =
      make_unique<HashedElGamalCiphertext>(move(pad), HEGResult->getData(), HEGResult->getMac());

    vector<uint8_t> new_plaintext =
      newHEG->decrypt(*publicKey, *secret, HashPrefix::get_prefix_contest_data_secret(),
                      *cryptoExtendedBaseHash, true);

    CHECK(plaintext == new_plaintext);
}
