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

TEST_CASE("Can Construct CiphertextElectionContext")
{
    // Arrange
    auto keypair = ElGamalKeyPair::fromSecret(TWO_MOD_Q());
    auto manifest = ManifestGenerator::getJeffersonCountyManifest_Minimal();
    auto internal = make_unique<InternalManifest>(*manifest);
    auto context = make_unique<CiphertextElectionContext>(
      1UL, 1UL, keypair->getPublicKey()->clone(), TWO_MOD_Q().clone(),
      internal->getManifestHash()->clone(), TWO_MOD_Q().clone(), TWO_MOD_Q().clone());
    auto json = context->toJson();
    auto bson = context->toBson();

    Log::debug(json);

    // Act
    auto fromJson = CiphertextElectionContext::fromJson(json);
    auto fromBson = CiphertextElectionContext::fromBson(bson);

    // Assert
    CHECK(fromJson->getManifestHash()->toHex() == context->getManifestHash()->toHex());
    CHECK(fromBson->getManifestHash()->toHex() == context->getManifestHash()->toHex());
    CHECK(fromJson->getParameterHash()->toHex() == PARAMETER_BASE_HASH().toHex());
}

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
    CHECK(fromJson->getManifestHash()->toHex() == context->getManifestHash()->toHex());
    CHECK(fromBson->getManifestHash()->toHex() == context->getManifestHash()->toHex());
}

TEST_CASE("Can serialize CiphertextElectionContext without ParameterHash")
{
    // Arrange
    auto json =
      "{\"commitment_hash\":\"02\",\"crypto_base_hash\":\"02\",\"crypto_extended_base_hash\":"
      "\"02\",\"elgamal_public_key\":"
      "\"A1DC7898431B0FD330161794512DF02CACE0A3848B0AE54DF215AE9A703F592A11ADFF4DC7E80157A8A33A"
      "15BB4319A94D76FD23419AA1DF03A85A018381FB5455AE70D296C76E11CAB43AB0289CB8FCC4394D87E25BB7"
      "0A88C47A385C1D6D870076A539D8F1CA411FE187195465278D925E0D46214F7A4A0CED063124503CEDD6B4D9"
      "61ADD48789951E2B350F4A760F1D8568C74E201753BA4835B19B73221EC226FFB81BC4796C2E64F47D3AAE4A"
      "4E31316E5596855A9C3868E00DCBF1E1F43FB10B94C36B1CFDA9FBA3D59424089B287C5BA8FEAE8AFA5191C0"
      "7F864C386AFA5C4CB8E0D6C95DB7819C7AC4060759AF96E7151054AAE8D4B6DA4BA5E919638F4CE574D5DDE1"
      "E3BF6AF1A796393E5851006E59F136A9BF73EA50BB40998E7A23D7C5A0236281095C044A4E8C65310CA6D644"
      "489F013AD0ACADCF354A9FC78B14E5FDE250D8F1576ECF2F7861FAF32101B2BE28590E59E7C3E8B2F9DCB4D0"
      "5F4483229CF5836879E45E76C56FB190BFB1877B9ED364BCDC8C3A2120356127540F8358ECC075FFC2FFD17E"
      "4B9383AF479DC834E4F5C453645180E9E633F218F6EE4D603E9F87FD63C2E86E0791B2F59ECEA042845A8C7B"
      "481E46DFF2E74D090DE9D61B895858FB9B0D167F9139620BD5FE6231879018560FC3BA7610CA32BB30D6B8C0"
      "D58666856761E727D2C9E42ED44C9727C7C5EAECA88EBE219D0A57BAC6\",\"manifest_hash\":"
      "\"E3045AFD00109A538BF9518B8F203042E8AA27810FEB68FD78A24DBDA168BF2E\",\"number_of_"
      "guardians\":1,\"quorum\":1}";

    Log::debug(json);

    // Act
    auto fromJson = CiphertextElectionContext::fromJson(json);

    // Assert
    CHECK(fromJson->getParameterHash()->toHex() == PARAMETER_BASE_HASH().toHex());
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
