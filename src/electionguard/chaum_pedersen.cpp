#include "electionguard/chaum_pedersen.hpp"

#include "convert.hpp"
#include "electionguard/nonces.hpp"
#include "electionguard/precompute_buffers.hpp"
#include "log.hpp"

#include <algorithm>
#include <cstdlib>
#include <cstring>
#include <electionguard/hash.hpp>
#include <map>
#include <stdexcept>

using electionguard::ONE_MOD_Q;
using std::for_each;
using std::invalid_argument;
using std::make_unique;
using std::map;
using std::move;
using std::reference_wrapper;
using std::string;
using std::to_string;
using std::unique_ptr;

namespace electionguard
{

#pragma region DisjunctiveChaumPedersenProof

    struct DisjunctiveChaumPedersenProof::Impl {

        // TODO: #362 reaplce internal structure with the generic ZKP
        unique_ptr<ElementModP> proof_zero_pad;
        unique_ptr<ElementModP> proof_zero_data;
        unique_ptr<ElementModP> proof_one_pad;
        unique_ptr<ElementModP> proof_one_data;
        unique_ptr<ElementModQ> proof_zero_challenge;
        unique_ptr<ElementModQ> proof_one_challenge;
        unique_ptr<ElementModQ> challenge;
        unique_ptr<ElementModQ> proof_zero_response;
        unique_ptr<ElementModQ> proof_one_response;

        Impl(unique_ptr<ElementModP> proof_zero_pad, unique_ptr<ElementModP> proof_zero_data,
             unique_ptr<ElementModP> proof_one_pad, unique_ptr<ElementModP> proof_one_data,
             unique_ptr<ElementModQ> proof_zero_challenge,
             unique_ptr<ElementModQ> proof_one_challenge, unique_ptr<ElementModQ> challenge,
             unique_ptr<ElementModQ> proof_zero_response,
             unique_ptr<ElementModQ> proof_one_response)
            : proof_zero_pad(move(proof_zero_pad)), proof_zero_data(move(proof_zero_data)),
              proof_one_pad(move(proof_one_pad)), proof_one_data(move(proof_one_data)),
              proof_zero_challenge(move(proof_zero_challenge)),
              proof_one_challenge(move(proof_one_challenge)), challenge(move(challenge)),
              proof_zero_response(move(proof_zero_response)),
              proof_one_response(move(proof_one_response))
        {
        }

        [[nodiscard]] unique_ptr<DisjunctiveChaumPedersenProof::Impl> clone() const
        {
            auto _proof_zero_pad = make_unique<ElementModP>(*proof_zero_pad);
            auto _proof_zero_data = make_unique<ElementModP>(*proof_zero_data);
            auto _proof_one_pad = make_unique<ElementModP>(*proof_one_pad);
            auto _proof_one_data = make_unique<ElementModP>(*proof_one_data);
            auto _proof_zero_challenge = make_unique<ElementModQ>(*proof_zero_challenge);
            auto _proof_one_challenge = make_unique<ElementModQ>(*proof_one_challenge);
            auto _challenge = make_unique<ElementModQ>(*challenge);
            auto _proof_zero_response = make_unique<ElementModQ>(*proof_zero_response);
            auto _proof_one_response = make_unique<ElementModQ>(*proof_one_response);

            return make_unique<DisjunctiveChaumPedersenProof::Impl>(
              move(_proof_zero_pad), move(_proof_zero_data), move(_proof_one_pad),
              move(_proof_one_data), move(_proof_zero_challenge), move(_proof_one_challenge),
              move(_challenge), move(_proof_zero_response), move(_proof_one_response));
        }
    };

    // Lifecycle Methods

    DisjunctiveChaumPedersenProof::DisjunctiveChaumPedersenProof(
      const DisjunctiveChaumPedersenProof &other)
        : pimpl(other.pimpl->clone())
    {
    }

    DisjunctiveChaumPedersenProof::DisjunctiveChaumPedersenProof(
      DisjunctiveChaumPedersenProof &&other)
        : pimpl(move(other.pimpl))
    {
    }

    DisjunctiveChaumPedersenProof::DisjunctiveChaumPedersenProof(
      unique_ptr<ElementModP> proof_zero_pad, unique_ptr<ElementModP> proof_zero_data,
      unique_ptr<ElementModP> proof_one_pad, unique_ptr<ElementModP> proof_one_data,
      unique_ptr<ElementModQ> proof_zero_challenge, unique_ptr<ElementModQ> proof_one_challenge,
      unique_ptr<ElementModQ> challenge, unique_ptr<ElementModQ> proof_zero_response,
      unique_ptr<ElementModQ> proof_one_response)
        : pimpl(new Impl(move(proof_zero_pad), move(proof_zero_data), move(proof_one_pad),
                         move(proof_one_data), move(proof_zero_challenge),
                         move(proof_one_challenge), move(challenge), move(proof_zero_response),
                         move(proof_one_response)))
    {
    }

    DisjunctiveChaumPedersenProof::~DisjunctiveChaumPedersenProof() = default;

    // Operator Overloads

    DisjunctiveChaumPedersenProof &
    DisjunctiveChaumPedersenProof::operator=(DisjunctiveChaumPedersenProof other)
    {
        swap(pimpl, other.pimpl);
        return *this;
    }

    // Property Getters

    ElementModP *DisjunctiveChaumPedersenProof::getProofZeroPad() const
    {
        return pimpl->proof_zero_pad.get();
    }
    ElementModP *DisjunctiveChaumPedersenProof::getProofZeroData() const
    {
        return pimpl->proof_zero_data.get();
    }
    ElementModP *DisjunctiveChaumPedersenProof::getProofOnePad() const
    {
        return pimpl->proof_one_pad.get();
    }
    ElementModP *DisjunctiveChaumPedersenProof::getProofOneData() const
    {
        return pimpl->proof_one_data.get();
    }
    ElementModQ *DisjunctiveChaumPedersenProof::getProofZeroChallenge() const
    {
        return pimpl->proof_zero_challenge.get();
    }
    ElementModQ *DisjunctiveChaumPedersenProof::getProofOneChallenge() const
    {
        return pimpl->proof_one_challenge.get();
    }
    ElementModQ *DisjunctiveChaumPedersenProof::getChallenge() const
    {
        return pimpl->challenge.get();
    }
    ElementModQ *DisjunctiveChaumPedersenProof::getProofZeroResponse() const
    {
        return pimpl->proof_zero_response.get();
    }
    ElementModQ *DisjunctiveChaumPedersenProof::getProofOneResponse() const
    {
        return pimpl->proof_one_response.get();
    }

    // Public Static Methods

