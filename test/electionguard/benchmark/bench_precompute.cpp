#include "../generators/ballot.hpp"
#include "../generators/election.hpp"
#include "../generators/manifest.hpp"
#include "../utils/constants.hpp"

#include <benchmark/benchmark.h>
#include <electionguard/ballot.hpp>
#include <electionguard/election.hpp>
#include <electionguard/encrypt.hpp>
#include <electionguard/manifest.hpp>
#include <electionguard/nonces.hpp>

using namespace electionguard;
using namespace electionguard::tools::generators;
using namespace std;

#pragma region ElGamalEncryptPrecomputed

class ElGamalEncryptPrecomputedFixture : public benchmark::Fixture
{
  public:
    void SetUp(const ::benchmark::State &state)
    {
        secret = ElementModQ::fromHex(a_fixed_secret);
        fixed_base_keypair = ElGamalKeyPair::fromSecret(*secret);

        // cause 50 precomputed entries that will be used by the selection
        // encryptions, that should be more than enough and on teardown
        // the rest will be removed.
        PrecomputeBufferContext::initialize(*fixed_base_keypair->getPublicKey(), 50);
        PrecomputeBufferContext::start();
    }

    void TearDown(const ::benchmark::State &state) { PrecomputeBufferContext::clear(); }

    unique_ptr<ElementModQ> secret;
    unique_ptr<ElGamalKeyPair> fixed_base_keypair;
};

BENCHMARK_DEFINE_F(ElGamalEncryptPrecomputedFixture, ElGamalEncryptPrecomputed)
(benchmark::State &state)
{
    while (state.KeepRunning()) {
        auto precomputedTwoTriplesAndAQuad = PrecomputeBufferContext::getTwoTriplesAndAQuadruple();

        // check if we found the precomputed values needed
        if (precomputedTwoTriplesAndAQuad != nullptr) {
            elgamalEncrypt(1UL, *precomputedTwoTriplesAndAQuad);
        }
    }
}

BENCHMARK_REGISTER_F(ElGamalEncryptPrecomputedFixture, ElGamalEncryptPrecomputed)
  ->Unit(benchmark::kMillisecond);

#pragma endregion

#pragma region disjunctiveChaumPedersenPrecomputed

class DisjunctiveChaumPedersenProofPrecomputedHarness : DisjunctiveChaumPedersenProof
{
  public:
    static unique_ptr<DisjunctiveChaumPedersenProof>
    make_zero(const ElGamalCiphertext &message,
              const TwoTriplesAndAQuadruple &precomputedTwoTriplesAndAQuad1, const ElementModP &k,
              const ElementModQ &q)
    {
        return DisjunctiveChaumPedersenProof::make_zero(message, precomputedTwoTriplesAndAQuad1, k,
                                                        q);
    }

    static unique_ptr<DisjunctiveChaumPedersenProof>
    make_one(const ElGamalCiphertext &message,
             const TwoTriplesAndAQuadruple &precomputedTwoTriplesAndAQuad1, const ElementModP &k,
             const ElementModQ &q)
    {
        return DisjunctiveChaumPedersenProof::make_one(message, precomputedTwoTriplesAndAQuad1, k,
                                                       q);
    }
};

class ChaumPedersenPrecomputedFixture : public benchmark::Fixture
{
  public:
    void SetUp(const ::benchmark::State &state)
    {
        keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q(), false);

        nonce = ONE_MOD_Q().clone();
        seed = TWO_MOD_Q().clone();

        // create precomputed entries that will be used by the selection
        // encryptions, on teardown the rest will be removed.
        PrecomputeBufferContext::initialize(*keypair->getPublicKey(), 150);
        PrecomputeBufferContext::start();

        message = elgamalEncrypt(1UL, *nonce, *keypair->getPublicKey());
        auto precomputedTwoTriplesAndAQuad = PrecomputeBufferContext::getTwoTriplesAndAQuadruple();

