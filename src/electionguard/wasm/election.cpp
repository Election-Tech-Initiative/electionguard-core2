#include "electionguard/election.hpp"

#include "../log.hpp"

#include <emscripten/bind.h>
#include <iostream>

using namespace emscripten;
using namespace electionguard;
using namespace std;

EMSCRIPTEN_BINDINGS(electionguard)
{
    class_<ContextConfiguration>("ContextConfiguration")
      .constructor()
      .constructor<const bool, const uint64_t>()
      .function("getMaxNumberOfBallots", &ContextConfiguration::getMaxNumberOfBallots)
      .function("getAllowOverVotes", &ContextConfiguration::getAllowOverVotes)
      .class_function("make", &ContextConfiguration::make);

    class_<CiphertextElectionContext>("CiphertextElectionContext")
      .function("getNumberOfGuardians", &CiphertextElectionContext::getNumberOfGuardians)
      .function("getQuorum", &CiphertextElectionContext::getQuorum)
      .function("getElGamalPublicKey", &CiphertextElectionContext::getElGamalPublicKey,
                allow_raw_pointers())
      .function("getElGamalPublicKeyRef", &CiphertextElectionContext::getElGamalPublicKeyRef)
      .function("getParameterHash", &CiphertextElectionContext::getParameterHash,
                allow_raw_pointers())
      .function("getCommitmentHash", &CiphertextElectionContext::getCommitmentHash,
                allow_raw_pointers())
      .function("getManifestHash", &CiphertextElectionContext::getManifestHash,
                allow_raw_pointers())
      .function("getCryptoBaseHash", &CiphertextElectionContext::getCryptoBaseHash,
                allow_raw_pointers())
      .function("getCryptoExtendedBaseHash", &CiphertextElectionContext::getCryptoExtendedBaseHash,
                allow_raw_pointers())
      .function("toJson", &CiphertextElectionContext::toJson)
      .class_function("fromJson", &CiphertextElectionContext::fromJson);
}