    unique_ptr<DisjunctiveChaumPedersenProof>
    DisjunctiveChaumPedersenProof::make(const ElGamalCiphertext &message, const ElementModQ &r,
                                        const ElementModP &k, const ElementModQ &q,
                                        uint64_t plaintext)
    {
        if (plaintext > 1) {
            throw invalid_argument(
              "DisjunctiveChaumPedersenProof::make:: only supports plaintexts of 0 or 1");
        }
        Log::trace("DisjunctiveChaumPedersenProof: making proof without seed.");
        if (plaintext == 1) {
            return make_one(message, r, k, q);
        }
        return make_zero(message, r, k, q);
    }

    unique_ptr<DisjunctiveChaumPedersenProof>
    DisjunctiveChaumPedersenProof::make(const ElGamalCiphertext &message, const ElementModQ &r,
                                        const ElementModP &k, const ElementModQ &q,
                                        const ElementModQ &seed, uint64_t plaintext)
    {
        if (plaintext > 1) {
            throw invalid_argument(
              "DisjunctiveChaumPedersenProof::make:: only supports plaintexts of 0 or 1");
        }
        Log::trace("DisjunctiveChaumPedersenProof: making proof with seed.");
        if (plaintext == 1) {
            return make_one(message, r, k, q, seed);
        }
        return make_zero(message, r, k, q, seed);
    }

    unique_ptr<DisjunctiveChaumPedersenProof> DisjunctiveChaumPedersenProof::make(
      const ElGamalCiphertext &message, const PrecomputedSelection &precomputedValues,
      const ElementModP &k, const ElementModQ &q, uint64_t plaintext)
    {
        return make(message, *precomputedValues.getPartialEncryption()->getSecret(),
                    precomputedValues.getRealCommitment()->clone(),
                    precomputedValues.getFakeCommitment()->clone(), k, q, plaintext);
    }

    unique_ptr<DisjunctiveChaumPedersenProof> DisjunctiveChaumPedersenProof::make(
      const ElGamalCiphertext &message, const ElementModQ &r,
      unique_ptr<PrecomputedEncryption> real, unique_ptr<PrecomputedFakeDisjuctiveCommitments> fake,
      const ElementModP &k, const ElementModQ &q, uint64_t plaintext)
    {
        unique_ptr<DisjunctiveChaumPedersenProof> result;

        if (plaintext > 1) {
            throw invalid_argument(
              "DisjunctiveChaumPedersenProof::make:: only supports plaintexts of 0 or 1");
        }
        Log::trace("DisjunctiveChaumPedersenProof: making proof without seed.");
        if (plaintext == 1) {
            return make_one(message, r, move(real), move(fake), k, q);
        }
        return make_zero(message, r, move(real), move(fake), k, q);
    }

    // Public Methods

    // TODO: return a result struct with a bool and a string
    bool DisjunctiveChaumPedersenProof::isValid(const ElGamalCiphertext &message,
                                                const ElementModP &k, const ElementModQ &q)
    {
        Log::trace("DisjunctiveChaumPedersenProof::isValid: ");
        auto *alpha = message.getPad();
        auto *beta = message.getData();

        auto *a0p = pimpl->proof_zero_pad.get();
        auto *b0p = pimpl->proof_zero_data.get();
        auto *a1p = pimpl->proof_one_pad.get();
        auto *b1p = pimpl->proof_one_data.get();

        auto a0 = *pimpl->proof_zero_pad;
        auto b0 = *pimpl->proof_zero_data;
        auto a1 = *pimpl->proof_one_pad;
        auto b1 = *pimpl->proof_one_data;
        auto c0 = *pimpl->proof_zero_challenge;
        auto c1 = *pimpl->proof_one_challenge;
        auto c = *pimpl->challenge;
        auto v0 = *pimpl->proof_zero_response;
        auto v1 = *pimpl->proof_one_response;

        auto inBounds_alpha = alpha->isValidResidue();
        auto inBounds_beta = beta->isValidResidue();
        auto inBounds_a0 = a0.isValidResidue();
        auto inBounds_b0 = b0.isValidResidue();
        auto inBounds_a1 = a1.isValidResidue();
        auto inBounds_b1 = b1.isValidResidue();
        auto inBounds_c0 = c0.isInBounds();
        auto inBounds_c1 = c1.isInBounds();
        auto inBounds_v0 = v0.isInBounds();
        auto inBounds_v1 = v1.isInBounds();

        // c = H(HE;21,K,α,β,a0,b0,a1,b1). Ballot Selection Encryption Proof (selected/unselected) 3.3.5
        auto consistent_c =
          (*add_mod_q(c0, c1) == c) &&
          (c ==
           *hash_elems({HashPrefix::get_prefix_selection_proof(), &const_cast<ElementModQ &>(q),
                        &const_cast<ElementModP &>(k), alpha, beta, a0p, b0p, a1p, b1p}));

        // 𝑎0 = 𝑔^𝑣0 mod 𝑝 ⋅ 𝛼^𝑐0 mod 𝑝
        auto consistent_gv0 = (a0 == *mul_mod_p(*g_pow_p(v0), *pow_mod_p(*alpha, c0)));

        // 𝑎1 = 𝑔^𝑣1 mod 𝑝 ⋅ 𝛼^𝑐1 mod 𝑝
        auto consistent_gv1 = (a1 == *mul_mod_p(*g_pow_p(v1), *pow_mod_p(*alpha, c1)));

        // 𝑏0 = 𝐾^𝑣0 mod 𝑝 ⋅ 𝛽^𝑐0 mod 𝑝
        auto consistent_kv0 = (b0 == *mul_mod_p(*pow_mod_p(k, v0), *pow_mod_p(*beta, c0)));

        // 𝑏1 = 𝐾^w1 mod 𝑝 ⋅ 𝛽^𝑐1 mod 𝑝
        auto w1 = sub_mod_q(v1, c1);
        auto consistent_kw1 = (b1 == *mul_mod_p(*pow_mod_p(k, *w1), *pow_mod_p(*beta, c1)));

        auto success = inBounds_alpha && inBounds_beta && inBounds_a0 && inBounds_b0 &&
                       inBounds_a1 && inBounds_b1 && inBounds_c0 && inBounds_c1 && inBounds_v0 &&
                       inBounds_v1 && consistent_c && consistent_gv0 && consistent_gv1 &&
                       consistent_kv0 && consistent_kw1;

        if (!success) {

            map<string, bool> printMap{
              {"inBounds_alpha", inBounds_alpha},  {"inBounds_beta", inBounds_beta},
              {"inBounds_a0", inBounds_a0},        {"inBounds_b0", inBounds_b0},
              {"inBounds_a1", inBounds_a1},        {"inBounds_b1", inBounds_b1},
              {"inBounds_c0", inBounds_c0},        {"inBounds_c1", inBounds_c1},
              {"inBounds_v0", inBounds_v0},        {"inBounds_v1", inBounds_v1},
              {"consistent_c", consistent_c},      {"consistent_g^v0", consistent_gv0},
              {"consistent_g^v1", consistent_gv1}, {"consistent_k^v0", consistent_kv0},
              {"consistent_k^w1", consistent_kw1},
            };

            Log::info("found an invalid Disjunctive Chaum-Pedersen proof", printMap);

            Log::debug("k->get", k.toHex());
            Log::debug("q->get", q.toHex());
            Log::debug("alpha->get", alpha->toHex());
            Log::debug("beta->get", beta->toHex());
            Log::debug("a0->get", a0.toHex());
            Log::debug("b0->get", b0.toHex());
            Log::debug("a1->get", a1.toHex());
            Log::debug("b1->get", b1.toHex());
            Log::debug("c0->get", c0.toHex());
            Log::debug("c1->get", c1.toHex());
            Log::debug("c->get", c.toHex());
            Log::debug("v0->get", v0.toHex());
            Log::debug("v1->get", v1.toHex());
            Log::debug("w1->get", w1->toHex());

            return false;
        }
        Log::trace("DisjunctiveChaumPedersenProof::isValid: TRUE!");
        return success;
    }