        // check if we found the precomputed values needed
        if (precomputedTwoTriplesAndAQuad != nullptr) {
            disjunctive = DisjunctiveChaumPedersenProof::make(
              *message, *precomputedTwoTriplesAndAQuad, TWO_MOD_P(), ONE_MOD_Q(), 1);
        }
    }

    void TearDown(const ::benchmark::State &state) { PrecomputeBufferContext::clear(); }

    unique_ptr<ElementModQ> nonce;
    unique_ptr<ElementModQ> seed;
    unique_ptr<ElementModQ> hashContext;
    unique_ptr<ElGamalKeyPair> keypair;
    unique_ptr<ElGamalCiphertext> message;
    unique_ptr<DisjunctiveChaumPedersenProof> disjunctive;
    unique_ptr<ConstantChaumPedersenProof> constant;
};

BENCHMARK_DEFINE_F(ChaumPedersenPrecomputedFixture, disjunctiveChaumPedersenPrecomputed)
(benchmark::State &state)
{
    while (state.KeepRunning()) {
        auto precomputedTwoTriplesAndAQuad = PrecomputeBufferContext::getTwoTriplesAndAQuadruple();

        // check if we found the precomputed values needed
        if (precomputedTwoTriplesAndAQuad != nullptr) {
            DisjunctiveChaumPedersenProof::make(*message, *precomputedTwoTriplesAndAQuad,
                                                TWO_MOD_P(), ONE_MOD_Q(), 1);
        }
    }
}

BENCHMARK_REGISTER_F(ChaumPedersenPrecomputedFixture, disjunctiveChaumPedersenPrecomputed)
  ->Unit(benchmark::kMillisecond);

BENCHMARK_DEFINE_F(ChaumPedersenPrecomputedFixture, disjunctiveChaumPedersenPrecomputed_Zero)
(benchmark::State &state)
{
    while (state.KeepRunning()) {
        auto precomputedTwoTriplesAndAQuad = PrecomputeBufferContext::getTwoTriplesAndAQuadruple();

        // check if we found the precomputed values needed
        if (precomputedTwoTriplesAndAQuad != nullptr) {
            auto item = DisjunctiveChaumPedersenProofPrecomputedHarness::make_zero(
              *message, *precomputedTwoTriplesAndAQuad, TWO_MOD_P(), ONE_MOD_Q());
        }
    }
}

BENCHMARK_REGISTER_F(ChaumPedersenPrecomputedFixture, disjunctiveChaumPedersenPrecomputed_Zero)
  ->Unit(benchmark::kMillisecond);

BENCHMARK_DEFINE_F(ChaumPedersenPrecomputedFixture, disjunctiveChaumPedersenPrecomputed_One)
(benchmark::State &state)
{
    while (state.KeepRunning()) {
        auto precomputedTwoTriplesAndAQuad = PrecomputeBufferContext::getTwoTriplesAndAQuadruple();

        // check if we found the precomputed values needed
        if (precomputedTwoTriplesAndAQuad != nullptr) {
            auto item = DisjunctiveChaumPedersenProofPrecomputedHarness::make_one(
              *message, *precomputedTwoTriplesAndAQuad, TWO_MOD_P(), ONE_MOD_Q());
        }
    }
}

BENCHMARK_REGISTER_F(ChaumPedersenPrecomputedFixture, disjunctiveChaumPedersenPrecomputed_One)
  ->Unit(benchmark::kMillisecond);

#pragma endregion

#pragma region ciphertextBallotSelectionPrecomputed

class CiphertextBallotSelectionPrecomputedFixture : public benchmark::Fixture
{
  public:
    void SetUp(const ::benchmark::State &state)
    {
        const auto *candidateId = "some-candidate-id";
        const auto *selectionId = "some-selection-object-id";
        auto secret = ElementModQ::fromHex(a_fixed_secret);
        keypair = ElGamalKeyPair::fromSecret(*secret);

        description = make_unique<SelectionDescription>(selectionId, candidateId, 1UL);
        descriptionHash = description->crypto_hash();

        auto nonce = ElementModQ::fromHex(a_fixed_nonce);

        // cause precomputed entries that will be used by the selection
        // encryptions, that should be more than enough and on teardown
        // the rest will be removed.
        PrecomputeBufferContext::initialize(*keypair->getPublicKey(), 50);
        PrecomputeBufferContext::start();

        auto precomputedTwoTriplesAndAQuad = PrecomputeBufferContext::getTwoTriplesAndAQuadruple();

        // check if we found the precomputed values needed
        if (precomputedTwoTriplesAndAQuad != nullptr) {
            // Generate the encryption using precomputed values
            ciphertext = elgamalEncrypt(1UL, *precomputedTwoTriplesAndAQuad);

            auto encrypted = CiphertextBallotSelection::make(
              selectionId, description->getSequenceOrder(), *descriptionHash, move(ciphertext),
              TWO_MOD_P(), ONE_MOD_Q(), move(precomputedTwoTriplesAndAQuad), 1UL, false, nullptr,
              true);
        }
    }

