#include "../../src/electionguard/log.hpp"

#include <doctest/doctest.h>
#include <electionguard/chaum_pedersen.hpp>
#include <electionguard/elgamal.hpp>
#include <electionguard/group.hpp>
#include <iostream>
#include <string>

using namespace electionguard;
using namespace std;

class DisjunctiveChaumPedersenProofHarness : DisjunctiveChaumPedersenProof
{
  public:
    static unique_ptr<DisjunctiveChaumPedersenProof> make_zero(const ElGamalCiphertext &message,
                                                               const ElementModQ &r,
                                                               const ElementModP &k,
                                                               const ElementModQ &q)
    {
        return DisjunctiveChaumPedersenProof::make_zero(message, r, k, q);
    }
    static unique_ptr<DisjunctiveChaumPedersenProof>
    make_zero(const ElGamalCiphertext &message, const ElementModQ &r, const ElementModP &k,
              const ElementModQ &q, const ElementModQ &seed)
    {
        return DisjunctiveChaumPedersenProof::make_zero(message, r, k, q, seed);
    }
    static unique_ptr<DisjunctiveChaumPedersenProof> make_one(const ElGamalCiphertext &message,
                                                              const ElementModQ &r,
                                                              const ElementModP &k,
                                                              const ElementModQ &q)
    {
        return DisjunctiveChaumPedersenProof::make_one(message, r, k, q);
    }
    static unique_ptr<DisjunctiveChaumPedersenProof>
    make_one(const ElGamalCiphertext &message, const ElementModQ &r, const ElementModP &k,
             const ElementModQ &q, const ElementModQ &seed)
    {
        return DisjunctiveChaumPedersenProof::make_one(message, r, k, q, seed);
    }

    static unique_ptr<DisjunctiveChaumPedersenProof>
    make_zero(const ElGamalCiphertext &message, const PrecomputedSelection &precomputedValues,
              const ElementModP &k, const ElementModQ &q)
    {
        return DisjunctiveChaumPedersenProof::make_zero(message, precomputedValues, k, q);
    }

    static unique_ptr<DisjunctiveChaumPedersenProof>
    make_one(const ElGamalCiphertext &message, const PrecomputedSelection &precomputedValues,
             const ElementModP &k, const ElementModQ &q)
    {
        return DisjunctiveChaumPedersenProof::make_one(message, precomputedValues, k, q);
    }
};

TEST_CASE("Disjunctive CP Proof simple valid inputs generate valid proofs")
{
    // Arrange
    const auto &nonce = ONE_MOD_Q();
    const auto &seed = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q(), false);

    auto firstMessage = elgamalEncrypt(0UL, nonce, *keypair->getPublicKey());
    auto secondMessage = elgamalEncrypt(1UL, nonce, *keypair->getPublicKey());

    // Act
    auto firstMessageZeroProof = DisjunctiveChaumPedersenProofHarness::make_zero(
      *firstMessage, nonce, *keypair->getPublicKey(), ONE_MOD_Q());
    auto firstMessageOneProof = DisjunctiveChaumPedersenProofHarness::make_one(
      *firstMessage, nonce, *keypair->getPublicKey(), ONE_MOD_Q());

    auto secondMessageZeroProof = DisjunctiveChaumPedersenProofHarness::make_zero(
      *secondMessage, nonce, *keypair->getPublicKey(), ONE_MOD_Q());
    auto secondMessageOneProof = DisjunctiveChaumPedersenProofHarness::make_one(
      *secondMessage, nonce, *keypair->getPublicKey(), ONE_MOD_Q());

    // Assert
    CHECK(firstMessageZeroProof->isValid(*firstMessage, *keypair->getPublicKey(), ONE_MOD_Q()) ==
          true);
    CHECK(firstMessageOneProof->isValid(*firstMessage, *keypair->getPublicKey(), ONE_MOD_Q()) ==
          false);
    CHECK(secondMessageZeroProof->isValid(*secondMessage, *keypair->getPublicKey(), ONE_MOD_Q()) ==
          false);
    CHECK(secondMessageOneProof->isValid(*secondMessage, *keypair->getPublicKey(), ONE_MOD_Q()) ==
          true);
}

TEST_CASE("Disjunctive CP Proof encryption of zero with precomputed values succeeds")
{
    const auto &nonce = ONE_MOD_Q();
    const auto &seed = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q(), false);

    // cause a two triples and a quad to be populated
    PrecomputeBufferContext::initialize(*keypair->getPublicKey(), 1);
    PrecomputeBufferContext::start();
    PrecomputeBufferContext::stop();

    // this function runs off to look in the precomputed values buffer and if
    // it finds what it needs the the returned class will contain those values
    auto precomputedValues = PrecomputeBufferContext::getPrecomputedSelection();

    CHECK(precomputedValues != nullptr);

    auto message1 =
      elgamalEncrypt(0UL, *keypair->getPublicKey(), *precomputedValues->getPartialEncryption());

    auto proof = DisjunctiveChaumPedersenProof::make(*message1, *precomputedValues,
                                                     *keypair->getPublicKey(), ONE_MOD_Q(), 0UL);

    CHECK(proof->isValid(*message1, *keypair->getPublicKey(), ONE_MOD_Q()) == true);
    PrecomputeBufferContext::clear();
}

