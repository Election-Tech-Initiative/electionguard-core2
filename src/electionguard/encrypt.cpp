
#include "electionguard/encrypt.hpp"

#include "electionguard/async.hpp"
#include "electionguard/ballot_code.hpp"
#include "electionguard/elgamal.hpp"
#include "electionguard/hash.hpp"
#include "electionguard/nonces.hpp"
#include "electionguard/precompute_buffers.hpp"
#include "facades/bignum4096.hpp"
#include "log.hpp"
#include "serialize.hpp"
#include "utils.hpp"

#include <future>
#include <iostream>
#include <nlohmann/json.hpp>
#include <optional>

using std::invalid_argument;
using std::make_unique;
using std::move;
using std::optional;
using std::runtime_error;
using std::to_string;
using std::unique_ptr;
using std::vector;

using electionguard::getSystemTimestamp;
using DeviceSerializer = electionguard::Serialize::EncryptionDevice;
using nlohmann::json;

namespace electionguard
{
#pragma region EncryptionDevice

    struct EncryptionDevice::Impl {
        uint64_t deviceUuid;
        uint64_t sessionUuid;
        uint64_t launchCode;
        string location;

        Impl(const uint64_t deviceUuid, const uint64_t sessionUuid, const uint64_t launchCode,
             const string &location)
        {
            this->deviceUuid = deviceUuid;
            this->sessionUuid = sessionUuid;
            this->launchCode = launchCode;
            this->location = location;
        }
    };

    EncryptionDevice::EncryptionDevice(const uint64_t deviceUuid, const uint64_t sessionUuid,
                                       const uint64_t launchCode, const string &location)
        : pimpl(new Impl(deviceUuid, sessionUuid, launchCode, location))
    {

        Log::trace("EncryptionDevice: Created: UUID: " + to_string(deviceUuid) +
                   " at: " + location);
    }
    EncryptionDevice::~EncryptionDevice() = default;

    EncryptionDevice &EncryptionDevice::operator=(EncryptionDevice other)
    {
        swap(pimpl, other.pimpl);
        return *this;
    }

    unique_ptr<ElementModQ> EncryptionDevice::getHash() const
    {
        return BallotCode::getHashForDevice(pimpl->deviceUuid, pimpl->sessionUuid,
                                            pimpl->launchCode, pimpl->location);
    }

    uint64_t EncryptionDevice::getTimestamp() const { return getSystemTimestamp(); }

    //allowing for serialization
    vector<uint8_t> EncryptionDevice::toBson() const { return DeviceSerializer::toBson(*this); }

    string EncryptionDevice::toJson() const { return DeviceSerializer::toJson(*this); }

    unique_ptr<EncryptionDevice> EncryptionDevice::fromJson(string data)
    {
        return DeviceSerializer::fromJson(move(data));
    }

    unique_ptr<EncryptionDevice> EncryptionDevice::fromBson(vector<uint8_t> data)
    {
        return DeviceSerializer::fromBson(move(data));
    }

    uint64_t EncryptionDevice::getDeviceUuid() const { return pimpl->deviceUuid; }
    uint64_t EncryptionDevice::getSessionUuid() const { return pimpl->sessionUuid; }
    uint64_t EncryptionDevice::getLaunchCode() const { return pimpl->launchCode; }
    std::string EncryptionDevice::getLocation() const { return pimpl->location; }

#pragma endregion

#pragma region EncryptionMediator

    struct EncryptionMediator::Impl {
        const InternalManifest &internalManifest;
        const CiphertextElectionContext &context;
        const EncryptionDevice &encryptionDevice;
        unique_ptr<ElementModQ> ballotCodeSeed;

        Impl(const InternalManifest &internalManifest, const CiphertextElectionContext &context,
             const EncryptionDevice &encryptionDevice)
            : internalManifest(internalManifest), context(context),
              encryptionDevice(encryptionDevice)

        {
        }
    };

    EncryptionMediator::EncryptionMediator(const InternalManifest &internalManifest,
                                           const CiphertextElectionContext &context,
                                           const EncryptionDevice &encryptionDevice)
        : pimpl(new Impl(internalManifest, context, encryptionDevice))
    {
        if (internalManifest.getManifestHash()->toHex() != context.getManifestHash()->toHex()) {
            throw invalid_argument("manifest and context do not match hashes manifest:" +
                                   internalManifest.getManifestHash()->toHex() +
                                   " context:" + context.getManifestHash()->toHex());
        }
    }