    void TearDown(const ::benchmark::State &state) { PrecomputeBufferContext::clear(); }

    unique_ptr<SelectionDescription> description;
    unique_ptr<ElementModQ> descriptionHash;
    unique_ptr<ElGamalKeyPair> keypair;
    unique_ptr<ElGamalCiphertext> ciphertext;
};

BENCHMARK_DEFINE_F(CiphertextBallotSelectionPrecomputedFixture,
                   ciphertextBallotSelectionPrecomputed)
(benchmark::State &state)
{
    const auto *selectionId = "some-selection-object-id";

    while (state.KeepRunningBatch(50)) {
        auto precomputedTwoTriplesAndAQuad = PrecomputeBufferContext::getTwoTriplesAndAQuadruple();

        // check if we found the precomputed values needed
        if (precomputedTwoTriplesAndAQuad != nullptr) {
            // Generate the encryption using precomputed values
            auto localciphertext = elgamalEncrypt(1UL, *precomputedTwoTriplesAndAQuad);

            auto encrypted = CiphertextBallotSelection::make(
              selectionId, description->getSequenceOrder(), *descriptionHash, move(localciphertext),
              TWO_MOD_P(), ONE_MOD_Q(), move(precomputedTwoTriplesAndAQuad), 1UL, false, nullptr,
              true);
        }
    }
}

BENCHMARK_REGISTER_F(CiphertextBallotSelectionPrecomputedFixture,
                     ciphertextBallotSelectionPrecomputed)
  ->Unit(benchmark::kMillisecond);

#pragma endregion // ciphertextBallotSelectionPrecomputed

#pragma region encryptSelectionPrecomputed

class EncryptSelectionPrecomputedFixture : public benchmark::Fixture
{
  public:
    void SetUp(const ::benchmark::State &state)
    {
        const auto *candidateId = "some-candidate-id";
        const auto *selectionId = "some-selection-object-id";
        auto secret = ElementModQ::fromHex(a_fixed_secret);
        keypair = ElGamalKeyPair::fromSecret(*secret);

        metadata = make_unique<SelectionDescription>(selectionId, candidateId, 1UL);
        hashContext = metadata->crypto_hash();
        plaintext = BallotGenerator::selectionFrom(*metadata);

        auto nonce = ElementModQ::fromHex(a_fixed_nonce);
        ciphertext = encryptSelection(*plaintext, *metadata, *keypair->getPublicKey(), ONE_MOD_Q(),
                                      *nonce, false, false);

        // cause precomputed entries that will be used by the selection
        // encryptions, that should be more than enough and on teardown
        // the rest will be removed.
        PrecomputeBufferContext::initialize(*keypair->getPublicKey(), 50);
        PrecomputeBufferContext::start();
    }

    void TearDown(const ::benchmark::State &state) { PrecomputeBufferContext::clear(); }

    unique_ptr<ElementModQ> hashContext;
    unique_ptr<ElGamalKeyPair> keypair;
    unique_ptr<SelectionDescription> metadata;
    unique_ptr<PlaintextBallotSelection> plaintext;
    unique_ptr<CiphertextBallotSelection> ciphertext;
};

BENCHMARK_DEFINE_F(EncryptSelectionPrecomputedFixture, encryptSelectionPrecomputed)
(benchmark::State &state)
{
    while (state.KeepRunningBatch(50)) {
        auto nonce = rand_q();
        encryptSelection(*plaintext, *metadata, *keypair->getPublicKey(), ONE_MOD_Q(), *nonce,
                         false, false);
    }
}

