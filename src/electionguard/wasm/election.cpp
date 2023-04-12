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
      .function("toJson", &CiphertextElectionContext::toJson)
      .class_function("fromJson", &CiphertextElectionContext::fromJson);
}