    std::unique_ptr<DisjunctiveChaumPedersenProof> DisjunctiveChaumPedersenProof::clone() const
    {
        return make_unique<DisjunctiveChaumPedersenProof>(
          pimpl->proof_zero_pad->clone(), pimpl->proof_zero_data->clone(),
          pimpl->proof_one_pad->clone(), pimpl->proof_one_pad->clone(),
          pimpl->proof_zero_challenge->clone(), pimpl->proof_one_challenge->clone(),
          pimpl->challenge->clone(), pimpl->proof_zero_response->clone(),
          pimpl->proof_one_response->clone());
    }

    // Protected Methods

    unique_ptr<DisjunctiveChaumPedersenProof>
    DisjunctiveChaumPedersenProof::make_zero(const ElGamalCiphertext &message, const ElementModQ &r,
                                             const ElementModP &k, const ElementModQ &q)
    {
        auto seed = rand_q();
        return make_zero(message, r, k, q, *seed);
    }

    unique_ptr<DisjunctiveChaumPedersenProof>
    DisjunctiveChaumPedersenProof::make_zero(const ElGamalCiphertext &message, const ElementModQ &r,
                                             const ElementModP &k, const ElementModQ &q,
                                             const ElementModQ &seed)
    {
        // NIZKP for plaintext 0
        // (a0, b0) = (g^𝑢0 mod p, K^𝑢0 mod p)
        // (a1, b1) = (g^𝑢1 mod p, K^(𝑢1-w) mod p) <-- fake proof

        auto *alpha = message.getPad();
        auto *beta = message.getData();

        Log::trace("alpha: ", alpha->toHex());
        Log::trace("beta: ", beta->toHex());

        // Pick three random numbers in Q.
        auto nonces = make_unique<Nonces>(seed, "disjoint-chaum-pedersen-proof");
        auto u0 = nonces->get(0);
        auto u1 = nonces->get(1);
        auto w = nonces->get(2);

        // Compute the NIZKP
        auto a0 = g_pow_p(*u0);                      // 𝑔^𝑢0 mod 𝑝
        auto b0 = pow_mod_p(k, *u0);                 // 𝐾^𝑢0 mod 𝑝
        auto a1 = g_pow_p(*u1);                      // 𝑔^𝑢1 mod 𝑝
        auto b1 = pow_mod_p(k, *sub_mod_q(*u1, *w)); // K^(𝑢1-w) mod p

        // Compute the challenge
        // c = H(HE;21,K,α,β,a0,b0,a1,b1). Ballot Selection Encryption Proof (selected/unselected) 3.3.5
        auto c =
          hash_elems({HashPrefix::get_prefix_selection_proof(), &const_cast<ElementModQ &>(q),
                      &const_cast<ElementModP &>(k), alpha, beta, a0.get(), b0.get(), a1.get(),
                      b1.get()}); // H(04,Q;K,α,β,a0,b0,a1,b1)

        //c1 = w so we dont assign a new var for it
        auto c0 = sub_mod_q(*c, *w);             // c0 = (c - w) mod q
        auto v0 = a_minus_bc_mod_q(*u0, *c0, r); // v0 = (𝑢0 - c0 ⋅ R) mod q
        auto v1 = a_minus_bc_mod_q(*u1, *w, r);  // v1 = (𝑢1 - c1 ⋅ R) mod q

        return make_unique<DisjunctiveChaumPedersenProof>(
          move(a0), move(b0), move(a1), move(b1), move(c0), move(w), move(c), move(v0), move(v1));
    }

    unique_ptr<DisjunctiveChaumPedersenProof>
    DisjunctiveChaumPedersenProof::make_zero(const ElGamalCiphertext &message,
                                             const PrecomputedSelection &precomputedValues,
                                             const ElementModP &k, const ElementModQ &q)
    {
        auto nonce = precomputedValues.getPartialEncryption()->getSecret();
        return make_zero(message, *nonce, precomputedValues.getRealCommitment()->clone(),
                         precomputedValues.getFakeCommitment()->clone(), k, q);
    }

    unique_ptr<DisjunctiveChaumPedersenProof> DisjunctiveChaumPedersenProof::make_zero(
      const ElGamalCiphertext &message, const ElementModQ &r,
      const unique_ptr<PrecomputedEncryption> real,
      const unique_ptr<PrecomputedFakeDisjuctiveCommitments> fake, const ElementModP &k,
      const ElementModQ &q)
    {
        // NIZKP for plaintext 0
        // (a0, b0) = (g^𝑢0 mod p, K^𝑢0 mod p)
        // (a1, b1) = (g^𝑢1 mod p, K^(𝑢1-w) mod p) <-- fake proof

        auto *alpha = message.getPad();
        auto *beta = message.getData();

        Log::trace("alpha: ", alpha->toHex());
        Log::trace("beta: ", beta->toHex());

        // Pick 3 random numbers in Q.
        auto u0 = real->getSecret();
        auto u1 = fake->getSecret1();
        auto w = fake->getSecret2();

        // Compute the NIZKP
        auto a0 = real->getPad()->clone();            // 𝑔^𝑢0 mod 𝑝
        auto b0 = real->getBlindingFactor()->clone(); // 𝐾^𝑢0 mod 𝑝
        auto a1 = fake->getPad()->clone();            // 𝑔^𝑢1 mod 𝑝
        auto b1 = fake->getDataZero()->clone();       // K^(𝑢1-w) mod p

        // Compute the challenge
        // c = H(HE;21,K,α,β,a0,b0,a1,b1). Ballot Selection Encryption Proof (selected/unselected) 3.3.5
        auto c =
          hash_elems({HashPrefix::get_prefix_selection_proof(), &const_cast<ElementModQ &>(q),
                      &const_cast<ElementModP &>(k), alpha, beta, a0.get(), b0.get(), a1.get(),
                      b1.get()}); // H(04,Q;K,α,β,a0,b0,a1,b1)

        auto c0 = sub_mod_q(*c, *w);             // c0 = (c - w) mod q
        auto v0 = a_minus_bc_mod_q(*u0, *c0, r); // v0 = (𝑢0 - c0 ⋅ R) mod q
        auto c1 = w->clone();                    // c1 = w
        auto v1 = a_minus_bc_mod_q(*u1, *w, r);  // v1 = (𝑢1 - c1 ⋅ R) mod q

        return make_unique<DisjunctiveChaumPedersenProof>(
          move(a0), move(b0), move(a1), move(b1), move(c0), move(c1), move(c), move(v0), move(v1));
    }

