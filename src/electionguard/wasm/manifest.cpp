#include "electionguard/manifest.hpp"

#include "../log.hpp"

#include <emscripten/bind.h>
#include <iostream>

using namespace emscripten;
using namespace electionguard;
using namespace std;

class ManifestFunctions
{
  public:
    static std::unique_ptr<InternalManifest>
    InternalManifestFromElectionManifestJson(const std::string &json)
    {
        auto manifest = Manifest::fromJson(json);
        return InternalManifestFromElectionManifest(*manifest);
    }

    static std::unique_ptr<InternalManifest>
    InternalManifestFromElectionManifest(const Manifest &manifest)
    {
        auto result = make_unique<InternalManifest>(manifest);
        return std::move(result);
    }
};

EMSCRIPTEN_BINDINGS(electionguard)
{
    class_<Manifest>("Manifest")
      .function("toJson", &Manifest::toJson)
      .class_function("fromJson", &Manifest::fromJson);

    class_<InternalManifest>("InternalManifest")
      .constructor<const Manifest &>()
      .function("toJson", &InternalManifest::toJson)
      .class_function("fromJson", &InternalManifest::fromJson);

    class_<ManifestFunctions>("ManifestFunctions")
      .class_function("InternalManifestFromElectionManifestJson",
                      &ManifestFunctions::InternalManifestFromElectionManifestJson)
      .class_function("InternalManifestFromElectionManifest",
                      &ManifestFunctions::InternalManifestFromElectionManifest);
}