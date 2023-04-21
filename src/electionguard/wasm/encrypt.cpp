#include "electionguard/encrypt.hpp"

#include "../log.hpp"

#include <emscripten/bind.h>
#include <iostream>

using namespace emscripten;
using namespace electionguard;
using namespace std;

class EncryptFunctions
{
  public:
    static std::unique_ptr<CiphertextBallot>
    encryptBallot(const PlaintextBallot &ballot, const InternalManifest &internalManifest,
                  const CiphertextElectionContext &context, const ElementModQ &ballotCodeSeed,
                  bool shouldVerifyProofs = true, bool usePrecomputedValues = false)
    {
        auto result =
          electionguard::encryptBallot(ballot, internalManifest, context, ballotCodeSeed, nullptr,
                                       0ULL, shouldVerifyProofs, usePrecomputedValues);
        return std::move(result);
    }
    static std::unique_ptr<CiphertextBallot>
    encryptBallotWithNonce(const PlaintextBallot &ballot, const InternalManifest &internalManifest,
                           const CiphertextElectionContext &context,
                           const ElementModQ &ballotCodeSeed, std::unique_ptr<ElementModQ> nonce,
                           uint64_t timestamp, bool shouldVerifyProofs = true)
    {
        auto noPrecomputeWithNonceExplicitFalse = false;
        auto result = electionguard::encryptBallot(
          ballot, internalManifest, context, ballotCodeSeed, std::move(nonce), timestamp,
          shouldVerifyProofs, noPrecomputeWithNonceExplicitFalse);
        return std::move(result);
    }
};

EMSCRIPTEN_BINDINGS(electionguard)
{
    class_<EncryptionDevice>("EncryptionDevice")
      .constructor<const uint64_t, const uint64_t, const uint64_t, const string>()
      .function("getTimestamp", &EncryptionDevice::getTimestamp)
      .function("getDeviceUuid", &EncryptionDevice::getDeviceUuid)
      .function("getSessionUuid", &EncryptionDevice::getSessionUuid)
      .function("getLaunchCode", &EncryptionDevice::getLaunchCode)
      .function("getLocation", &EncryptionDevice::getLocation)
      .function("toJson", &EncryptionDevice::toJson)
      .class_function("fromJson", &EncryptionDevice::fromJson);

    class_<EncryptionMediator>("EncryptionMediator")
      .constructor<const InternalManifest &, const CiphertextElectionContext &,
                   const EncryptionDevice &>()
      .function("encrypt", &EncryptionMediator::encrypt)
      .function("compactEncrypt", &EncryptionMediator::compactEncrypt);

    class_<EncryptFunctions>("EncryptFunctions")
      .class_function("encryptBallot", &EncryptFunctions::encryptBallot)
      .class_function("encryptBallotWithNonce", &EncryptFunctions::encryptBallotWithNonce);
}