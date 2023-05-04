#include "electionguard/ballot.hpp"

#include "../log.hpp"

#include <emscripten/bind.h>
#include <iostream>

using namespace emscripten;
using namespace electionguard;
using namespace std;

EMSCRIPTEN_BINDINGS(electionguard)
{
    class_<PlaintextBallot>("PlaintextBallot")
      .function("getObjectId", &PlaintextBallot::getObjectId)
      .function("getStyleId", &PlaintextBallot::getStyleId)
      .function("toJson", &PlaintextBallot::toJson)
      .class_function("fromJson", &PlaintextBallot::fromJson);

    class_<CiphertextBallot>("CiphertextBallot")
      .function("getObjectId", &CiphertextBallot::getObjectId)
      .function("getStyleId", &CiphertextBallot::getStyleId)
      .function("getManifestHash", &CiphertextBallot::getManifestHash, allow_raw_pointers())
      .function("getBallotCodeSeed", &CiphertextBallot::getBallotCodeSeed, allow_raw_pointers())
      .function("getBallotCode", &CiphertextBallot::getBallotCode, allow_raw_pointers())
      .function("getTimestamp", &CiphertextBallot::getTimestamp, allow_raw_pointers())
      .function("getNonce", &CiphertextBallot::getNonce, allow_raw_pointers())
      .function("isValidEncryption", &CiphertextBallot::isValidEncryption)
      .function("cast", &CiphertextBallot::cast)
      .function("challenge", &CiphertextBallot::challenge)
      .function("spoil", &CiphertextBallot::spoil)
      .function("toJson", &CiphertextBallot::toJson)
      .class_function("fromJson", &CiphertextBallot::fromJson);
}