    EncryptionMediator::~EncryptionMediator() = default;

    EncryptionMediator &EncryptionMediator::operator=(EncryptionMediator other)
    {
        swap(pimpl, other.pimpl);
        return *this;
    }

    unique_ptr<CiphertextBallot>
    EncryptionMediator::encrypt(const PlaintextBallot &ballot, bool verifyProofs /* = true */,
                                bool usePrecomputedValues /* = false */) const
    {
        Log::trace("encrypt: objectId: " + ballot.getObjectId());

        // this implementation chains each ballot encrypted by the mediator
        // to every subsequent ballot creating a linked list data structure
        // that can be used to prove there are no gaps in the election record
        // but this is not required as part of the specification
        if (!pimpl->ballotCodeSeed) {
            auto deviceHash = pimpl->encryptionDevice.getHash();
            pimpl->ballotCodeSeed.swap(deviceHash);
            Log::trace("encrypt: instantiated ballotCodeSeed:", pimpl->ballotCodeSeed->toHex());
        }

        auto encryptedBallot = encryptBallot(
          ballot, pimpl->internalManifest, pimpl->context, *pimpl->ballotCodeSeed, nullptr,
          pimpl->encryptionDevice.getTimestamp(), verifyProofs, usePrecomputedValues);

        Log::trace("encrypt: ballot encrypted");
        pimpl->ballotCodeSeed = make_unique<ElementModQ>(*encryptedBallot->getBallotCode());
        return encryptedBallot;
    }

    unique_ptr<CompactCiphertextBallot>
    EncryptionMediator::compactEncrypt(const PlaintextBallot &ballot,
                                       bool verifyProofs /* = true */) const
    {
        Log::trace("encrypt: objectId:" + ballot.getObjectId());

        // this implementation chains each ballot encrypted by the mediator
        // to every subsequent ballot creating a linked list data structure
        // that can be used to prove there are no gaps in the election record
        // but this is not required as part of the specification.
        if (!pimpl->ballotCodeSeed) {
            auto deviceHash = pimpl->encryptionDevice.getHash();
            pimpl->ballotCodeSeed.swap(deviceHash);
            Log::trace("encrypt: instantiated ballotCodeSeed:", pimpl->ballotCodeSeed->toHex());
        }

        auto encryptedBallot = encryptCompactBallot(
          ballot, pimpl->internalManifest, pimpl->context, *pimpl->ballotCodeSeed, nullptr,
          pimpl->encryptionDevice.getTimestamp(), verifyProofs);

        Log::trace("encrypt: ballot encrypted");
        pimpl->ballotCodeSeed = make_unique<ElementModQ>(*encryptedBallot->getBallotCode());
        return encryptedBallot;
    }

#pragma endregion

#pragma region Encryption Helpers

    unique_ptr<PlaintextBallotSelection> selectionFrom(const SelectionDescription &description,
                                                       bool isPlaceholder = false,
                                                       bool isAffirmative = false)
    {
        return make_unique<PlaintextBallotSelection>(description.getObjectId(),
                                                     isAffirmative ? 1UL : 0UL, isPlaceholder);
    }

    unique_ptr<PlaintextBallotContest> contestFrom(const ContestDescription &description)
    {
        vector<unique_ptr<PlaintextBallotSelection>> selections;
        for (const auto &selectionDescription : description.getSelections()) {
            selections.push_back(selectionFrom(selectionDescription));
        }

        return make_unique<PlaintextBallotContest>(description.getObjectId(), move(selections));
    }