BENCHMARK_REGISTER_F(EncryptSelectionPrecomputedFixture, encryptSelectionPrecomputed)
  ->Unit(benchmark::kMillisecond);

#pragma endregion //encryptSelectionPrecomputed

#pragma region precomputed

class PrecomputeFixture : public benchmark::Fixture
{
  public:
    void SetUp(const ::benchmark::State &state)
    {
        auto secret = ElementModQ::fromHex(a_fixed_secret);
        keypair = ElGamalKeyPair::fromSecret(*secret);
    }

    void TearDown(const ::benchmark::State &state) { PrecomputeBufferContext::clear(); }

    unique_ptr<ElGamalKeyPair> keypair;
};

BENCHMARK_DEFINE_F(PrecomputeFixture, precomputed)
(benchmark::State &state)
{
    for (auto _ : state) {
        PrecomputeBufferContext::initialize(*keypair->getPublicKey(), 1);
        PrecomputeBufferContext::start();

        auto precomputedTwoTriplesAndAQuad = PrecomputeBufferContext::getTwoTriplesAndAQuadruple();
    }
    PrecomputeBufferContext::clear();
}

BENCHMARK_REGISTER_F(PrecomputeFixture, precomputed)->Unit(benchmark::kMillisecond);

#pragma endregion // precomputed

#pragma region encryptBallotPrecompute

class EncryptBallotPrecomputeFixture : public benchmark::Fixture
{
  public:
    void SetUp(const ::benchmark::State &state)
    {
        auto secret = ElementModQ::fromHex(a_fixed_secret);
        nonce = ElementModQ::fromHex(a_fixed_nonce);
        nonces = make_unique<Nonces>(*nonce, "encrypt-ballot-benchmark");
        keypair = ElGamalKeyPair::fromSecret(*secret);
        manifest = ManifestGenerator::getManifestFromFile(TEST_SPEC_VERSION, TEST_USE_SAMPLE);
        internal = make_unique<InternalManifest>(*manifest);
        context = ElectionGenerator::getFakeContext(*internal, *keypair->getPublicKey());
        contextJson = ElectionGenerator::getJsonContext();
        device = make_unique<EncryptionDevice>(12345UL, 23456UL, 34567UL, "Location");
        ballot = BallotGenerator::getFakeBallot(*internal);

        ciphertext = encryptBallot(*ballot, *internal, *context, *device->getHash(), nonces->next(),
                                   0ULL, false);
        ciphertext = encryptBallot(*ballot, *internal, *contextJson, *device->getHash(),
                                   nonces->next(), 0ULL, false);

        // precomputed entries that will be used by the selection
        // encryptions, on teardown the rest will be removed.
        PrecomputeBufferContext::initialize(*keypair->getPublicKey(), 2000);
        PrecomputeBufferContext::start();
    }

    void TearDown(const ::benchmark::State &state) { PrecomputeBufferContext::clear(); }

    unique_ptr<Nonces> nonces;
    unique_ptr<ElGamalKeyPair> keypair;
    unique_ptr<Manifest> manifest;
    unique_ptr<InternalManifest> internal;
    unique_ptr<CiphertextElectionContext> context;
    unique_ptr<CiphertextElectionContext> contextJson;
    unique_ptr<EncryptionDevice> device;
    unique_ptr<ElementModQ> nonce;
    unique_ptr<PlaintextBallot> ballot;
    unique_ptr<CiphertextBallot> ciphertext;
};

BENCHMARK_DEFINE_F(EncryptBallotPrecomputeFixture, encryptBallotPrecompute_Full_NoProofCheck)
(benchmark::State &state)
{
    PrecomputeBufferContext::stop();
    while (state.KeepRunning()) {
        auto result = encryptBallot(*ballot, *internal, *context, *device->getHash(),
                                    make_unique<ElementModQ>(*nonce), 0UL, false);
    }
}

BENCHMARK_REGISTER_F(EncryptBallotPrecomputeFixture, encryptBallotPrecompute_Full_NoProofCheck)
  ->Unit(benchmark::kMillisecond);

#pragma endregion // encryptBallotPrecompute