    unique_ptr<DisjunctiveChaumPedersenProof>
    DisjunctiveChaumPedersenProof::make_one(const ElGamalCiphertext &message, const ElementModQ &r,
                                            const ElementModP &k, const ElementModQ &q)
    {
        auto seed = rand_q();
        return make_one(message, r, k, q, *seed);
    }

    unique_ptr<DisjunctiveChaumPedersenProof>
    DisjunctiveChaumPedersenProof::make_one(const ElGamalCiphertext &message, const ElementModQ &r,
                                            const ElementModP &k, const ElementModQ &q,
                                            const ElementModQ &seed)
    {
        // NIZKP for plaintext 1
        // (a0, b0) = (g^𝑢0 mod p, K^(w+𝑢0) mod p) <-- fake proof
        // (a1, b1) = (g^𝑢1 mod p, K^𝑢1 mod p)

        auto *alpha = message.getPad();
        auto *beta = message.getData();

        // Pick three random numbers in Q.
        auto nonces = make_unique<Nonces>(seed, "disjoint-chaum-pedersen-proof");
        auto u0 = nonces->get(0);
        auto u1 = nonces->get(1);
        auto w = nonces->get(2);

        auto a0 = g_pow_p(*u0);                      // 𝑔^𝑢0 mod 𝑝
        auto b0 = pow_mod_p(k, *add_mod_p(*w, *u0)); // K^(w+𝑢0)  mod p
        auto a1 = g_pow_p(*u1);                      // g^𝑢1  mod p
        auto b1 = pow_mod_p(k, *u1);                 // K^𝑢1  mod p

        // Compute challenge
        // c = H(HE;21,K,α,β,a0,b0,a1,b1). Ballot Selection Encryption Proof (selected/unselected) 3.3.5
        auto c =
          hash_elems({HashPrefix::get_prefix_selection_proof(), &const_cast<ElementModQ &>(q),
                      &const_cast<ElementModP &>(k), alpha, beta, a0.get(), b0.get(), a1.get(),
                      b1.get()}); // H(04,Q;K,α,β,a0,b0,a1,b1)

        // auto c0 = *w                          // c0 = w  mod q
        auto c1 = sub_mod_q(*c, *w);             // c1 = (c - w)  mod q
        auto v0 = a_minus_bc_mod_q(*u0, *w, r);  // v0 = (𝑢0 - c0 ⋅ R)  mod q
        auto v1 = a_minus_bc_mod_q(*u1, *c1, r); // v1 = (𝑢1 - c1 ⋅ R)  mod q

        return make_unique<DisjunctiveChaumPedersenProof>(
          move(a0), move(b0), move(a1), move(b1), move(w), move(c1), move(c), move(v0), move(v1));
    }

    unique_ptr<DisjunctiveChaumPedersenProof>
    DisjunctiveChaumPedersenProof::make_one(const ElGamalCiphertext &message,
                                            const PrecomputedSelection &precomputedValues,
                                            const ElementModP &k, const ElementModQ &q)
    {
        auto nonce = precomputedValues.getPartialEncryption()->getSecret();
        return make_one(message, *nonce, precomputedValues.getRealCommitment()->clone(),
                        precomputedValues.getFakeCommitment()->clone(), k, q);
    }

    unique_ptr<DisjunctiveChaumPedersenProof> DisjunctiveChaumPedersenProof::make_one(
      const ElGamalCiphertext &message, const ElementModQ &r,
      const unique_ptr<PrecomputedEncryption> real,
      const unique_ptr<PrecomputedFakeDisjuctiveCommitments> fake, const ElementModP &k,
      const ElementModQ &q)
    {
        // NIZKP for plaintext 1
        // (a0, b0) = (g^𝑢0 mod p, K^(w+𝑢0) mod p) <-- fake proof
        // (a1, b1) = (g^𝑢1 mod p, K^𝑢1 mod p)

        auto *alpha = message.getPad();
        auto *beta = message.getData();

        Log::trace("alpha: ", alpha->toHex());
        Log::trace("beta: ", beta->toHex());

        // Pick three random numbers in Q.
        auto u0 = fake->getSecret1();
        auto u1 = real->getSecret();
        auto w = fake->getSecret2();

        auto a0 = fake->getPad()->clone();            // 𝑔^𝑢0 mod 𝑝
        auto b0 = fake->getDataOne()->clone();        // K^(w+𝑢0) mod p
        auto a1 = real->getPad()->clone();            // 𝑔^𝑢1 mod 𝑝
        auto b1 = real->getBlindingFactor()->clone(); // 𝐾^𝑢1 mod 𝑝

        // Compute challenge
        // c = H(HE;21,K,α,β,a0,b0,a1,b1). Ballot Selection Encryption Proof (selected/unselected) 3.3.5
        auto c = hash_elems({HashPrefix::get_prefix_selection_proof(),
                             &const_cast<ElementModQ &>(q), &const_cast<ElementModP &>(k), alpha,
                             beta, a0.get(), b0.get(), a1.get(), b1.get()});

        auto c0 = w->clone();                    // c0 = w  mod q
        auto c1 = sub_mod_q(*c, *w);             // c1 = (c - w)  mod q
        auto v0 = a_minus_bc_mod_q(*u0, *w, r);  // v0 = (𝑢0 - c0 ⋅ R)  mod q
        auto v1 = a_minus_bc_mod_q(*u1, *c1, r); // v1 = (𝑢1 - c1 ⋅ R)  mod q

        return make_unique<DisjunctiveChaumPedersenProof>(
          move(a0), move(b0), move(a1), move(b1), move(c0), move(c1), move(c), move(v0), move(v1));
    }

#pragma endregion

#pragma region RangedChaumPedersenProof

    struct RangedChaumPedersenProof::Impl {
        uint64_t rangeLimit;

        // the joint challenge Equation (56) in the v2.0.0 spec
        // c = H(HE;0x21,K,α ̄,β ̄,a0,b0,a1,b1,...,aL,bL)
        unique_ptr<ElementModQ> challenge;
        map<uint64_t, unique_ptr<ZeroKnowledgeProof>> integerProofs;