    /// <summary>
    /// Gets overvote and write in information from the selections in a contest
    /// puts the information in a json string. The internalManifest is needed because
    /// it has the candidates and the candidate holds the indicator if a
    /// selection is a write in.
    ///
    /// <param name="contest">the contest in valid input form</param>
    /// <param name="internalManifest">the `InternalManifest` which defines this ballot's structure</param>
    /// <param name="is_overvote">indicates if an overvote was detected</param>
    /// <returns>string holding the json with the write ins</returns>
    /// </summary>
    string encodeExtendedData(const PlaintextBallotContest &contest,
                              const InternalManifest &manifest,
                              eg_contest_is_valid_result_t is_overvote)
    {
        // TODO: refactor overvote and writein so that we do not need the internal manifest
        json overvoteAndWriteIns;
        auto selections = contest.getSelections();

        // if an overvote is detected then put the selections into json
        if (is_overvote == OVERVOTE) {
            overvoteAndWriteIns["error"] = "overvote";
            json errorData;
            // run through the selections in this contest and see if any of them are writeins
            // the number of selections should be short, the number of ballot selections
            // and candidates will be longer but shouldn't be too long
            for (const auto &selection : selections) {
                if (selection.get().getVote() == 1) {
                    errorData.push_back(selection.get().getObjectId());
                }
            }
            overvoteAndWriteIns["error_data"] = errorData;
        }

        json writeins;
        auto candidates = manifest.getCandidates();
        std::vector<std::reference_wrapper<SelectionDescription>> ballotSelections;

        // find the contest in the manifest
        for (const auto &manifestContest : manifest.getContests()) {
            if (contest.getObjectId() == manifestContest.get().getObjectId()) {
                ballotSelections = manifestContest.get().getSelections();
            }
        }

        // run through the selections in this contest and see if any of them are writeins
        // the number of selections should be short, the number of ballot selections
        // and candidates will be longer but shouldn't be too long
        for (const auto &selection : selections) {
            if (selection.get().getVote() == 1) {
                for (const auto ballotSelection : ballotSelections) {
                    if (selection.get().getObjectId() == ballotSelection.get().getObjectId()) {
                        for (const auto &candidate : candidates) {
                            // check if the candidate is the correct one and if it is the writein option
                            if (ballotSelection.get().getCandidateId() ==
                                candidate.get().getObjectId()) {
                                if (candidate.get().isWriteIn()) {
                                    writeins[selection.get().getObjectId()] =
                                      selection.get().getWriteIn();
                                }
                            }
                        }
                    }
                }
            }
        }

        if (writeins.dump() != string("null")) {
            overvoteAndWriteIns["write_ins"] = writeins;
        }

        string overvoteAndWriteIns_string("");
        if (overvoteAndWriteIns.dump() != string("null")) {
            overvoteAndWriteIns_string = overvoteAndWriteIns.dump();
        }

        return overvoteAndWriteIns_string;
    }

    unique_ptr<PlaintextBallotContest> emplaceAllZeroes(const ContestDescription &description)
    {
        vector<unique_ptr<PlaintextBallotSelection>> selections;
        // loop through the selections for the contest
        for (const auto &selectionDescription : description.getSelections()) {
            selections.push_back(selectionFrom(selectionDescription));
        }

        return make_unique<PlaintextBallotContest>(description.getObjectId(), move(selections));
    }

    unique_ptr<PlaintextBallotContest> emplaceMissingValues(const PlaintextBallotContest &contest,
                                                            const ContestDescription &description)
    {
        vector<unique_ptr<PlaintextBallotSelection>> selections;
        // loop through the selections for the contest
        for (const auto &selectionDescription : description.getSelections()) {
            bool hasSelection = false;
            // loop through all the existing contests to find an existing value
            for (const auto &selection : contest.getSelections()) {
                if (selection.get().getObjectId() == selectionDescription.get().getObjectId()) {
                    hasSelection = true;
                    selections.push_back(selection.get().clone());
                    break;
                }
            }

            // no selections provided for the contest, so create a placeholder selection
            if (!hasSelection) {
                selections.push_back(selectionFrom(selectionDescription));
            }
        }

        return make_unique<PlaintextBallotContest>(description.getObjectId(), move(selections));
    }

    unique_ptr<PlaintextBallot> emplaceMissingValues(const PlaintextBallot &ballot,
                                                     const InternalManifest &manifest)
    {
        auto *style = manifest.getBallotStyle(ballot.getStyleId());

        vector<unique_ptr<PlaintextBallotContest>> contests;
        // loop through the contests for the ballot style
        for (const auto &description : manifest.getContestsFor(style->getObjectId())) {
            bool hasContest = false;
            // loop through all the existing contests to find an existing value
            for (const auto &contest : ballot.getContests()) {
                if (contest.get().getObjectId() == description.get().getObjectId()) {
                    hasContest = true;
                    contests.push_back(emplaceMissingValues(contest.get(), description.get()));
                    break;
                }
            }
            // no selections provided for the contest, so create a placeholder contest
            if (!hasContest) {
                contests.push_back(contestFrom(description));
            }
        }
        return make_unique<PlaintextBallot>(ballot.getObjectId(), ballot.getStyleId(),
                                            move(contests));
    }

#pragma region Encryption Functions

