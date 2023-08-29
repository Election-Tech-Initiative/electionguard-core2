#include "electionguard/encrypt.hpp"

#include "../log.hpp"
#include "convert.hpp"
#include "variant_cast.hpp"

#include <cerrno>
#include <exception>
#include <stdexcept>

extern "C" {
#include "electionguard/encrypt.h"
}

using electionguard::CiphertextBallot;
using electionguard::CiphertextBallotContest;
using electionguard::CiphertextBallotSelection;
using electionguard::CiphertextElectionContext;
using electionguard::CompactCiphertextBallot;
using electionguard::ContestDescriptionWithPlaceholders;
using electionguard::dynamicCopy;
using electionguard::ElementModP;
using electionguard::ElementModQ;
using electionguard::encryptBallot;
using electionguard::encryptContest;
using electionguard::EncryptionDevice;
using electionguard::EncryptionMediator;
using electionguard::InternalManifest;
using electionguard::Log;
using electionguard::PlaintextBallot;
using electionguard::PlaintextBallotContest;
using electionguard::PlaintextBallotSelection;
using electionguard::SelectionDescription;

using std::invalid_argument;
using std::make_unique;
using std::runtime_error;
using std::unique_ptr;

#pragma region EncryptionDevice

eg_electionguard_status_t eg_encryption_device_new(uint64_t in_device_uuid,
                                                   uint64_t in_session_uuid,
                                                   uint64_t in_launch_code, const char *in_location,
                                                   eg_encryption_device_t **out_handle)
{
    try {
        auto device = make_unique<EncryptionDevice>(in_device_uuid, in_session_uuid, in_launch_code,
                                                    string(in_location));
        *out_handle = AS_TYPE(eg_encryption_device_t, device.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(": eg_encryption_device_new", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_encryption_device_free(eg_encryption_device_t *handle)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    delete AS_TYPE(EncryptionDevice, handle); // NOLINT(cppcoreguidelines-owning-memory)
    handle = nullptr;
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_encryption_device_get_timestamp(eg_encryption_device_t *handle,
                                                             uint64_t *out_timestamp)
{

    try {
        *out_timestamp = AS_TYPE(EncryptionDevice, handle)->getTimestamp();
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(":eg_encryption_device_get_timestamp", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_encryption_device_get_uuid(eg_encryption_device_t *handle,
                                                        uint64_t *out_uuid)
{
    try {
        *out_uuid = AS_TYPE(EncryptionDevice, handle)->getDeviceUuid();
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(":eg_encryption_device_get_uuid", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_encryption_device_get_session_uuid(eg_encryption_device_t *handle,
                                                                uint64_t *out_session_id)
{
    try {
        *out_session_id = AS_TYPE(EncryptionDevice, handle)->getSessionUuid();
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(":eg_encryption_device_get_session_uuid", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_encryption_device_get_launch_code(eg_encryption_device_t *handle,
                                                               uint64_t *out_launch_code)
{
    try {
        *out_launch_code = AS_TYPE(EncryptionDevice, handle)->getLaunchCode();
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(":eg_encryption_device_get_launch_code", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_encryption_device_get_location(eg_encryption_device_t *handle,
                                                            char **out_location, uint64_t *out_size)
{
    try {
        auto location = AS_TYPE(EncryptionDevice, handle)->getLocation();
        size_t size = 0;
        *out_location = dynamicCopy(location, &size);
        *out_size = (uint64_t)size;
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(":eg_encryption_device_get_location", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_encryption_device_get_hash(eg_encryption_device_t *handle,
                                                        eg_element_mod_q_t **out_hash)
{
    try {
        auto hash = AS_TYPE(EncryptionDevice, handle)->getHash();
        *out_hash = AS_TYPE(eg_element_mod_q_t, hash.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(": eg_encryption_device_get_hash", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_encryption_device_from_json(char *in_data,
                                                         eg_encryption_device_t **out_handle)
{
    try {
        auto data = string(in_data);
        auto deserialized = EncryptionDevice::fromJson(data);
        *out_handle = AS_TYPE(eg_encryption_device_t, deserialized.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(":eg_encryption_device_from_json", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}
eg_electionguard_status_t eg_encryption_device_to_json(eg_encryption_device_t *handle,
                                                       char **out_data, uint64_t *out_size)
{
    try {
        auto *domain_type = AS_TYPE(EncryptionDevice, handle);
        auto data_string = domain_type->toJson();
        size_t size = 0;
        *out_data = dynamicCopy(data_string, &size);
        *out_size = (uint64_t)size;
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(":eg_encryption_device_to_json", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}
#pragma endregion

#pragma region EncryptionMediator

eg_electionguard_status_t eg_encryption_mediator_new(eg_internal_manifest_t *in_manifest,
                                                     eg_ciphertext_election_context_t *in_context,
                                                     eg_encryption_device_t *in_encryption_device,
                                                     eg_encryption_mediator_t **out_handle)
{
    try {
        auto *manifest = AS_TYPE(InternalManifest, in_manifest);
        auto *context = AS_TYPE(CiphertextElectionContext, in_context);
        auto *device = AS_TYPE(EncryptionDevice, in_encryption_device);
        auto mediator = make_unique<EncryptionMediator>(*manifest, *context, *device);

        *out_handle = AS_TYPE(eg_encryption_mediator_t, mediator.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(":eg_encryption_mediator_new", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_encryption_mediator_free(eg_encryption_mediator_t *handle)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    delete AS_TYPE(EncryptionMediator, handle); // NOLINT(cppcoreguidelines-owning-memory)
    handle = nullptr;
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_encryption_mediator_encrypt_ballot(
  eg_encryption_mediator_t *handle, eg_plaintext_ballot_t *in_plaintext,
  bool in_use_precomputed_values, eg_ciphertext_ballot_t **out_ciphertext_handle)
{
    try {
        auto *plaintext = AS_TYPE(PlaintextBallot, in_plaintext);
        auto ciphertext = AS_TYPE(EncryptionMediator, handle)
                            ->encrypt(*plaintext, false, in_use_precomputed_values);

        *out_ciphertext_handle = AS_TYPE(eg_ciphertext_ballot_t, ciphertext.release());
        return ELECTIONGUARD_STATUS_SUCCESS;

    } catch (const invalid_argument &e) {
        Log::error(":eg_encryption_mediator_encrypt_ballot", e);
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    } catch (const runtime_error &e) {
        Log::error(":eg_encryption_mediator_encrypt_ballot", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    } catch (const exception &e) {
        Log::error(":eg_encryption_mediator_encrypt_ballot", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_encryption_mediator_compact_encrypt_ballot(
  eg_encryption_mediator_t *handle, eg_plaintext_ballot_t *in_plaintext,
  eg_compact_ciphertext_ballot_t **out_ciphertext_handle)
{
    try {
        auto *plaintext = AS_TYPE(PlaintextBallot, in_plaintext);
        auto ciphertext = AS_TYPE(EncryptionMediator, handle)->compactEncrypt(*plaintext, false);

        *out_ciphertext_handle = AS_TYPE(eg_compact_ciphertext_ballot_t, ciphertext.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const invalid_argument &e) {
        Log::error(":eg_encryption_mediator_compact_encrypt_ballot", e);
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    } catch (const runtime_error &e) {
        Log::error(":eg_encryption_mediator_compact_encrypt_ballot", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    } catch (const exception &e) {
        Log::error(":eg_encryption_mediator_compact_encrypt_ballot", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_encryption_mediator_compact_encrypt_ballot_verify_proofs(
  eg_encryption_mediator_t *handle, eg_plaintext_ballot_t *in_plaintext,
  eg_compact_ciphertext_ballot_t **out_ciphertext_handle)
{
    try {
        auto *plaintext = AS_TYPE(PlaintextBallot, in_plaintext);
        auto ciphertext = AS_TYPE(EncryptionMediator, handle)->compactEncrypt(*plaintext, true);

        *out_ciphertext_handle = AS_TYPE(eg_compact_ciphertext_ballot_t, ciphertext.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const invalid_argument &e) {
        Log::error(":eg_encryption_mediator_compact_encrypt_ballot_verify_proofs", e);
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    } catch (const runtime_error &e) {
        Log::error(":eg_encryption_mediator_compact_encrypt_ballot_verify_proofs", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    } catch (const exception &e) {
        Log::error(":eg_encryption_mediator_compact_encrypt_ballot_verify_proofs", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_encryption_mediator_encrypt_ballot_verify_proofs(
  eg_encryption_mediator_t *handle, eg_plaintext_ballot_t *in_plaintext,
  bool in_use_precomputed_values, eg_ciphertext_ballot_t **out_ciphertext_handle)
{
    try {
        auto *plaintext = AS_TYPE(PlaintextBallot, in_plaintext);
        auto ciphertext =
          AS_TYPE(EncryptionMediator, handle)->encrypt(*plaintext, true, in_use_precomputed_values);

        *out_ciphertext_handle = AS_TYPE(eg_ciphertext_ballot_t, ciphertext.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const invalid_argument &e) {
        Log::error(":eg_encryption_mediator_encrypt_ballot_verify_proofs", e);
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    } catch (const runtime_error &e) {
        Log::error(":eg_encryption_mediator_encrypt_ballot_verify_proofs", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    } catch (const exception &e) {
        Log::error(":eg_encryption_mediator_encrypt_ballot_verify_proofs", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

#pragma endregion

#pragma region EncryptSelection

eg_electionguard_status_t eg_encrypt_selection(eg_plaintext_ballot_selection_t *in_plaintext,
                                               eg_selection_description_t *in_description,
                                               eg_ciphertext_election_context_t *in_context,
                                               eg_element_mod_q_t *in_nonce_seed,
                                               bool in_is_placeholder, bool in_should_verify_proofs,
                                               bool in_use_precomputed_values,
                                               eg_ciphertext_ballot_selection_t **out_handle)
{
    try {
        auto *plaintext = AS_TYPE(PlaintextBallotSelection, in_plaintext);
        auto *description = AS_TYPE(SelectionDescription, in_description);
        auto *context = AS_TYPE(CiphertextElectionContext, in_context);
        auto *nonce_seed_ = AS_TYPE(ElementModQ, in_nonce_seed);

        auto ciphertext =
          encryptSelection(*plaintext, *description, *context, *nonce_seed_, in_is_placeholder,
                           in_should_verify_proofs, in_use_precomputed_values);

        *out_handle = AS_TYPE(eg_ciphertext_ballot_selection_t, ciphertext.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const invalid_argument &e) {
        Log::error(":eg_encrypt_selection", e);
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    } catch (const runtime_error &e) {
        Log::error(":eg_encrypt_selection", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    } catch (const exception &e) {
        Log::error(":eg_encrypt_selection", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

#pragma endregion

#pragma region EncryptContest

eg_electionguard_status_t
eg_encrypt_contest(eg_plaintext_ballot_contest_t *in_plaintext, eg_internal_manifest_t *in_manifest,
                   eg_contest_description_with_placeholders_t *in_description,
                   eg_ciphertext_election_context_t *in_context, eg_element_mod_q_t *in_nonce_seed,
                   bool in_should_verify_proofs, bool in_use_precomputed_values,
                   eg_ciphertext_ballot_contest_t **out_handle)
{
    try {
        auto *plaintext = AS_TYPE(PlaintextBallotContest, in_plaintext);
        auto *internalManifest = AS_TYPE(InternalManifest, in_manifest);
        auto *description = AS_TYPE(ContestDescriptionWithPlaceholders, in_description);
        auto *context = AS_TYPE(CiphertextElectionContext, in_context);
        auto *nonce_seed_ = AS_TYPE(ElementModQ, in_nonce_seed);

        auto ciphertext =
          encryptContest(*plaintext, *internalManifest, *description, *context, *nonce_seed_,
                         in_should_verify_proofs, in_use_precomputed_values);

        *out_handle = AS_TYPE(eg_ciphertext_ballot_contest_t, ciphertext.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const invalid_argument &e) {
        Log::error(":eg_encrypt_contest", e);
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    } catch (const runtime_error &e) {
        Log::error(":eg_encrypt_contest", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    } catch (const exception &e) {
        Log::error(":eg_encrypt_contest", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

#pragma endregion

#pragma region EncryptBallot

eg_electionguard_status_t
eg_encrypt_ballot(eg_plaintext_ballot_t *in_plaintext, eg_internal_manifest_t *in_manifest,
                  eg_ciphertext_election_context_t *in_context,
                  eg_element_mod_q_t *in_ballot_code_seed, bool in_should_verify_proofs,
                  bool in_use_precomputed_values, eg_ciphertext_ballot_t **out_handle)
{
    try {
        auto *plaintext = AS_TYPE(PlaintextBallot, in_plaintext);
        auto *manifest = AS_TYPE(InternalManifest, in_manifest);
        auto *context = AS_TYPE(CiphertextElectionContext, in_context);
        auto *code_seed = AS_TYPE(ElementModQ, in_ballot_code_seed);

        auto ciphertext = encryptBallot(*plaintext, *manifest, *context, *code_seed, nullptr, 0ULL,
                                        in_should_verify_proofs, in_use_precomputed_values);
        *out_handle = AS_TYPE(eg_ciphertext_ballot_t, ciphertext.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const invalid_argument &e) {
        Log::error(":eg_encrypt_ballot", e);
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    } catch (const runtime_error &e) {
        Log::error(":eg_encrypt_ballot", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    } catch (const exception &e) {
        Log::error(":eg_encrypt_ballot", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_encrypt_ballot_with_nonce(
  eg_plaintext_ballot_t *in_plaintext, eg_internal_manifest_t *in_manifest,
  eg_ciphertext_election_context_t *in_context, eg_element_mod_q_t *in_ballot_code_seed,
  eg_element_mod_q_t *in_nonce, uint64_t timestamp, bool in_should_verify_proofs,
  eg_ciphertext_ballot_t **out_handle)
{
    try {
        auto *plaintext = AS_TYPE(PlaintextBallot, in_plaintext);
        auto *manifest = AS_TYPE(InternalManifest, in_manifest);
        auto *context = AS_TYPE(CiphertextElectionContext, in_context);
        auto *code_seed = AS_TYPE(ElementModQ, in_ballot_code_seed);
        auto *nonce = AS_TYPE(ElementModQ, in_nonce);
        auto nonce_ptr = nonce->clone();

        auto no_precompute_with_nonce_explicit_false = false;
        auto ciphertext =
          encryptBallot(*plaintext, *manifest, *context, *code_seed, move(nonce_ptr), timestamp,
                        in_should_verify_proofs, no_precompute_with_nonce_explicit_false);
        *out_handle = AS_TYPE(eg_ciphertext_ballot_t, ciphertext.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const invalid_argument &e) {
        Log::error(":eg_encrypt_ballot_with_nonce", e);
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    } catch (const runtime_error &e) {
        Log::error(":eg_encrypt_ballot_with_nonce", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    } catch (const exception &e) {
        Log::error(":eg_encrypt_ballot_with_nonce", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

#pragma endregion

#pragma region EncryptCompactBallot

eg_electionguard_status_t eg_encrypt_compact_ballot(eg_plaintext_ballot_t *in_plaintext,
                                                    eg_internal_manifest_t *in_manifest,
                                                    eg_ciphertext_election_context_t *in_context,
                                                    eg_element_mod_q_t *in_ballot_code_seed,
                                                    bool in_should_verify_proofs,
                                                    eg_compact_ciphertext_ballot_t **out_handle)
{
    try {
        auto *plaintext = AS_TYPE(PlaintextBallot, in_plaintext);
        auto *manifest = AS_TYPE(InternalManifest, in_manifest);
        auto *context = AS_TYPE(CiphertextElectionContext, in_context);
        auto *code_seed = AS_TYPE(ElementModQ, in_ballot_code_seed);

        auto ciphertext = encryptCompactBallot(*plaintext, *manifest, *context, *code_seed, nullptr,
                                               0, in_should_verify_proofs);
        *out_handle = AS_TYPE(eg_compact_ciphertext_ballot_t, ciphertext.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const invalid_argument &e) {
        Log::error(":eg_encrypt_compact_ballot", e);
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    } catch (const runtime_error &e) {
        Log::error(":eg_encrypt_compact_ballot", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    } catch (const exception &e) {
        Log::error(":eg_encrypt_compact_ballot", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_encrypt_compact_ballot_with_nonce(
  eg_plaintext_ballot_t *in_plaintext, eg_internal_manifest_t *in_manifest,
  eg_ciphertext_election_context_t *in_context, eg_element_mod_q_t *in_ballot_code_seed,
  eg_element_mod_q_t *in_nonce, uint64_t timestamp, bool in_should_verify_proofs,
  eg_compact_ciphertext_ballot_t **out_handle)
{
    try {
        auto *plaintext = AS_TYPE(PlaintextBallot, in_plaintext);
        auto *manifest = AS_TYPE(InternalManifest, in_manifest);
        auto *context = AS_TYPE(CiphertextElectionContext, in_context);
        auto *code_seed = AS_TYPE(ElementModQ, in_ballot_code_seed);
        auto *nonce = AS_TYPE(ElementModQ, in_nonce);
        auto nonce_ptr = nonce->clone();

        auto ciphertext = encryptCompactBallot(*plaintext, *manifest, *context, *code_seed,
                                               move(nonce_ptr), timestamp, in_should_verify_proofs);
        *out_handle = AS_TYPE(eg_compact_ciphertext_ballot_t, ciphertext.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const invalid_argument &e) {
        Log::error(":eg_encrypt_compact_ballot_with_nonce", e);
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    } catch (const runtime_error &e) {
        Log::error(":eg_encrypt_compact_ballot_with_nonce", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    } catch (const exception &e) {
        Log::error(":eg_encrypt_compact_ballot_with_nonce", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

#pragma endregion
