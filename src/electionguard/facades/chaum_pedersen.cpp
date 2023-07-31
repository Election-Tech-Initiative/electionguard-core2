#include "electionguard/chaum_pedersen.hpp"

#include "../log.hpp"
#include "electionguard/group.hpp"
#include "variant_cast.hpp"

extern "C" {
#include "electionguard/chaum_pedersen.h"
}

using electionguard::ChaumPedersenProof;
using electionguard::ConstantChaumPedersenProof;
using electionguard::DisjunctiveChaumPedersenProof;
using electionguard::ElementModP;
using electionguard::ElementModQ;
using electionguard::ElGamalCiphertext;
using electionguard::Log;
using electionguard::RangedChaumPedersenProof;

using std::make_unique;
using std::move;

#pragma region DisjunctiveChaumPedersenProof

eg_electionguard_status_t
eg_disjunctive_chaum_pedersen_proof_free(eg_disjunctive_chaum_pedersen_proof_t *handle)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    delete AS_TYPE(DisjunctiveChaumPedersenProof,
                   handle); // NOLINT(cppcoreguidelines-owning-memory)
    handle = nullptr;
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t
eg_disjunctive_chaum_pedersen_proof_get_zero_pad(eg_disjunctive_chaum_pedersen_proof_t *handle,
                                                 eg_element_mod_p_t **out_element_ref)
{
    auto *element = AS_TYPE(DisjunctiveChaumPedersenProof, handle)->getProofZeroPad();
    *out_element_ref = AS_TYPE(eg_element_mod_p_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t
eg_disjunctive_chaum_pedersen_proof_get_zero_data(eg_disjunctive_chaum_pedersen_proof_t *handle,
                                                  eg_element_mod_p_t **out_element_ref)
{
    auto *element = AS_TYPE(DisjunctiveChaumPedersenProof, handle)->getProofZeroData();
    *out_element_ref = AS_TYPE(eg_element_mod_p_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t
eg_disjunctive_chaum_pedersen_proof_get_one_pad(eg_disjunctive_chaum_pedersen_proof_t *handle,
                                                eg_element_mod_p_t **out_element_ref)
{
    auto *element = AS_TYPE(DisjunctiveChaumPedersenProof, handle)->getProofOnePad();
    *out_element_ref = AS_TYPE(eg_element_mod_p_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t
eg_disjunctive_chaum_pedersen_proof_get_one_data(eg_disjunctive_chaum_pedersen_proof_t *handle,
                                                 eg_element_mod_p_t **out_element_ref)
{
    auto *element = AS_TYPE(DisjunctiveChaumPedersenProof, handle)->getProofOneData();
    *out_element_ref = AS_TYPE(eg_element_mod_p_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_disjunctive_chaum_pedersen_proof_get_zero_challenge(
  eg_disjunctive_chaum_pedersen_proof_t *handle, eg_element_mod_q_t **out_element_ref)
{
    auto *element = AS_TYPE(DisjunctiveChaumPedersenProof, handle)->getProofZeroChallenge();
    *out_element_ref = AS_TYPE(eg_element_mod_q_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t
eg_disjunctive_chaum_pedersen_proof_get_one_challenge(eg_disjunctive_chaum_pedersen_proof_t *handle,
                                                      eg_element_mod_q_t **out_element_ref)
{
    auto *element = AS_TYPE(DisjunctiveChaumPedersenProof, handle)->getProofOneChallenge();
    *out_element_ref = AS_TYPE(eg_element_mod_q_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t
eg_disjunctive_chaum_pedersen_proof_get_challenge(eg_disjunctive_chaum_pedersen_proof_t *handle,
                                                  eg_element_mod_q_t **out_element_ref)
{
    auto *element = AS_TYPE(DisjunctiveChaumPedersenProof, handle)->getChallenge();
    *out_element_ref = AS_TYPE(eg_element_mod_q_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t
eg_disjunctive_chaum_pedersen_proof_get_zero_response(eg_disjunctive_chaum_pedersen_proof_t *handle,
                                                      eg_element_mod_q_t **out_element_ref)
{
    auto *element = AS_TYPE(DisjunctiveChaumPedersenProof, handle)->getProofZeroResponse();
    *out_element_ref = AS_TYPE(eg_element_mod_q_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t
eg_disjunctive_chaum_pedersen_proof_get_one_response(eg_disjunctive_chaum_pedersen_proof_t *handle,
                                                     eg_element_mod_q_t **out_element_ref)
{
    auto *element = AS_TYPE(DisjunctiveChaumPedersenProof, handle)->getProofOneResponse();
    *out_element_ref = AS_TYPE(eg_element_mod_q_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t
eg_disjunctive_chaum_pedersen_proof_make(eg_elgamal_ciphertext_t *in_message,
                                         eg_element_mod_q_t *in_r, eg_element_mod_p_t *in_k,
                                         eg_element_mod_q_t *in_q, uint64_t in_plaintext,
                                         eg_disjunctive_chaum_pedersen_proof_t **out_handle)
{
    if (in_message == nullptr || in_r == nullptr || in_k == nullptr || in_q == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    auto *message = AS_TYPE(ElGamalCiphertext, in_message);
    auto *r = AS_TYPE(ElementModQ, in_r);
    auto *k = AS_TYPE(ElementModP, in_k);
    auto *q = AS_TYPE(ElementModQ, in_q);

    try {
        auto proof = DisjunctiveChaumPedersenProof::make(*message, *r, *k, *q, in_plaintext);
        *out_handle = AS_TYPE(eg_disjunctive_chaum_pedersen_proof_t, proof.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(":eg_disjunctive_chaum_pedersen_proof_make", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_disjunctive_chaum_pedersen_proof_make_deterministic(
  eg_elgamal_ciphertext_t *in_message, eg_element_mod_q_t *in_r, eg_element_mod_p_t *in_k,
  eg_element_mod_q_t *in_q, eg_element_mod_q_t *in_seed, uint64_t in_plaintext,
  eg_disjunctive_chaum_pedersen_proof_t **out_handle)
{
    if (in_message == nullptr || in_r == nullptr || in_k == nullptr || in_q == nullptr ||
        in_seed == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    auto *message = AS_TYPE(ElGamalCiphertext, in_message);
    auto *r = AS_TYPE(ElementModQ, in_r);
    auto *k = AS_TYPE(ElementModP, in_k);
    auto *q = AS_TYPE(ElementModQ, in_q);
    auto *seed = AS_TYPE(ElementModQ, in_seed);

    try {
        auto proof = DisjunctiveChaumPedersenProof::make(*message, *r, *k, *q, *seed, in_plaintext);
        *out_handle = AS_TYPE(eg_disjunctive_chaum_pedersen_proof_t, proof.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(":eg_disjunctive_chaum_pedersen_proof_make_deterministic", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

bool eg_disjunctive_chaum_pedersen_proof_is_valid(eg_disjunctive_chaum_pedersen_proof_t *handle,
                                                  eg_elgamal_ciphertext_t *in_ciphertext,
                                                  eg_element_mod_p_t *in_k,
                                                  eg_element_mod_q_t *in_q)
{
    if (handle == nullptr || in_ciphertext == nullptr || in_k == nullptr || in_q == nullptr) {
        return false;
    }
    auto *ciphertext = AS_TYPE(ElGamalCiphertext, in_ciphertext);
    auto *k = AS_TYPE(ElementModP, in_k);
    auto *q = AS_TYPE(ElementModQ, in_q);
    return AS_TYPE(DisjunctiveChaumPedersenProof, handle)->isValid(*ciphertext, *k, *q);
}

#pragma endregion

#pragma region RangedChaumPedersenProof

eg_electionguard_status_t
eg_ranged_chaum_pedersen_proof_free(eg_ranged_chaum_pedersen_proof_t *handle)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    delete AS_TYPE(RangedChaumPedersenProof,
                   handle); // NOLINT(cppcoreguidelines-owning-memory)
    handle = nullptr;
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_ranged_chaum_pedersen_proof_make(
  eg_elgamal_ciphertext_t *in_message, eg_element_mod_q_t *in_r, uint64_t in_selected,
  uint64_t in_maxLimit, eg_element_mod_p_t *in_k, eg_element_mod_q_t *in_q,
  const char *in_hash_prefix, eg_ranged_chaum_pedersen_proof_t **out_handle)
{
    // TODO: Add validation to selected and maxLimit.
    if (in_message == nullptr || in_r == nullptr || in_k == nullptr || in_q == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    auto *message = AS_TYPE(ElGamalCiphertext, in_message);
    auto *r = AS_TYPE(ElementModQ, in_r);
    auto *k = AS_TYPE(ElementModP, in_k);
    auto *q = AS_TYPE(ElementModQ, in_q);

    try {
        auto hashPrefix = string(in_hash_prefix);
        auto proof = RangedChaumPedersenProof::make(*message, *r, in_selected, in_maxLimit, *k, *q,
                                                    hashPrefix);
        *out_handle = AS_TYPE(eg_ranged_chaum_pedersen_proof_t, proof.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_ranged_chaum_pedersen_proof_make_deterministic(
  eg_elgamal_ciphertext_t *in_message, eg_element_mod_q_t *in_r, uint64_t in_selected,
  uint64_t in_maxLimit, eg_element_mod_p_t *in_k, eg_element_mod_q_t *in_q,
  const char *in_hash_prefix, eg_element_mod_q_t *in_seed,
  eg_ranged_chaum_pedersen_proof_t **out_handle)
{
    // TODO: Add validation to selected and maxLimit.
    if (in_message == nullptr || in_r == nullptr || in_k == nullptr || in_q == nullptr ||
        in_seed == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    auto *message = AS_TYPE(ElGamalCiphertext, in_message);
    auto *r = AS_TYPE(ElementModQ, in_r);
    auto *k = AS_TYPE(ElementModP, in_k);
    auto *q = AS_TYPE(ElementModQ, in_q);
    auto *seed = AS_TYPE(ElementModQ, in_seed);

    try {
        auto hashPrefix = string(in_hash_prefix);
        auto proof = RangedChaumPedersenProof::make(*message, *r, in_selected, in_maxLimit, *k, *q,
                                                    hashPrefix, *seed);
        *out_handle = AS_TYPE(eg_ranged_chaum_pedersen_proof_t, proof.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t
eg_ranged_chaum_pedersen_proof_get_range_limit(eg_ranged_chaum_pedersen_proof_t *handle,
                                               uint64_t *out_element_ref)
{
    auto element = AS_TYPE(RangedChaumPedersenProof, handle)->getRangeLimit();
    *out_element_ref = element;
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t
eg_ranged_chaum_pedersen_proof_get_challenge(eg_ranged_chaum_pedersen_proof_t *handle,
                                             eg_element_mod_q_t **out_element_ref)
{
    auto *element = AS_TYPE(RangedChaumPedersenProof, handle)->getChallenge();
    *out_element_ref = AS_TYPE(eg_element_mod_q_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

// TODO: add a getter for integer proofs. This is a vector of ElementModQ,
// and we need to create a path to marshall a vector out of C layer.

// TODO: Create a getter for proof at an index.

bool eg_ranged_chaum_pedersen_proof_is_valid(eg_ranged_chaum_pedersen_proof_t *in_handle,
                                             eg_elgamal_ciphertext_t *in_ciphertext,
                                             eg_element_mod_p_t *in_k, eg_element_mod_q_t *in_q,
                                             const char *in_hash_prefix)
{
    if (in_handle == nullptr || in_ciphertext == nullptr || in_k == nullptr || in_q == nullptr) {
        return false;
    }

    try {
        auto *ciphertext = AS_TYPE(ElGamalCiphertext, in_ciphertext);
        auto *k = AS_TYPE(ElementModP, in_k);
        auto *q = AS_TYPE(ElementModQ, in_q);
        auto hashPrefix = string(in_hash_prefix);
        auto validationResult =
          AS_TYPE(RangedChaumPedersenProof, in_handle)->isValid(*ciphertext, *k, *q, hashPrefix);

        if (!validationResult.isValid) {
            string messageBuilder = "RangedChaumPedersenProof is invalid";
            for (int i = 0; i < validationResult.messages.size(); i++) {
                messageBuilder += "\n" + validationResult.messages[i];
            }
            Log::error(__func__, messageBuilder);
        }
        return validationResult.isValid;
    } catch (const std::exception &e) {
        Log::error(__func__, e);
        return false;
    }
}

#pragma endregion

#pragma region ConstantChaumPedersenProof

eg_electionguard_status_t
eg_constant_chaum_pedersen_proof_free(eg_constant_chaum_pedersen_proof_t *handle)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    delete AS_TYPE(ConstantChaumPedersenProof, handle); // NOLINT(cppcoreguidelines-owning-memory)
    handle = nullptr;
    return ELECTIONGUARD_STATUS_SUCCESS;
}

EG_API eg_electionguard_status_t eg_constant_chaum_pedersen_proof_get_pad(
  eg_constant_chaum_pedersen_proof_t *handle, eg_element_mod_p_t **out_element_ref)
{
    auto *element = AS_TYPE(ConstantChaumPedersenProof, handle)->getPad();
    *out_element_ref = AS_TYPE(eg_element_mod_p_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

EG_API eg_electionguard_status_t eg_constant_chaum_pedersen_proof_get_data(
  eg_constant_chaum_pedersen_proof_t *handle, eg_element_mod_p_t **out_element_ref)
{
    auto *element = AS_TYPE(ConstantChaumPedersenProof, handle)->getData();
    *out_element_ref = AS_TYPE(eg_element_mod_p_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

EG_API eg_electionguard_status_t eg_constant_chaum_pedersen_proof_get_challenge(
  eg_constant_chaum_pedersen_proof_t *handle, eg_element_mod_q_t **out_element_ref)
{
    auto *element = AS_TYPE(ConstantChaumPedersenProof, handle)->getChallenge();
    *out_element_ref = AS_TYPE(eg_element_mod_q_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

EG_API eg_electionguard_status_t eg_constant_chaum_pedersen_proof_get_response(
  eg_constant_chaum_pedersen_proof_t *handle, eg_element_mod_q_t **out_element_ref)
{
    auto *element = AS_TYPE(ConstantChaumPedersenProof, handle)->getResponse();
    *out_element_ref = AS_TYPE(eg_element_mod_q_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_constant_chaum_pedersen_proof_make(
  eg_elgamal_ciphertext_t *in_message, eg_element_mod_q_t *in_r, eg_element_mod_p_t *in_k,
  eg_element_mod_q_t *in_seed, eg_element_mod_q_t *in_hash_header, uint64_t in_constant,
  bool in_should_use_precomputed_values, eg_constant_chaum_pedersen_proof_t **out_handle)
{
    if (in_message == nullptr || in_r == nullptr || in_k == nullptr || in_seed == nullptr ||
        in_hash_header == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    auto *ciphertext = AS_TYPE(ElGamalCiphertext, in_message);
    auto *r = AS_TYPE(ElementModQ, in_r);
    auto *k = AS_TYPE(ElementModP, in_k);
    auto *seed = AS_TYPE(ElementModQ, in_seed);
    auto *hashHeader = AS_TYPE(ElementModQ, in_hash_header);

    try {
        auto proof = ConstantChaumPedersenProof::make(
          *ciphertext, *r, *k, *seed, *hashHeader, in_constant, in_should_use_precomputed_values);
        *out_handle = AS_TYPE(eg_constant_chaum_pedersen_proof_t, proof.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(":eg_constant_chaum_pedersen_proof_make", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

bool eg_constant_chaum_pedersen_proof_is_valid(eg_constant_chaum_pedersen_proof_t *handle,
                                               eg_elgamal_ciphertext_t *in_ciphertext,
                                               eg_element_mod_p_t *in_k, eg_element_mod_q_t *in_q)
{
    if (handle == nullptr || in_ciphertext == nullptr || in_k == nullptr || in_q == nullptr) {
        return false;
    }
    auto *ciphertext = AS_TYPE(ElGamalCiphertext, in_ciphertext);
    auto *k = AS_TYPE(ElementModP, in_k);
    auto *q = AS_TYPE(ElementModQ, in_q);
    return AS_TYPE(ConstantChaumPedersenProof, handle)->isValid(*ciphertext, *k, *q);
}

#pragma endregion

#pragma region ChaumPedersenProof

eg_electionguard_status_t eg_chaum_pedersen_proof_free(eg_chaum_pedersen_proof_t *handle)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    delete AS_TYPE(ChaumPedersenProof, handle); // NOLINT(cppcoreguidelines-owning-memory)
    handle = nullptr;
    return ELECTIONGUARD_STATUS_SUCCESS;
}

EG_API eg_electionguard_status_t eg_chaum_pedersen_proof_get_pad(
  eg_chaum_pedersen_proof_t *handle, eg_element_mod_p_t **out_element_ref)
{
    auto *element = AS_TYPE(ChaumPedersenProof, handle)->getPad();
    *out_element_ref = AS_TYPE(eg_element_mod_p_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

EG_API eg_electionguard_status_t eg_chaum_pedersen_proof_get_data(
  eg_chaum_pedersen_proof_t *handle, eg_element_mod_p_t **out_element_ref)
{
    auto *element = AS_TYPE(ChaumPedersenProof, handle)->getData();
    *out_element_ref = AS_TYPE(eg_element_mod_p_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

EG_API eg_electionguard_status_t eg_chaum_pedersen_proof_get_challenge(
  eg_chaum_pedersen_proof_t *handle, eg_element_mod_q_t **out_element_ref)
{
    auto *element = AS_TYPE(ChaumPedersenProof, handle)->getChallenge();
    *out_element_ref = AS_TYPE(eg_element_mod_q_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

EG_API eg_electionguard_status_t eg_chaum_pedersen_proof_get_response(
  eg_chaum_pedersen_proof_t *handle, eg_element_mod_q_t **out_element_ref)
{
    auto *element = AS_TYPE(ChaumPedersenProof, handle)->getResponse();
    *out_element_ref = AS_TYPE(eg_element_mod_q_t, element);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_chaum_pedersen_proof_make(eg_elgamal_ciphertext_t *in_commitment,
                                                       eg_element_mod_q_t *in_challenge,
                                                       eg_element_mod_q_t *in_response,
                                                       eg_chaum_pedersen_proof_t **out_handle)
{
    if (in_commitment == nullptr || in_challenge == nullptr || in_response == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    auto commitment = AS_TYPE(ElGamalCiphertext, in_commitment)->clone();
    auto challenge = AS_TYPE(ElementModQ, in_challenge)->clone();
    auto response = AS_TYPE(ElementModQ, in_response)->clone();

    try {
        auto proof =
          make_unique<ChaumPedersenProof>(move(commitment), move(challenge), move(response));
        *out_handle = AS_TYPE(eg_chaum_pedersen_proof_t, proof.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(":eg_constant_chaum_pedersen_proof_make", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}
bool eg_chaum_pedersen_proof_is_valid(eg_chaum_pedersen_proof_t *handle,
                                      eg_elgamal_ciphertext_t *in_ciphertext,
                                      eg_element_mod_p_t *in_k, eg_element_mod_p_t *in_m,
                                      eg_element_mod_q_t *in_q)
{
    if (handle == nullptr || in_ciphertext == nullptr || in_k == nullptr || in_m == nullptr ||
        in_q == nullptr) {
        return false;
    }
    auto *ciphertext = AS_TYPE(ElGamalCiphertext, in_ciphertext);
    auto *k = AS_TYPE(ElementModP, in_k);
    auto *m = AS_TYPE(ElementModP, in_m);
    auto *q = AS_TYPE(ElementModQ, in_q);
    return AS_TYPE(ChaumPedersenProof, handle)->isValid(*ciphertext, *k, *m, *q);
}

#pragma endregion