    /// <summary>
    /// Encrypts a specific selection in a contest using the provided precomputed values.
    ///
    /// This method does not validate the input data and does not check if the selection is a valid
    /// selection for the contest. It is assumed that the consumer of the method has validated the
    /// input data.
    ///
    /// Additionally, when using precomputed values, the deterministic nature of the ballot nonces
    /// is broken and the `nonce_seed` value is not used.
    /// see: https://github.com/microsoft/electionguard-cpp/issues/240
    /// <param name="objectId">the object id of the selection to encrypt</param>
    /// <param name="sequenceOrder">the sequence order of the selection to encrypt</param>
    /// <param name="vote">the vote to encrypt</param>
    /// <param name="descriptionHash">the hash of the selection description</param>
    /// <param name="cryptoExtendedBaseHash">the hash of the extended base hash</param>
    /// <param name="precomputedValues">the precomputed values</param>
    /// <param name="isPlaceholder">whether the selection is a placeholder</param>
    /// </summary>
    unique_ptr<CiphertextBallotSelection>
    encryptSelection(const std::string objectId, uint64_t sequenceOrder, uint64_t vote,
                     const ElementModQ &descriptionHash, const CiphertextElectionContext &context,
                     std::unique_ptr<PrecomputedSelection> precomputedValues, bool isPlaceholder)
    {
        // Configure the crypto input values
        Log::trace("encryptSelection: precompute for " + objectId + " hash: ",
                   descriptionHash.toHex());

        // Generate the encryption using precomputed values
        auto partialEncryption = *precomputedValues->getPartialEncryption();
        auto ciphertext = elgamalEncrypt(vote, *context.getElGamalPublicKey(), partialEncryption);
        if (ciphertext == nullptr) {
            throw runtime_error("encryptSelection:: Error generating ciphertext");
        }

        // We dont use the public key and the nonce like we do in the normal encryption
        // because the public key was used to seed the precompute table and the nonce
        // was generated when the precompute table was generated
        auto encrypted = CiphertextBallotSelection::make(
          objectId, sequenceOrder, descriptionHash, move(ciphertext), context,
          move(precomputedValues), vote, isPlaceholder, nullptr, true);

        if (encrypted == nullptr || encrypted->getProof() == nullptr) {
            throw runtime_error("encryptSelection:: Error constructing encrypted selection");
        }
        return encrypted;
    }

    /// <summary>
    /// Encrypts a specific selection in a contest in realtime.
    ///
    /// This method does not validate the input data and does not check if the selection is a valid
    /// selection for the contest. It is assumed that the consumer of the method has validated the
    /// input data.
    ///
    /// When using this method, the deterministic nature of the ballot nonces is preserved.
    /// <param name="objectId">the object id of the selection to encrypt</param>
    /// <param name="sequenceOrder">the sequence order of the selection to encrypt</param>
    /// <param name="vote">the vote to encrypt</param>
    /// <param name="descriptionHash">the hash of the selection description</param>
    /// <param name="elgamalPublicKey">the public key to use for encryption</param>
    /// <param name="cryptoExtendedBaseHash">the hash of the extended base hash</param>
    /// <param name="selectionNonce">the nonce to use for encryption</param>
    /// <param name="isPlaceholder">whether the selection is a placeholder</param>
    unique_ptr<CiphertextBallotSelection>
    encryptSelection(const std::string objectId, uint64_t sequenceOrder, uint64_t vote,
                     const ElementModQ &descriptionHash, const CiphertextElectionContext &context,
                     unique_ptr<ElementModQ> selectionNonce, bool isPlaceholder)
    {
        Log::trace("encryptSelection: for " + objectId + " hash: ", descriptionHash.toHex());

        // standard encryption in real-time
        auto ciphertext = elgamalEncrypt(vote, *selectionNonce, *context.getElGamalPublicKey());
        if (ciphertext == nullptr) {
            throw runtime_error("encryptSelection:: Error generating ciphertext");
        }

        auto encrypted = CiphertextBallotSelection::make(objectId, sequenceOrder, descriptionHash,
                                                         move(ciphertext), context, vote,
                                                         isPlaceholder, true, move(selectionNonce));

        if (encrypted == nullptr || encrypted->getProof() == nullptr) {
            throw runtime_error("encryptSelection:: Error constructing encrypted selection");
        }
        return encrypted;
    }