        Impl(uint64_t inRangeLimit, unique_ptr<ElementModQ> inChallenge,
             map<uint64_t, unique_ptr<ZeroKnowledgeProof>> inProofs)
            : rangeLimit(inRangeLimit), challenge(move(inChallenge)), integerProofs(move(inProofs))
        {
        }

        [[nodiscard]] unique_ptr<RangedChaumPedersenProof::Impl> clone() const
        {
            map<uint64_t, unique_ptr<ZeroKnowledgeProof>> _proofs;
            for (auto &proof : integerProofs) {
                _proofs[proof.first] = proof.second->clone();
            }
            return make_unique<RangedChaumPedersenProof::Impl>(rangeLimit, challenge->clone(),
                                                               move(_proofs));
        }

        // get hashable commitments for the integer proofs
        // if the commitment is not present, recompute it using the public values
        vector<reference_wrapper<CryptoHashable>>
        getHashableCommitments(const ElGamalCiphertext &message, const ElementModP &k) const
        {
            vector<reference_wrapper<CryptoHashable>> commitments;
            for (auto &proof : integerProofs) {
                if (!proof.second->commitment.has_value()) {
                    // recompute using the publically known values
                    auto recomputedCommitment = recomputeCommitment(
                      message, *proof.second->challenge, *proof.second->response, proof.first, k);
                    proof.second->commitment = move(recomputedCommitment);
                }
                commitments.emplace_back(
                  static_cast<CryptoHashable &>(*proof.second->commitment.value()));
            }
            return commitments;
        }

        // get the challenges for the integer proofs
        vector<reference_wrapper<ElementModQ>> getChallenges() const
        {
            vector<reference_wrapper<ElementModQ>> challengeValues;
            for (auto &proof : integerProofs) {
                challengeValues.emplace_back(*proof.second->challenge);
            }
            return challengeValues;
        }

        static std::unique_ptr<ElGamalCiphertext>
        recomputeCommitment(const ElGamalCiphertext &message, const ElementModQ &cj,
                            const ElementModQ &vj, uint64_t j, const ElementModP &k)
        {
            auto *alpha = message.getPad();
            auto *beta = message.getData();

            // 𝑎 = 𝑔^𝑉 ⋅ 𝐴^𝐶 mod 𝑝
            auto aj = mul_mod_p(*g_pow_p(vj), *pow_mod_p(*alpha, cj));

            // w = v - jc
            auto w = sub_mod_q(vj, *mul_mod_q(*ElementModQ::fromUint64(j), cj));

            // 𝑏  = 𝐾^w ⋅ 𝐵^𝐶 mod 𝑝
            auto bj = mul_mod_p(*pow_mod_p(k, *w), *pow_mod_p(*beta, cj));

            return make_unique<ElGamalCiphertext>(move(aj), move(bj));
        }

        // validate a specific proof at position j against the message
        static ValidationResult isValid(const ElGamalCiphertext &message,
                                        const ZeroKnowledgeProof &proof, uint64_t j,
                                        const ElementModP &k)
        {
            // if we don't have a commitment, we can't validate
            // so we explicitly return false, but this is expected behavior
            // when trying to validate against a proof that has been deserialized
            // from an election record that does not include the commitment values
            if (!proof.commitment.has_value()) {
                Log::trace("RangedChaumPedersenProof::Impl::isValid: inconclusive integer proof " +
                           to_string(j));
                return ValidationResult{false, {"j: " + to_string(j) + " inconclusive commitment"}};
            }

            // recompute the commitment using the publically known values
            auto recomputedCommitment =
              recomputeCommitment(message, *proof.challenge, *proof.response, j, k);

            auto aj = *proof.commitment.value()->getPad();
            auto bj = *proof.commitment.value()->getData();
            auto cj = *proof.challenge;
            auto vj = *proof.response;

            // Verification 6.3
            // 𝑎j = 𝑔^𝑉j ⋅ 𝐴^𝐶j mod 𝑝
            auto consistent_gv = (aj == *recomputedCommitment->getPad());

            // Verification 6.4
            // 𝑏j = 𝐾^wj ⋅ 𝐵^𝐶j mod 𝑝
            auto consistent_kv = (bj == *recomputedCommitment->getData());

            // Verification 6.A
            auto in_range_aj = aj.isInBounds();
            auto in_range_bj = bj.isInBounds();

            // Verification 6.B
            auto in_range_cj = cj.isInBounds();

            // Verification 6.C
            auto in_range_vj = vj.isInBounds();

            if (!consistent_gv || !consistent_kv) {
                auto jstring = to_string(j);
                Log::debug("RangedChaumPedersenProof::Impl::isValid: invalid integer proof " +
                           jstring);
                return ValidationResult{
                  false,
                  {
                    "j: " + jstring + " consistent_gv: " + to_string(consistent_gv),
                    "j: " + jstring + " consistent_kv: " + to_string(consistent_kv),
                    "j: " + jstring + " in_range_aj: " + to_string(in_range_aj),
                    "j: " + jstring + " in_range_bj: " + to_string(in_range_bj),
                    "j: " + jstring + " in_range_cj: " + to_string(in_range_cj),
                    "j: " + jstring + " in_range_vj: " + to_string(in_range_vj),
                  }};
            }
            return ValidationResult{true, {}};
        }

        // validate the integer proofs against the message
        ValidationResult isValid(const ElGamalCiphertext &message, const ElementModP &k)
        {
            bool proofsAreValid = true;
            std::vector<std::string> messages;
            for (uint64_t i = 0; i < this->rangeLimit; i++) {
                auto proof = *this->integerProofs[i];
                auto isInclonclusive = !proof.commitment.has_value();
                auto validationResult = isValid(message, proof, i, k);

                // if the proof is conclusively invalid, meanining it has a commitment
                // and the commitment does not match the recomputed commitment
                // then we can return false immediately.
                // this happens when we are validating a proof that was constructed
                // from an election record that does include the commitment values
                // or when we are validating a proof that was constructed from a
                // direct encryption operation as part of the encryption process.
                if (!validationResult.isValid & !isInclonclusive) {
                    proofsAreValid = false;
                    messages.push_back("proof for selection " + std::to_string(i) + " is invalid");
                }

                // but if it is inconclusive, we just log the message
                // and allow the loop to continue.
                // this happens when we are validating a proof that was constructed
                // from an election record that does not include the commitment values
                if (!validationResult.isValid) {
                    for (const auto &message : validationResult.messages) {
                        messages.push_back(message);
                    }
                }
            }
            return ValidationResult{proofsAreValid, messages};
        }
    };

    // Lifecycle Methods