TEST_CASE("Disjunctive CP Proof encryption of zero with precomputed values invalid proof fails")
{
    const auto &nonce = ONE_MOD_Q();
    const auto &seed = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q(), false);

    // cause a two triples and a quad to be populated
    PrecomputeBufferContext::initialize(*keypair->getPublicKey(), 1);
    PrecomputeBufferContext::start();
    PrecomputeBufferContext::stop();

    // this function runs off to look in the precomputed values buffer and if
    // it finds what it needs the the returned class will contain those values
    auto precomputedValues = PrecomputeBufferContext::getPrecomputedSelection();

    CHECK(precomputedValues != nullptr);

    auto message1 =
      elgamalEncrypt(0UL, *keypair->getPublicKey(), *precomputedValues->getPartialEncryption());

    auto badProof = DisjunctiveChaumPedersenProof::make(*message1, *precomputedValues,
                                                        *keypair->getPublicKey(), ONE_MOD_Q(), 1UL);

    CHECK(badProof->isValid(*message1, *keypair->getPublicKey(), ONE_MOD_Q()) == false);
    PrecomputeBufferContext::clear();
}

TEST_CASE("Disjunctive CP Proof encryption of one with precomputed values succeeds")
{
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q(), false);
    const auto &nonce = ONE_MOD_Q();
    const auto &seed = TWO_MOD_Q();

    // cause a two triples and a quad to be populated
    PrecomputeBufferContext::initialize(*keypair->getPublicKey(), 1);
    PrecomputeBufferContext::start();
    PrecomputeBufferContext::stop();

    // this function runs off to look in the precomputed values buffer and if
    // it finds what it needs the the returned class will contain those values
    auto precomputedValues1 = PrecomputeBufferContext::getPrecomputedSelection();

    CHECK(precomputedValues1 != nullptr);

    auto message1 =
      elgamalEncrypt(1UL, *keypair->getPublicKey(), *precomputedValues1->getPartialEncryption());

    auto proof = DisjunctiveChaumPedersenProof::make(*message1, *precomputedValues1,
                                                     *keypair->getPublicKey(), ONE_MOD_Q(), 1UL);

    CHECK(proof->isValid(*message1, *keypair->getPublicKey(), ONE_MOD_Q()) == true);
    PrecomputeBufferContext::clear();
}

TEST_CASE("Disjunctive CP Proof encryption of one with precomputed values invalid proof fails")
{
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q(), false);
    const auto &nonce = ONE_MOD_Q();
    const auto &seed = TWO_MOD_Q();

    // cause a two triples and a quad to be populated
    PrecomputeBufferContext::initialize(*keypair->getPublicKey(), 1);
    PrecomputeBufferContext::start();
    PrecomputeBufferContext::stop();

    // this function runs off to look in the precomputed values buffer and if
    // it finds what it needs the the returned class will contain those values
    auto precomputedValues1 = PrecomputeBufferContext::getPrecomputedSelection();

    CHECK(precomputedValues1 != nullptr);

    auto message1 =
      elgamalEncrypt(1UL, *keypair->getPublicKey(), *precomputedValues1->getPartialEncryption());

    auto badProof = DisjunctiveChaumPedersenProof::make(*message1, *precomputedValues1,
                                                        *keypair->getPublicKey(), ONE_MOD_Q(), 0UL);

    CHECK(badProof->isValid(*message1, *keypair->getPublicKey(), ONE_MOD_Q()) == false);
    PrecomputeBufferContext::clear();
}

TEST_CASE("Constant CP Proof encryption of zero" * doctest::may_fail())
{
    Log::info("Skipping Constant CP Proof Check because the proof is invalid when using the "
              "base-k elgamal encryption method");
    const auto &nonce = ONE_MOD_Q();
    const auto &seed = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q(), false);

    auto message = elgamalEncrypt(0UL, nonce, *keypair->getPublicKey());
    auto proof = ConstantChaumPedersenProof::make(*message, nonce, *keypair->getPublicKey(), seed,
                                                  ONE_MOD_Q(), 0UL);
    auto badProof = ConstantChaumPedersenProof::make(*message, nonce, *keypair->getPublicKey(),
                                                     seed, ONE_MOD_Q(), 1UL);

    CHECK(proof->isValid(*message, *keypair->getPublicKey(), ONE_MOD_Q()) == true);
    CHECK(badProof->isValid(*message, *keypair->getPublicKey(), ONE_MOD_Q()) == false);
}

TEST_CASE("Constant CP Proof encryption of one" * doctest::should_fail())
{
    Log::info("Skipping Constant CP Proof Check because the proof is invalid when using the "
              "base-k elgamal encryption method");
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q(), false);
    const auto &nonce = ONE_MOD_Q();
    const auto &seed = TWO_MOD_Q();

    auto message = elgamalEncrypt(1UL, nonce, *keypair->getPublicKey());
    auto proof = ConstantChaumPedersenProof::make(*message, nonce, *keypair->getPublicKey(), seed,
                                                  ONE_MOD_Q(), 1UL);
    auto badProof = ConstantChaumPedersenProof::make(*message, nonce, *keypair->getPublicKey(),
                                                     seed, ONE_MOD_Q(), 0UL);

    CHECK(proof->isValid(*message, *keypair->getPublicKey(), ONE_MOD_Q()) == true);
    CHECK(badProof->isValid(*message, *keypair->getPublicKey(), ONE_MOD_Q()) == false);
}