    unique_ptr<CiphertextBallotSelection>
    encryptSelection(const PlaintextBallotSelection &selection,
                     const SelectionDescription &description,
                     const CiphertextElectionContext &context, const ElementModQ &nonceSeed,
                     bool isPlaceholder /* = false */, bool verifyProofs /* = true */,
                     bool usePrecompute /* = true */)
    {
        // Validate Input
        if (!selection.isValid(description.getObjectId())) {
            // todo: include plaintext data in log output
            throw invalid_argument("malformed input selection " + selection.getObjectId());
        }

        unique_ptr<CiphertextBallotSelection> encrypted = nullptr;
        auto sequenceOrder = description.getSequenceOrder();

        // Configure the crypto input values
        auto descriptionHash = description.crypto_hash();

        auto precomputePublicKey = PrecomputeBufferContext::getPublicKey();

        // check if we should use precomputed values
        // TODO: issue #216 ensure that the PrecomputeBufferContext is the correct context
        // associated with the elgamalPublicKey of this election and we can remove this
        // equality check.
        if (usePrecompute && precomputePublicKey != nullptr &&
            *precomputePublicKey == *context.getElGamalPublicKey()) {
            Log::trace("encryptSelection: using precomputed values");
            auto precomputedValues = PrecomputeBufferContext::popPrecomputedSelection();
            if (precomputedValues != nullptr && precomputedValues.has_value()) {
                encrypted = encryptSelection(selection.getObjectId(), sequenceOrder,
                                             selection.getVote(), *descriptionHash, context,
                                             move(precomputedValues.value()), isPlaceholder);
            }
        }

        // if we didn't use precomputed values then we need to generate values in realtime
        if (encrypted == nullptr) {
            Log::trace("encryptSelection: generating values in realtime");
            auto selectionNonce = hash_elems({nonceSeed, description.getSequenceOrder()});

            encrypted =
              encryptSelection(selection.getObjectId(), sequenceOrder, selection.getVote(),
                               *descriptionHash, context, move(selectionNonce), isPlaceholder);
        }

        // optionally, skip the verification step
        if (!verifyProofs) {
            return encrypted;
        }

        // verify the selection.
        if (encrypted->isValidEncryption(*descriptionHash, *context.getElGamalPublicKey(),
                                         *context.getCryptoExtendedBaseHash())) {
            return encrypted;
        }
        throw runtime_error("encryptSelection failed validity check");
    }

    unique_ptr<CiphertextBallotContest>
    encryptContest(const PlaintextBallotContest &contest, const InternalManifest &internalManifest,
                   const ContestDescriptionWithPlaceholders &description,
                   const CiphertextElectionContext &context, const ElementModQ &nonceSeed,
                   bool verifyProofs /* = true */, bool usePrecompute /* = true */,
                   bool allowOvervotes /* = true */)