    RangedChaumPedersenProof::RangedChaumPedersenProof(const RangedChaumPedersenProof &other)
        : pimpl(other.pimpl->clone())
    {
    }

    RangedChaumPedersenProof::RangedChaumPedersenProof(RangedChaumPedersenProof &&other)
        : pimpl(move(other.pimpl))
    {
    }

    RangedChaumPedersenProof::RangedChaumPedersenProof(
      uint64_t inRangeLimit, unique_ptr<ElementModQ> challenge,
      map<uint64_t, unique_ptr<ZeroKnowledgeProof>> inProofs)
        : pimpl(new Impl(inRangeLimit, move(challenge), move(inProofs)))
    {
    }

    RangedChaumPedersenProof::~RangedChaumPedersenProof() = default;

    // Operator Overloads

    RangedChaumPedersenProof &RangedChaumPedersenProof::operator=(RangedChaumPedersenProof other)
    {
        swap(pimpl, other.pimpl);
        return *this;
    }

    // Property Getters

    uint64_t RangedChaumPedersenProof::getRangeLimit() const { return pimpl->rangeLimit; }
    ElementModQ *RangedChaumPedersenProof::getChallenge() const { return pimpl->challenge.get(); }
    ZeroKnowledgeProof *RangedChaumPedersenProof::getProofAtIndex(uint64_t index) const
    {
        return pimpl->integerProofs.at(index).get();
    }

    std::vector<std::reference_wrapper<ZeroKnowledgeProof>>
    RangedChaumPedersenProof::getProofs() const
    {
        return referenceWrap(pimpl->integerProofs);
    }

    // Public Static Methods

    std::unique_ptr<RangedChaumPedersenProof>
    RangedChaumPedersenProof::make(const ElGamalCiphertext &message, const ElementModQ &r,
                                   uint64_t selected, uint64_t maxLimit, const ElementModP &k,
                                   const ElementModQ &q, const string &hashPrefix)

    {
        auto seed = rand_q();
        return make(message, r, selected, maxLimit, k, q, hashPrefix, *seed);
    }

    /// <summary>
    /// Note that while mathematically we can recompute the individual integer commitments
    /// and then hash them to verify the proof, in this implementation we keep the commitments
    /// made for each integer proof so that we can compare them during verification
    /// which allows us to know which one failed.
    /// </summary>
    unique_ptr<RangedChaumPedersenProof> RangedChaumPedersenProof::make(
      const ElGamalCiphertext &message, const ElementModQ &r, uint64_t selected, uint64_t maxLimit,
      const ElementModP &k, const ElementModQ &q, const string &hashPrefix, const ElementModQ &seed)
    {
        Log::trace("RangedChaumPedersenProof:: making proof");

        auto *alpha = message.getPad();
        auto *beta = message.getData();
        auto l = ElementModQ::fromUint64(selected);

        auto nonces = make_unique<Nonces>(seed, "ranged-chaum-pedersen-proof");

        map<uint64_t, unique_ptr<ElGamalCiphertext>> commitments;
        map<uint64_t, unique_ptr<ElementModQ>> challenges;

        // Compute commitments
        for (uint64_t i = 0; i < maxLimit; i++) {
            auto u = nonces->get(i);
            auto a = g_pow_p(*u); // 𝑔^𝑢 mod 𝑝

            unique_ptr<ElementModQ> cj;
            unique_ptr<ElementModQ> tj;
            if (i == selected) {
                // create the real proof
                cj = ZERO_MOD_Q().clone();
                tj = make_unique<ElementModQ>(*u);
            } else {
                // create a fake proof
                auto j = ElementModQ::fromUint64(i);

                // 𝑢 + (𝑙 − 𝑗) ⋅ 𝑐𝑗 mod 𝑞
                cj = nonces->get(maxLimit + i + 1);
                tj = add_mod_q(*u, *mul_mod_q(*sub_mod_q(*l, *j), *cj));
            }

            auto b = pow_mod_p(k, *tj); // 𝐾^tj mod 𝑝

            commitments[i] = make_unique<ElGamalCiphertext>(move(a), move(b));
            challenges[i] = move(cj);
        }

        // compute the joint challenge

        // c = H(HE;21,K,α ̄,β ̄,a0,b0,a1,b1,...,aL,bL). Ballot Contest Limit Encryption Proof 3.3.8
        auto commitmentReferences = referenceWrap<CryptoHashable>(commitments);
        auto c = hash_elems({&const_cast<ElementModQ &>(q), hashPrefix,
                             &const_cast<ElementModP &>(k), alpha, beta, commitmentReferences});

        // Compute the challenge for the selected value
        // and replace it in the challenges map
        auto c_sum = add_mod_q(referenceWrap(challenges));
        challenges[selected] = sub_mod_q(*c, *c_sum); // 𝑐𝑙 = 𝑐 − ∑𝑐𝑗 mod 𝑞

        // Compute the responses
        map<uint64_t, unique_ptr<ZeroKnowledgeProof>> responses;
        for (uint64_t i = 0; i < maxLimit; i++) {
            auto u = nonces->get(i);
            auto cjR = mul_mod_q(*challenges[i], r);
            auto vj = sub_mod_q(*u, *cjR); // 𝑢 − 𝑐 ⋅ 𝑅 mod 𝑞
            responses[i] =
              make_unique<ZeroKnowledgeProof>(move(commitments[i]), move(challenges[i]), move(vj));
        }

        return make_unique<RangedChaumPedersenProof>(maxLimit, move(c), move(responses));
    }

    // Public Methods

    ValidationResult RangedChaumPedersenProof::isValid(const ElGamalCiphertext &message,
                                                       const ElementModP &k, const ElementModQ &q,
                                                       const std::string &hashPrefix)
    {
        auto *alpha = message.getPad();
        auto *beta = message.getData();

        // validate the integer proofs against the message
        auto validationResult = pimpl->isValid(message, k);

        auto commitments = pimpl->getHashableCommitments(message, k);

        // Compute the challenge
        // TODO: change the HashPrefix to an input param since it can also be
        // use for selection proofs

        // Verification 6.5
        // c = H(HE;0x21,K,α ̄,β ̄,a0,b0,a1,b1,...,aL,bL), Ballot Contest Limit Encryption Proof 3.3.8
        auto computedChallenge =
          hash_elems({&const_cast<ElementModQ &>(q), hashPrefix, &const_cast<ElementModP &>(k),
                      alpha, beta, commitments});
        auto consistent_c = (*pimpl->challenge == *computedChallenge);

        if (!consistent_c) {
            validationResult.isValid = false;
            validationResult.messages.push_back("- Verification 6.5: invalid computed challenge");
        }

        // print out the error messages if the proof is invalid
        if (!validationResult.isValid) {
            Log::info("RangedChaumPedersenProof::isValid: found an invalid Range-Bound "
                      "Chaum-Pedersen proof");
            for (const auto &message : validationResult.messages) {
                Log::debug(message);
            }
        }

        return validationResult;
    }
#pragma endregion

#pragma region ConstantChaumPedersenProof

