#include "../../src/electionguard/convert.hpp"
#include "../../src/electionguard/log.hpp"
#include "generators/election.hpp"
#include "generators/manifest.hpp"
#include "utils/byte_logger.hpp"

#include <doctest/doctest.h>
#include <electionguard/election.hpp>
#include <electionguard/elgamal.hpp>
#include <electionguard/manifest.hpp>
#include <unordered_map>

using namespace electionguard;
using namespace electionguard::tools::generators;
using namespace std;

TEST_CASE("Can serialize CiphertextElectionContext")
{
    // Arrange
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q());
    auto manifest = ManifestGenerator::getJeffersonCountyManifest_Minimal();
    auto internal = make_unique<InternalManifest>(*manifest);
    auto context = ElectionGenerator::getFakeContext(*internal, *keypair->getPublicKey());
    auto json = context->toJson();
    auto bson = context->toBson();

    Log::debug(json);

    // Act
    auto fromJson = CiphertextElectionContext::fromJson(json);
    auto fromBson = CiphertextElectionContext::fromBson(bson);

    // Assert
    // validate against manifest->getManifestHash()
    CHECK(fromJson->getManifestHash()->toHex() == context->getManifestHash()->toHex());
    CHECK(fromBson->getManifestHash()->toHex() == context->getManifestHash()->toHex());
}

TEST_CASE("Version Code in CiphertextElectionContext Matches Constant")
{
    // Arrange
    auto versionCode = string_to_fixed_width_bytes<32>("v2.0.0");

    // Act
    auto versionElement = bytes_to_q(versionCode, true);

    // Assert
    CHECK(versionElement->toHex() ==
          "76322E302E300000000000000000000000000000000000000000000000000000");
}

TEST_CASE("Parameter Hash in CiphertextElectionContext Matches Constant")
{
    // Arrange

    // Act
    auto context = CiphertextElectionContext::make(3UL, 2UL, TWO_MOD_P().clone(),
                                                   TWO_MOD_Q().clone(), TWO_MOD_Q().clone());

    // Assert
    CHECK(context->getParameterHash()->toHex() == PARAMETER_BASE_HASH().toHex());

    // TODO: update the constant check when hasing is updated to E.G. 2.0 spec.
    // CHECK(context->getParameterHash()->toHex() ==
    //       "0x2B3B025E50E09C119CBA7E9448ACD1CABC9447EF39BF06327D81C665CDD86296");
}

TEST_CASE("Assign ExtraData to CiphertextElectionContext")
{
    // Arrange
    auto key = "ballot_base_uri";
    auto value = "http://something.vote/";
    unordered_map<string, string> extendedData({{key, value}});

    // Act
    auto context = CiphertextElectionContext::make(
      3UL, 2UL, TWO_MOD_P().clone(), TWO_MOD_Q().clone(), TWO_MOD_Q().clone(), extendedData);

    auto cached = context->getExtendedData();
    auto resolved = cached.find(key);

    // Assert
    if (resolved == cached.end()) {
        FAIL(resolved);
    } else {
        CHECK(resolved->second == value);
    }
}

TEST_CASE("Assign ExtraData to CiphertextElectionContextand Serialize")
{
    // Arrange
    auto key = "uri";
    auto value = "http://something.vote/";
    unordered_map<string, string> extendedData({{key, value}});

    // Act
    auto context = CiphertextElectionContext::make(
      3UL, 2UL, TWO_MOD_P().clone(), TWO_MOD_Q().clone(), TWO_MOD_Q().clone(), extendedData);

    auto json = context->toJson();
    auto bson = context->toBson();

    Log::debug(json);

    // Act
    auto fromJson = CiphertextElectionContext::fromJson(json);
    auto fromBson = CiphertextElectionContext::fromBson(bson);

    // Assert
    CHECK(fromJson->getExtendedData().at("uri") == context->getExtendedData().at("uri"));
    CHECK(fromBson->getExtendedData().at("uri") == context->getExtendedData().at("uri"));
}
