#include "electionguard/manifest.hpp"

#include "../log.hpp"

#include <emscripten/bind.h>
#include <iostream>

using namespace emscripten;
using namespace electionguard;
using namespace std;

EMSCRIPTEN_BINDINGS(electionguard)
{
    class_<Manifest>("Manifest")
      .function("toJson", &Manifest::toJson)
      .class_function("fromJson", &Manifest::fromJson);
    ;

    class_<InternalManifest>("InternalManifest")
      .constructor<const Manifest &>()
      .function("toJson", &InternalManifest::toJson)
      .class_function("fromJson", &InternalManifest::fromJson);
    ;
}