    struct ConstantChaumPedersenProof::Impl {
        unique_ptr<ElementModP> pad;
        unique_ptr<ElementModP> data;
        unique_ptr<ElementModQ> challenge;
        unique_ptr<ElementModQ> response;
        uint64_t constant;

        Impl(unique_ptr<ElementModP> pad, unique_ptr<ElementModP> data,
             unique_ptr<ElementModQ> challenge, unique_ptr<ElementModQ> response, uint64_t constant)
            : pad(move(pad)), data(move(data)), challenge(move(challenge)), response(move(response))
        {
            this->constant = constant;
        }

        [[nodiscard]] unique_ptr<ConstantChaumPedersenProof::Impl> clone() const
        {
            auto _pad = make_unique<ElementModP>(*pad);
            auto _data = make_unique<ElementModP>(*data);
            auto _challenge = make_unique<ElementModQ>(*challenge);
            auto _response = make_unique<ElementModQ>(*response);

            return make_unique<ConstantChaumPedersenProof::Impl>(
              move(_pad), move(_data), move(_challenge), move(_response), constant);
        }
    };

    // Lifecycle Methods

    ConstantChaumPedersenProof::ConstantChaumPedersenProof(const ConstantChaumPedersenProof &other)
        : pimpl(other.pimpl->clone())
    {
    }

    ConstantChaumPedersenProof::ConstantChaumPedersenProof(unique_ptr<ElementModP> pad,
                                                           unique_ptr<ElementModP> data,
                                                           unique_ptr<ElementModQ> challenge,
                                                           unique_ptr<ElementModQ> response,
                                                           uint64_t constant)
        : pimpl(new Impl(move(pad), move(data), move(challenge), move(response), constant))
    {
    }

    ConstantChaumPedersenProof::~ConstantChaumPedersenProof() = default;

    // Operator Overloads

    ConstantChaumPedersenProof &
    ConstantChaumPedersenProof::operator=(ConstantChaumPedersenProof other)
    {
        swap(pimpl, other.pimpl);
        return *this;
    }

    // Property Getters

    ElementModP *ConstantChaumPedersenProof::getPad() const { return pimpl->pad.get(); }
    ElementModP *ConstantChaumPedersenProof::getData() const { return pimpl->data.get(); }
    ElementModQ *ConstantChaumPedersenProof::getChallenge() const { return pimpl->challenge.get(); }
    ElementModQ *ConstantChaumPedersenProof::getResponse() const { return pimpl->response.get(); }
    uint64_t ConstantChaumPedersenProof::getConstant() const { return pimpl->constant; }

    // Public Static Methods

    unique_ptr<ConstantChaumPedersenProof>
    ConstantChaumPedersenProof::make(const ElGamalCiphertext &message, const ElementModQ &r,
                                     const ElementModP &k, const ElementModQ &seed,
                                     const ElementModQ &hash_header, uint64_t constant,
                                     bool shouldUsePrecomputedValues /* = false */)
    {
        Log::trace("ConstantChaumPedersenProof:: making proof");
        auto *alpha = message.getPad();
        auto *beta = message.getData();

        unique_ptr<ElementModQ> u;

        // Compute the NIZKP
        unique_ptr<ElementModP> a; //𝑔^𝑢 mod 𝑝
        unique_ptr<ElementModP> b; // 𝐾^𝑢 mod 𝑝

        if (shouldUsePrecomputedValues) {
            Log::debug("ConstantChaumPedersenProof:: using precomputed values. Your seed value is "
                       "ignored and is no longer deterministic.");
            // check if the are precompute values rather than doing the exponentiations here
            auto triple = PrecomputeBufferContext::popPrecomputedEncryption();
            if (triple != nullptr && triple.has_value()) {
                u = triple.value()->getSecret()->clone();
                a = triple.value()->getPad()->clone();
                b = triple.value()->getBlindingFactor()->clone();
            }
        }
        // if there are no precomputed values, do the exponentiations here
        if (u == nullptr || a == nullptr || b == nullptr) {
            // Derive nonce from seed and the constant string below
            auto nonces = make_unique<Nonces>(seed, "constant-chaum-pedersen-proof");
            u = nonces->get(0);
            a = g_pow_p(*u);      //𝑔^𝑢 mod 𝑝
            b = pow_mod_p(k, *u); // 𝐾^𝑢 mod 𝑝
        }

        // sha256(𝑄', A, B, a, b)
        auto c =
          hash_elems({&const_cast<ElementModQ &>(hash_header), alpha, beta, a.get(), b.get()});
        auto v = a_plus_bc_mod_q(*u, *c, r);

        return make_unique<ConstantChaumPedersenProof>(move(a), move(b), move(c), move(v),
                                                       constant);
    }

    // Public Methods

    bool ConstantChaumPedersenProof::isValid(const ElGamalCiphertext &message, const ElementModP &k,
                                             const ElementModQ &q)
    {
        Log::trace("ConstantChaumPedersenProof::isValid: checking validity");
        auto *alpha = message.getPad();
        auto *beta = message.getData();

        auto *a_ptr = pimpl->pad.get();
        auto *b_ptr = pimpl->data.get();
        auto *c_ptr = pimpl->challenge.get();

        auto a = *pimpl->pad;
        auto b = *pimpl->data;
        auto c = *pimpl->challenge;
        auto v = *pimpl->response;
        auto constant = pimpl->constant;

        auto inBounds_alpha = alpha->isValidResidue();
        auto inBounds_beta = beta->isValidResidue();
        auto inBounds_a = a.isValidResidue();
        auto inBounds_b = b.isValidResidue();
        auto inBounds_c = c.isInBounds();
        auto inBounds_v = v.isInBounds();

        auto constant_q = ElementModQ::fromUint64(constant);

        auto consistent_c =
          (c == *hash_elems({&const_cast<ElementModQ &>(q), alpha, beta, a_ptr, b_ptr}));

        // 𝑔^𝑉 = 𝑎 ⋅ 𝐴^𝐶 mod 𝑝
        auto consistent_gv = (*g_pow_p(v) == *mul_mod_p(a, *pow_mod_p(*alpha, c)));

        // 𝑔^𝐿 ⋅ 𝐾^𝑣 = 𝑏 ⋅ 𝐵^𝐶 mod 𝑝
        auto consistent_kv = (*mul_mod_p(*g_pow_p(*mul_mod_p({c_ptr, constant_q.get()})),
                                         *pow_mod_p(k, v)) == *mul_mod_p(b, *pow_mod_p(*beta, c)));

        auto success = inBounds_alpha && inBounds_beta && inBounds_a && inBounds_b && inBounds_c &&
                       inBounds_v && consistent_c && consistent_gv && consistent_kv;

        if (!success) {

            map<string, bool> printMap{
              {"inBounds_alpha", inBounds_alpha}, {"inBounds_beta", inBounds_beta},
              {"inBounds_a", inBounds_a},         {"inBounds_b", inBounds_b},
              {"inBounds_c", inBounds_c},         {"inBounds_v", inBounds_v},
              {"consistent_c", consistent_c},     {"consistent_gv", consistent_gv},
              {"consistent_kv", consistent_kv},
            };

            Log::info("found an invalid Constant Chaum-Pedersen proof", printMap);

            Log::debug("k->get", k.toHex());
            Log::debug("q->get", q.toHex());
            Log::debug("alpha->get", alpha->toHex());
            Log::debug("beta->get", beta->toHex());
            Log::debug("a->get", a.toHex());
            Log::debug("b->get", b.toHex());
            Log::debug("c->get", c.toHex());
            Log::debug("v->get", v.toHex());

            return false;
        }
        Log::trace("ConstantChaumPedersenProof::isValid: TRUE!");
        return success;
    }
#pragma endregion

#pragma region ChaumPedersenProof