    {
        // Validate Input
        auto validationResult = contest.isValid(
          description.getObjectId(), description.getSelections().size(),
          description.getNumberElected(), description.getVotesAllowed(), allowOvervotes);

        if ((validationResult != SUCCESS) && (validationResult != OVERVOTE)) {
            throw invalid_argument("encryptedContest:: the plaintext contest was invalid");
        }

        // TODO: validate the description input

        // create the encryption nonces
        auto descriptionHash = description.crypto_hash();
        auto contestNonce =
          CiphertextBallotContest::contestNonce(context, description.getSequenceOrder(), nonceSeed);
        std::shared_ptr<ElementModQ> sharedNonce(move(contestNonce));

        // normalize contest selections
        unique_ptr<PlaintextBallotContest> normalizedContest;
        if (validationResult == OVERVOTE) {
            // if the is an overvote then we need to make all the selection votes 0
            normalizedContest = emplaceAllZeroes(description);
        } else {
            // iterate over the actual selections for each contest description
            // and apply the selected value if it exists.  If it does not, an explicit
            // false is entered instead and the selection_count is not incremented
            // this allows consumers to only pass in the relevant selections made by a voter
            normalizedContest = emplaceMissingValues(contest, description);
        }

        // encrypt selections
        uint64_t selectionCount = 0;
        vector<unique_ptr<CiphertextBallotSelection>> encryptedSelections;
        auto normalizedSelections = normalizedContest->getSelections();
        for (const auto &selectionDescription : description.getSelections()) {
            auto description_id = selectionDescription.get().getObjectId();
            if (auto selection =
                  std::find_if(normalizedSelections.begin(), normalizedSelections.end(),
                               [description_id](const PlaintextBallotSelection &item) {
                                   return item.getObjectId() == description_id;
                               });
                selection != normalizedSelections.end()) {

                // always false for E.G. 2.0 encryptions
                auto isPlaceholder = false;

                // track the selection count for the range proof
                auto selection_ptr = &selection->get();
                selectionCount += selection_ptr->getVote();

                // explicitly do not verify proofs when creating the encrypted selections
                // since we may verify the proofs on the entire contest
                encryptedSelections.push_back(
                  encryptSelection(*selection_ptr, selectionDescription.get(), context,
                                   *sharedNonce.get(), isPlaceholder, verifyProofs, usePrecompute));
            } else {
                // Should never happen since the contest is normalized by emplaceMissingValues
                throw runtime_error("Error constructing encrypted selection. Missing selection.");
            }
        }

        // Encrypt ExtendedData
        auto extendedData = encodeExtendedData(contest, internalManifest, validationResult);

        // Derive the extendedDataNonce from the contest nonce and a constant
        auto extendedDataNonce = hash_elems({sharedNonce->clone().get(), "contest-data"});

        vector<uint8_t> extendedData_plaintext(extendedData.begin(), extendedData.end());

        // Perform HashedElGamalCiphertext calculation
        unique_ptr<HashedElGamalCiphertext> hashedElGamal = hashedElgamalEncrypt(
          extendedData_plaintext, *extendedDataNonce, HashPrefix::get_prefix_contest_data_secret(),
          *context.getElGamalPublicKey(), *context.getCryptoExtendedBaseHash(), BYTES_512, true,
          usePrecompute);

        // Create the CiphertextBallotContest return object
        auto encryptedContest = CiphertextBallotContest::make(
          contest.getObjectId(), description.getSequenceOrder(), *descriptionHash,
          move(encryptedSelections), context, *sharedNonce->clone().get(), selectionCount,
          description.getNumberElected(), sharedNonce->clone(), nullptr, nullptr,
          move(hashedElGamal));

        if (encryptedContest == nullptr || encryptedContest->getProof() == nullptr) {
            throw runtime_error("Error constructing encrypted constest");
        }

        // optionally, skip the verification step
        if (!verifyProofs) {
            return encryptedContest;
        }

        // verify the contest.
        if (encryptedContest->isValidEncryption(*descriptionHash, *context.getElGamalPublicKey(),
                                                *context.getCryptoExtendedBaseHash())) {
            return encryptedContest;
        }

        throw runtime_error("failed validity check");
    }

    vector<unique_ptr<CiphertextBallotContest>>
    encryptContests(const PlaintextBallot &ballot, const InternalManifest &internalManifest,
                    const CiphertextElectionContext &context, const ElementModQ &nonceSeed,
                    bool verifyProofs /* = true */, bool usePrecompute /* = true */,
                    bool allowOvervotes /* = true */)
    {
        auto *style = internalManifest.getBallotStyle(ballot.getStyleId());
        vector<unique_ptr<CiphertextBallotContest>> encryptedContests;
        auto normalizedBallot = emplaceMissingValues(ballot, internalManifest);

        // TODO: Issue #217: implement async multithreading

        // only iterate on contests for this specific ballot style
        for (const auto &description : internalManifest.getContestsFor(style->getObjectId())) {
            bool hasContest = false;
            for (const auto &contest : normalizedBallot->getContests()) {
                if (contest.get().getObjectId() == description.get().getObjectId()) {
                    hasContest = true;
                    auto encrypted =
                      encryptContest(contest.get(), internalManifest, description.get(), context,
                                     nonceSeed, verifyProofs, usePrecompute, allowOvervotes);

                    encryptedContests.push_back(move(encrypted));
                    break;
                }
            }

            if (!hasContest) {
                // Should never happen since the ballot is normalized by emplacing missing values
                throw runtime_error("The ballot was malformed");
            }
        }
        return encryptedContests;
    }

