#include "../../src/electionguard/convert.hpp"
#include "../../src/electionguard/log.hpp"

#include <doctest/doctest.h>
#include <electionguard/chaum_pedersen.hpp>
#include <electionguard/elgamal.hpp>
#include <electionguard/group.hpp>
#include <iostream>
#include <string>
#include <utility>

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
    CHECK(secondMessageOneProof->isValid(*secondMessage, *keypair->getPublicKey(), ONE_MOD_Q()) ==
          true);
}

TEST_CASE("Disjunctive CP Proof simple valid inputs fail invalid proofs")
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
    CHECK(firstMessageOneProof->isValid(*firstMessage, *keypair->getPublicKey(), ONE_MOD_Q()) ==
          false);
    CHECK(secondMessageZeroProof->isValid(*secondMessage, *keypair->getPublicKey(), ONE_MOD_Q()) ==
          false);
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

// make a fake ranged CP proof according to the provided parameters
static pair<unique_ptr<ElGamalCiphertext>, unique_ptr<RangedChaumPedersenProof>>
makeAFakeRangedProof(const ElGamalKeyPair &keypair, uint64_t selected, uint64_t limit,
                     uint64_t count)
{
    const auto &nonce = ONE_MOD_Q();
    const auto &seed = TWO_MOD_Q();

    // encrypt selections representing a contest on ballot
    vector<unique_ptr<ElGamalCiphertext>> messages;
    for (size_t i = 0; i < count; i++) {
        auto choice = i < selected ? 1UL : 0UL;
        auto message = elgamalEncrypt(choice, nonce, *keypair.getPublicKey());
        messages.push_back(move(message));
    }

    auto accumulation = elgamalAdd(referenceWrap(messages));
    auto aggregateNonce = mul_mod_q(nonce, *ElementModQ::fromUint64(count));

    auto proof = RangedChaumPedersenProof::make(*accumulation, *aggregateNonce, selected, limit,
                                                *keypair.getPublicKey(), ONE_MOD_Q(), "test");
    return make_pair(move(accumulation), move(proof));
}

TEST_CASE("Ranged CP Proof encryption of zero generates valid proof")
{
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q(), false);
    const auto selected = 0UL; // we chose 0 selections on the ballot
    const auto limit = 4UL;    // can choose up to 4 selections out of 5
    const auto count = 5UL;    // 5 selections on the ballot

    auto [accumulation, proof] = makeAFakeRangedProof(*keypair, selected, limit, count);
    auto result = proof->isValid(*accumulation, *keypair->getPublicKey(), ONE_MOD_Q(), "test");

    CHECK(result.isValid == true);
}

TEST_CASE("Ranged CP Proof encryption of some generates valid proof")
{
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q(), false);
    const auto selected = 3UL; // we chose 3 selections on the ballot
    const auto limit = 4UL;    // can choose up to 4 selections out of 5
    const auto count = 5UL;    // 5 selections on the ballot

    auto [accumulation, proof] = makeAFakeRangedProof(*keypair, selected, limit, count);
    auto result = proof->isValid(*accumulation, *keypair->getPublicKey(), ONE_MOD_Q(), "test");

    CHECK(result.isValid == true);
}

TEST_CASE("Ranged CP Proof encryption of some with missing commitments generates valid proof")
{
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q(), false);
    const auto selected = 3UL; // we chose 3 selections on the ballot
    const auto limit = 4UL;    // can choose up to 4 selections out of 5
    const auto count = 5UL;    // 5 selections on the ballot

    auto [accumulation, proof] = makeAFakeRangedProof(*keypair, selected, limit, count);
    auto integerProofs = proof->getProofs();
    Log::trace("proofs size: " + to_string(integerProofs.size()));
    for (size_t i = 0; i < limit; i++) {
        // just arbitrarily remove some but not all of the commitments
        if (i != selected - 1) {
            integerProofs.at(i).get().commitment.reset();
        }
    }

    auto result = proof->isValid(*accumulation, *keypair->getPublicKey(), ONE_MOD_Q(), "test");

    CHECK(result.isValid == true);
}

TEST_CASE("Ranged CP Proof encryption of all generates valid proof")
{
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q(), false);
    const auto selected = 4UL; // we chose 3 selections on the ballot
    const auto limit = 4UL;    // can choose up to 4 selections out of 5
    const auto count = 5UL;    // 5 selections on the ballot

    auto [accumulation, proof] = makeAFakeRangedProof(*keypair, selected, limit, count);
    auto result = proof->isValid(*accumulation, *keypair->getPublicKey(), ONE_MOD_Q(), "test");
    CHECK(result.isValid == true);
}

// the constant CP Proof is only compatible with
// E.G. 1.0 Compatible ElGamal Encrypt.
// for E.G. 2.0 Base-K ElGamal Encrypt use RangedChaumPedersenProof
TEST_CASE("Constant CP Proof encryption of zero")
{
    const auto &nonce = ONE_MOD_Q();
    const auto &seed = TWO_MOD_Q();
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q(), false);

    // E.G. 1.0 Compatible ElGamal Encrypt.
    auto message = elgamalEncrypt(0UL, nonce, *keypair->getPublicKey(), G());
    auto proof = ConstantChaumPedersenProof::make(*message, nonce, *keypair->getPublicKey(), seed,
                                                  ONE_MOD_Q(), 0UL);
    auto badProof = ConstantChaumPedersenProof::make(*message, nonce, *keypair->getPublicKey(),
                                                     seed, ONE_MOD_Q(), 1UL);

    CHECK(proof->isValid(*message, *keypair->getPublicKey(), ONE_MOD_Q()) == true);
    CHECK(badProof->isValid(*message, *keypair->getPublicKey(), ONE_MOD_Q()) == false);
}

TEST_CASE("Constant CP Proof encryption of one")
{
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q(), false);
    const auto &nonce = ONE_MOD_Q();
    const auto &seed = TWO_MOD_Q();

    // E.G. 1.0 Compatible ElGamal Encrypt.
    auto message = elgamalEncrypt(1UL, nonce, *keypair->getPublicKey(), G());
    auto proof = ConstantChaumPedersenProof::make(*message, nonce, *keypair->getPublicKey(), seed,
                                                  ONE_MOD_Q(), 1UL);
    auto badProof = ConstantChaumPedersenProof::make(*message, nonce, *keypair->getPublicKey(),
                                                     seed, ONE_MOD_Q(), 0UL);

    CHECK(proof->isValid(*message, *keypair->getPublicKey(), ONE_MOD_Q()) == true);
    CHECK(badProof->isValid(*message, *keypair->getPublicKey(), ONE_MOD_Q()) == false);
}