    struct ChaumPedersenProof::Impl {
        unique_ptr<ElGamalCiphertext> commitment;
        unique_ptr<ElementModQ> challenge;
        unique_ptr<ElementModQ> response;

        Impl(unique_ptr<ElGamalCiphertext> commitment, unique_ptr<ElementModQ> challenge,
             unique_ptr<ElementModQ> response)
            : commitment(move(commitment)), challenge(move(challenge)), response(move(response))
        {
        }

        [[nodiscard]] unique_ptr<ChaumPedersenProof::Impl> clone() const
        {
            auto _commitment = commitment->clone();
            auto _challenge = make_unique<ElementModQ>(*challenge);
            auto _response = make_unique<ElementModQ>(*response);

            return make_unique<ChaumPedersenProof::Impl>(move(_commitment), move(_challenge),
                                                         move(_response));
        }
    };

    // Lifecycle Methods

    ChaumPedersenProof::ChaumPedersenProof(const ChaumPedersenProof &other)
        : pimpl(other.pimpl->clone())
    {
    }

    ChaumPedersenProof::ChaumPedersenProof(unique_ptr<ElGamalCiphertext> commitment,
                                           unique_ptr<ElementModQ> challenge,
                                           unique_ptr<ElementModQ> response)
        : pimpl(new Impl(move(commitment), move(challenge), move(response)))
    {
    }

    ChaumPedersenProof::ChaumPedersenProof(unique_ptr<ElementModP> pad,
                                           unique_ptr<ElementModP> data,
                                           unique_ptr<ElementModQ> challenge,
                                           unique_ptr<ElementModQ> response)
        : pimpl(new Impl(make_unique<ElGamalCiphertext>(move(pad), move(data)), move(challenge),
                         move(response)))
    {
    }

    ChaumPedersenProof::~ChaumPedersenProof() = default;

    // Operator Overloads

    ChaumPedersenProof &ChaumPedersenProof::operator=(ChaumPedersenProof other)
    {
        swap(pimpl, other.pimpl);
        return *this;
    }

    // Property Getters

    ElementModP *ChaumPedersenProof::getPad() const { return pimpl->commitment->getPad(); }
    ElementModP *ChaumPedersenProof::getData() const { return pimpl->commitment->getData(); }
    ElementModQ *ChaumPedersenProof::getChallenge() const { return pimpl->challenge.get(); }
    ElementModQ *ChaumPedersenProof::getResponse() const { return pimpl->response.get(); }

    // Public Methods

    bool ChaumPedersenProof::isValid(const ElGamalCiphertext &message, const ElementModP &k,
                                     const ElementModP &m, const ElementModQ &q)
    {
        Log::trace("ChaumPedersenProof::isValid: checking validity");
        auto *alpha = message.getPad();
        auto *beta = message.getData();

        auto *a_ptr = pimpl->commitment->getPad();
        auto *b_ptr = pimpl->commitment->getData();
        auto *c_ptr = pimpl->challenge.get();

        auto a = *pimpl->commitment->getPad();
        auto b = *pimpl->commitment->getData();
        auto c = *pimpl->challenge;
        auto v = *pimpl->response;

        auto inBounds_alpha = alpha->isValidResidue();
        auto inBounds_beta = beta->isValidResidue();
        auto inBounds_a = a.isValidResidue();
        auto inBounds_b = b.isValidResidue();
        auto inBounds_c = c.isInBounds();
        auto inBounds_v = v.isInBounds();

        // TODO: actual implementation

        // auto consistent_c =
        //   (c == *hash_elems({&const_cast<ElementModQ &>(q), alpha, beta, a_ptr, b_ptr}));

        // // 𝑔^𝑉 = 𝑎 ⋅ 𝐴^𝐶 mod 𝑝
        // auto consistent_gv = (*g_pow_p(v) == *mul_mod_p(a, *pow_mod_p(*alpha, c)));

        // // 𝑔^𝐿 ⋅ 𝐾^𝑣 = 𝑏 ⋅ 𝐵^𝐶 mod 𝑝
        // auto consistent_kv = (*mul_mod_p(*g_pow_p(*mul_mod_p({c_ptr, constant_q.get()})),
        //                                  *pow_mod_p(k, v)) == *mul_mod_p(b, *pow_mod_p(*beta, c)));

        // auto success = inBounds_alpha && inBounds_beta && inBounds_a && inBounds_b && inBounds_c &&
        //                inBounds_v && consistent_c && consistent_gv && consistent_kv;

        // if (!success) {

        //     map<string, bool> printMap{
        //       {"inBounds_alpha", inBounds_alpha}, {"inBounds_beta", inBounds_beta},
        //       {"inBounds_a", inBounds_a},         {"inBounds_b", inBounds_b},
        //       {"inBounds_c", inBounds_c},         {"inBounds_v", inBounds_v},
        //       {"consistent_c", consistent_c},     {"consistent_gv", consistent_gv},
        //       {"consistent_kv", consistent_kv},
        //     };

        //     Log::info("found an invalid Constant Chaum-Pedersen proof", printMap);

        //     Log::debug("k->get", k.toHex());
        //     Log::debug("q->get", q.toHex());
        //     Log::debug("alpha->get", alpha->toHex());
        //     Log::debug("beta->get", beta->toHex());
        //     Log::debug("a->get", a.toHex());
        //     Log::debug("b->get", b.toHex());
        //     Log::debug("c->get", c.toHex());
        //     Log::debug("v->get", v.toHex());

        //     return false;
        // }
        // Log::trace("ConstantChaumPedersenProof::isValid: TRUE!");
        return true;
    }
#pragma endregion

} // namespace electionguard