    unique_ptr<CiphertextBallot>
    encryptBallot(const PlaintextBallot &ballot, const InternalManifest &manifest,
                  const CiphertextElectionContext &context, const ElementModQ &ballotCodeSeed,
                  unique_ptr<ElementModQ> nonce /* = nullptr */, uint64_t timestamp /* = 0 */,
                  bool verifyProofs /* = true */, bool usePrecompute /* = false */,
                  bool allowOvervotes /* = true */)
    {
        Log::trace("encryptBallot:: encrypting");
        auto *style = manifest.getBallotStyle(ballot.getStyleId());

        // Validate Input
        if (style == nullptr) {
            throw invalid_argument("could not find a ballot style: " + ballot.getStyleId());
        }

        // when supplying a nonce we cannot use precompute
        // because the precomputed nonces are not deterministic
        // and so we can't regenerate them from the main nonce
        if (nonce != nullptr && usePrecompute) {
            throw invalid_argument("cannot use precomputed values with a provided nonce");
        }

        // Generate a random seed nonce to use for the contest and selection nonce's on the ballot
        if (nonce == nullptr) {
            nonce = rand_q();
        }

        // Include a representation of the election and the external Id in the nonce's used
        // to derive other nonce values on the ballot
        auto nonceSeed =
          CiphertextBallot::nonceSeed(*manifest.getManifestHash(), ballot.getObjectId(), *nonce);

        Log::trace("manifestHash   :", manifest.getManifestHash()->toHex());
        Log::trace("encryptionSeed :", ballotCodeSeed.toHex());
        Log::trace("timestamp       :", to_string(timestamp));

        // encrypt contests
        auto encryptedContests = encryptContests(ballot, manifest, context, *nonceSeed,
                                                 verifyProofs, usePrecompute, allowOvervotes);

        // Get the system time
        if (timestamp == 0) {
            timestamp = getSystemTimestamp();
        }

        // make the Ciphertext Ballot object
        auto encryptedBallot = CiphertextBallot::make(
          ballot.getObjectId(), ballot.getStyleId(), *manifest.getManifestHash(), context,
          move(encryptedContests), move(nonce), timestamp, make_unique<ElementModQ>(ballotCodeSeed),
          nullptr);

        //Log::info("ballot      :", encryptedBallot->toJson(true));
        if (!encryptedBallot) {
            throw runtime_error("encryptedBallot:: Error constructing encrypted ballot");
        }

        if (!verifyProofs) {
            Log::trace("encryptBallot:: bypass proof verification");
            return encryptedBallot;
        }

        // verify the ballot.
        if (encryptedBallot->isValidEncryption(*manifest.getManifestHash(),
                                               *context.getElGamalPublicKey(),
                                               *context.getCryptoExtendedBaseHash())) {
            Log::trace("encryptBallot:: proof verification success");
            return encryptedBallot;
        }

        throw runtime_error("encryptBallot: failed validity check");
    }

    unique_ptr<CompactCiphertextBallot>
    encryptCompactBallot(const PlaintextBallot &ballot, const InternalManifest &manifest,
                         const CiphertextElectionContext &context,
                         const ElementModQ &ballotCodeSeed,
                         unique_ptr<ElementModQ> nonceSeed /* = nullptr */,
                         uint64_t timestamp /* = 0 */, bool verifyProofs /* = true*/)
    {
        // Compact ballots cannot use precompute because they rely
        // on the naonce's being deterministic in order to rehydrate
        auto noPrecomputeForCompactBallotsExplicitFalse = false;
        auto normalized = emplaceMissingValues(ballot, manifest);
        auto ciphertext =
          encryptBallot(*normalized, manifest, context, ballotCodeSeed, move(nonceSeed), timestamp,
                        verifyProofs, noPrecomputeForCompactBallotsExplicitFalse);
        return CompactCiphertextBallot::make(*normalized, *ciphertext);
    }

#pragma endregion

} // namespace electionguard
