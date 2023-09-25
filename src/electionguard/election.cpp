#include "electionguard/election.hpp"

#include "electionguard/hash.hpp"
#include "log.hpp"
#include "serialize.hpp"
#include "utils.hpp"

#include <cstring>
#include <iostream>
#include <utility>

using std::make_unique;
using std::map;
using std::move;
using std::out_of_range;
using std::ref;
using std::reference_wrapper;
using std::runtime_error;
using std::string;
using std::to_string;
using std::unique_ptr;
using std::unordered_map;
using std::vector;
using std::chrono::system_clock;

using ContextSerializer = electionguard::Serialize::CiphertextElectionContext;

namespace electionguard
{
    const ElementModQ &PARAMETER_BASE_HASH()
    {
        static auto versionCode = string_to_fixed_width_bytes<32>("v2.0.0");
        static auto parameterHash = hash_elems(
          {versionCode, HashPrefix::get_prefix_parameter_hash(), &const_cast<ElementModP &>(P()),
           &const_cast<ElementModQ &>(Q()), &const_cast<ElementModP &>(G())});
        static ElementModQ instance{parameterHash->ref(), true};
        return instance;
    }

#pragma region CiphertextElectionContext

    struct CiphertextElectionContext::Impl {
        uint64_t numberOfGuardians;
        uint64_t quorum;
        unique_ptr<ElementModP> elGamalPublicKey;
        unique_ptr<ElementModQ> parameterHash;
        unique_ptr<ElementModQ> commitmentHash;
        unique_ptr<ElementModQ> manifestHash;
        unique_ptr<ElementModQ> cryptoBaseHash;
        unique_ptr<ElementModQ> cryptoExtendedBaseHash;
        unordered_map<string, string> extendedData;
        unique_ptr<ContextConfiguration> configuration;

        Impl(uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
             unique_ptr<ElementModQ> parameterHash, unique_ptr<ElementModQ> commitmentHash,
             unique_ptr<ElementModQ> manifestHash, unique_ptr<ElementModQ> cryptoBaseHash,
             unique_ptr<ElementModQ> cryptoExtendedBaseHash)
            : elGamalPublicKey(move(elGamalPublicKey)), parameterHash(move(parameterHash)),
              commitmentHash(move(commitmentHash)), manifestHash(move(manifestHash)),
              cryptoBaseHash(move(cryptoBaseHash)),
              cryptoExtendedBaseHash(move(cryptoExtendedBaseHash))
        {
            this->numberOfGuardians = numberOfGuardians;
            this->quorum = quorum;
            this->extendedData = {};
            this->configuration = make_unique<ContextConfiguration>();
        }

        Impl(uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
             unique_ptr<ElementModQ> parameterHash, unique_ptr<ElementModQ> commitmentHash,
             unique_ptr<ElementModQ> manifestHash, unique_ptr<ElementModQ> cryptoBaseHash,
             unique_ptr<ElementModQ> cryptoExtendedBaseHash,
             unique_ptr<ContextConfiguration> config)
            : Impl(numberOfGuardians, quorum, move(elGamalPublicKey), move(parameterHash),
                   move(commitmentHash), move(manifestHash), move(cryptoBaseHash),
                   move(cryptoExtendedBaseHash))

        {
            this->numberOfGuardians = numberOfGuardians;
            this->quorum = quorum;
            this->extendedData = {};
            this->configuration = move(config);
        }

        Impl(uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
             unique_ptr<ElementModQ> parameterHash, unique_ptr<ElementModQ> commitmentHash,
             unique_ptr<ElementModQ> manifestHash, unique_ptr<ElementModQ> cryptoBaseHash,
             unique_ptr<ElementModQ> cryptoExtendedBaseHash,
             unordered_map<string, string> extendedData)
            : elGamalPublicKey(move(elGamalPublicKey)), parameterHash(move(parameterHash)),
              commitmentHash(move(commitmentHash)), manifestHash(move(manifestHash)),
              cryptoBaseHash(move(cryptoBaseHash)),
              cryptoExtendedBaseHash(move(cryptoExtendedBaseHash)), extendedData(move(extendedData))
        {
            this->numberOfGuardians = numberOfGuardians;
            this->quorum = quorum;
            this->configuration = make_unique<ContextConfiguration>();
        }

        Impl(uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
             unique_ptr<ElementModQ> parameterHash, unique_ptr<ElementModQ> commitmentHash,
             unique_ptr<ElementModQ> manifestHash, unique_ptr<ElementModQ> cryptoBaseHash,
             unique_ptr<ElementModQ> cryptoExtendedBaseHash,
             unique_ptr<ContextConfiguration> config, unordered_map<string, string> extendedData)
            : Impl(numberOfGuardians, quorum, move(elGamalPublicKey), move(parameterHash),
                   move(commitmentHash), move(manifestHash), move(cryptoBaseHash),
                   move(cryptoExtendedBaseHash), move(extendedData))
        {
            this->numberOfGuardians = numberOfGuardians;
            this->quorum = quorum;
            this->configuration = move(config);
        }
    };

    // Lifecycle Methods

    CiphertextElectionContext::CiphertextElectionContext(
      uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
      unique_ptr<ElementModQ> parameterHash, unique_ptr<ElementModQ> commitmentHash,
      unique_ptr<ElementModQ> manifestHash, unique_ptr<ElementModQ> cryptoBaseHash,
      unique_ptr<ElementModQ> cryptoExtendedBaseHash)
        : pimpl(new Impl(numberOfGuardians, quorum, move(elGamalPublicKey), move(parameterHash),
                         move(commitmentHash), move(manifestHash), move(cryptoBaseHash),
                         move(cryptoExtendedBaseHash)))
    {
    }
    CiphertextElectionContext::CiphertextElectionContext(
      uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
      unique_ptr<ElementModQ> commitmentHash, unique_ptr<ElementModQ> manifestHash,
      unique_ptr<ElementModQ> cryptoBaseHash, unique_ptr<ElementModQ> cryptoExtendedBaseHash)
        : pimpl(new Impl(numberOfGuardians, quorum, move(elGamalPublicKey),
                         move(PARAMETER_BASE_HASH().clone()), move(commitmentHash),
                         move(manifestHash), move(cryptoBaseHash), move(cryptoExtendedBaseHash)))
    {
    }
    CiphertextElectionContext::CiphertextElectionContext(
      uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
      unique_ptr<ElementModQ> parameterHash, unique_ptr<ElementModQ> commitmentHash,
      unique_ptr<ElementModQ> manifestHash, unique_ptr<ElementModQ> cryptoBaseHash,
      unique_ptr<ElementModQ> cryptoExtendedBaseHash, unique_ptr<ContextConfiguration> config)
        : pimpl(new Impl(numberOfGuardians, quorum, move(elGamalPublicKey), move(parameterHash),
                         move(commitmentHash), move(manifestHash), move(cryptoBaseHash),
                         move(cryptoExtendedBaseHash), move(config)))
    {
    }
    CiphertextElectionContext::CiphertextElectionContext(
      uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
      unique_ptr<ElementModQ> commitmentHash, unique_ptr<ElementModQ> manifestHash,
      unique_ptr<ElementModQ> cryptoBaseHash, unique_ptr<ElementModQ> cryptoExtendedBaseHash,
      unique_ptr<ContextConfiguration> config)
        : pimpl(new Impl(numberOfGuardians, quorum, move(elGamalPublicKey),
                         move(PARAMETER_BASE_HASH().clone()), move(commitmentHash),
                         move(manifestHash), move(cryptoBaseHash), move(cryptoExtendedBaseHash),
                         move(config)))
    {
    }
    CiphertextElectionContext::CiphertextElectionContext(
      uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
      unique_ptr<ElementModQ> parameterHash, unique_ptr<ElementModQ> commitmentHash,
      unique_ptr<ElementModQ> manifestHash, unique_ptr<ElementModQ> cryptoBaseHash,
      unique_ptr<ElementModQ> cryptoExtendedBaseHash, unordered_map<string, string> extendedData)
        : pimpl(new Impl(numberOfGuardians, quorum, move(elGamalPublicKey), move(parameterHash),
                         move(commitmentHash), move(manifestHash), move(cryptoBaseHash),
                         move(cryptoExtendedBaseHash), move(extendedData)))
    {
    }
    CiphertextElectionContext::CiphertextElectionContext(
      uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
      unique_ptr<ElementModQ> commitmentHash, unique_ptr<ElementModQ> manifestHash,
      unique_ptr<ElementModQ> cryptoBaseHash, unique_ptr<ElementModQ> cryptoExtendedBaseHash,
      unordered_map<string, string> extendedData)
        : pimpl(new Impl(numberOfGuardians, quorum, move(elGamalPublicKey),
                         move(PARAMETER_BASE_HASH().clone()), move(commitmentHash),
                         move(manifestHash), move(cryptoBaseHash), move(cryptoExtendedBaseHash),
                         move(extendedData)))
    {
    }
    CiphertextElectionContext::CiphertextElectionContext(
      uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
      unique_ptr<ElementModQ> parameterHash, unique_ptr<ElementModQ> commitmentHash,
      unique_ptr<ElementModQ> manifestHash, unique_ptr<ElementModQ> cryptoBaseHash,
      unique_ptr<ElementModQ> cryptoExtendedBaseHash, unique_ptr<ContextConfiguration> config,
      unordered_map<string, string> extendedData)
        : pimpl(new Impl(numberOfGuardians, quorum, move(elGamalPublicKey), move(parameterHash),
                         move(commitmentHash), move(manifestHash), move(cryptoBaseHash),
                         move(cryptoExtendedBaseHash), move(config), move(extendedData)))
    {
    }
    CiphertextElectionContext::CiphertextElectionContext(
      uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
      unique_ptr<ElementModQ> commitmentHash, unique_ptr<ElementModQ> manifestHash,
      unique_ptr<ElementModQ> cryptoBaseHash, unique_ptr<ElementModQ> cryptoExtendedBaseHash,
      unique_ptr<ContextConfiguration> config, unordered_map<string, string> extendedData)
        : pimpl(new Impl(numberOfGuardians, quorum, move(elGamalPublicKey),
                         move(PARAMETER_BASE_HASH().clone()), move(commitmentHash),
                         move(manifestHash), move(cryptoBaseHash), move(cryptoExtendedBaseHash),
                         move(config), move(extendedData)))
    {
    }
    CiphertextElectionContext::~CiphertextElectionContext() = default;

    // Operator Overloads

    CiphertextElectionContext &CiphertextElectionContext::operator=(CiphertextElectionContext other)
    {
        swap(pimpl, other.pimpl);
        return *this;
    }

    // Property Getters
    const ContextConfiguration *CiphertextElectionContext::getConfiguration() const
    {
        return pimpl->configuration.get();
    }

    uint64_t CiphertextElectionContext::getNumberOfGuardians() const
    {
        return pimpl->numberOfGuardians;
    }
    uint64_t CiphertextElectionContext::getQuorum() const { return pimpl->quorum; }
    const ElementModP *CiphertextElectionContext::getElGamalPublicKey() const
    {
        return pimpl->elGamalPublicKey.get();
    }
    const ElementModP &CiphertextElectionContext::getElGamalPublicKeyRef() const
    {
        return *pimpl->elGamalPublicKey.get();
    }
    const ElementModQ *CiphertextElectionContext::getParameterHash() const
    {
        return pimpl->parameterHash.get();
    }
    const ElementModQ *CiphertextElectionContext::getCommitmentHash() const
    {
        return pimpl->commitmentHash.get();
    }
    const ElementModQ *CiphertextElectionContext::getManifestHash() const
    {
        return pimpl->manifestHash.get();
    }
    const ElementModQ *CiphertextElectionContext::getCryptoBaseHash() const
    {
        return pimpl->cryptoBaseHash.get();
    }
    const ElementModQ *CiphertextElectionContext::getCryptoExtendedBaseHash() const
    {
        return pimpl->cryptoExtendedBaseHash.get();
    }

    const unordered_map<string, string> CiphertextElectionContext::getExtendedData() const
    {
        return pimpl->extendedData;
    }

    // Public Methods

    vector<uint8_t> CiphertextElectionContext::toBson() const
    {
        return ContextSerializer::toBson(*this);
    }

    string CiphertextElectionContext::toJson() const { return ContextSerializer::toJson(*this); }

    unique_ptr<CiphertextElectionContext> CiphertextElectionContext::fromJson(string data)
    {
        return ContextSerializer::fromJson(move(data));
    }

    unique_ptr<CiphertextElectionContext> CiphertextElectionContext::fromBson(vector<uint8_t> data)
    {
        return ContextSerializer::fromBson(move(data));
    }

    // Public Static Methods

    unique_ptr<CiphertextElectionContext> CiphertextElectionContext::make(
      uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
      unique_ptr<ElementModQ> commitmentHash, unique_ptr<ElementModQ> manifestHash)
    {
        // HM = H(HP;01,manifest). Manifest Hash 3.1.4
        auto manifestDigest = hash_elems(
          {PARAMETER_BASE_HASH(), HashPrefix::get_prefix_manifest_hash(), manifestHash.get()});

        // HB =(HP;02,n,k,date,info,HM). Election Base Hash 3.1.5
        auto cryptoBaseHash = hash_elems({PARAMETER_BASE_HASH(), HashPrefix::get_prefix_base_hash(),
                                          manifestDigest.get(), numberOfGuardians, quorum});

        // HE = H(HB;12,K,K1,0,K1,1,...,K1,k−1,K2,0,...,Kn,k−2,Kn,k−1). // Extended Base Hash 3.2.3
        auto cryptoExtendedBaseHash =
          hash_elems({cryptoBaseHash.get(), HashPrefix::get_prefix_extended_hash(),
                      elGamalPublicKey.get(), commitmentHash.get()});

        // ensure the elgamal public key instance is set as a fixed base
        elGamalPublicKey->setIsFixedBase(true);

        return make_unique<CiphertextElectionContext>(
          numberOfGuardians, quorum, move(elGamalPublicKey), PARAMETER_BASE_HASH().clone(),
          move(commitmentHash), move(manifestHash), move(cryptoBaseHash),
          move(cryptoExtendedBaseHash));
    }

    unique_ptr<CiphertextElectionContext> CiphertextElectionContext::make(
      uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
      unique_ptr<ElementModQ> commitmentHash, unique_ptr<ElementModQ> manifestHash,
      unique_ptr<ContextConfiguration> config)
    {
        // HM = H(HP;01,manifest). Manifest Hash 3.1.4
        auto manifestDigest = hash_elems(
          {PARAMETER_BASE_HASH(), HashPrefix::get_prefix_manifest_hash(), manifestHash.get()});

        // TODO: complete according to spec
        // HB =(HP;02,n,k,date,info,HM). Election Base Hash 3.1.5
        auto cryptoBaseHash = hash_elems({PARAMETER_BASE_HASH(), HashPrefix::get_prefix_base_hash(),
                                          numberOfGuardians, quorum, manifestDigest.get()});

        // HE = H(HB;12,K,K1,0,K1,1,...,K1,k−1,K2,0,...,Kn,k−2,Kn,k−1). // Extended Base Hash 3.2.3
        auto cryptoExtendedBaseHash =
          hash_elems({cryptoBaseHash.get(), HashPrefix::get_prefix_extended_hash(),
                      elGamalPublicKey.get(), commitmentHash.get()});

        // ensure the elgamal public key instance is set as a fixed base
        elGamalPublicKey->setIsFixedBase(true);

        return make_unique<CiphertextElectionContext>(
          numberOfGuardians, quorum, move(elGamalPublicKey), PARAMETER_BASE_HASH().clone(),
          move(commitmentHash), move(manifestHash), move(cryptoBaseHash),
          move(cryptoExtendedBaseHash), move(config));
    }

    unique_ptr<CiphertextElectionContext> CiphertextElectionContext::make(
      uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
      unique_ptr<ElementModQ> commitmentHash, unique_ptr<ElementModQ> manifestHash,
      std::unordered_map<std::string, std::string> extendedData)
    {
        // HM = H(HP;01,manifest). Manifest Hash 3.1.4
        auto manifestDigest = hash_elems(
          {PARAMETER_BASE_HASH(), HashPrefix::get_prefix_manifest_hash(), manifestHash.get()});

        // TODO: complete according to spec
        // HB =(HP;02,n,k,date,info,HM). Election Base Hash 3.1.5
        auto cryptoBaseHash = hash_elems({PARAMETER_BASE_HASH(), HashPrefix::get_prefix_base_hash(),
                                          numberOfGuardians, quorum, manifestDigest.get()});

        // HE = H(HB;12,K,K1,0,K1,1,...,K1,k−1,K2,0,...,Kn,k−2,Kn,k−1). // Extended Base Hash 3.2.3
        auto cryptoExtendedBaseHash =
          hash_elems({cryptoBaseHash.get(), HashPrefix::get_prefix_extended_hash(),
                      elGamalPublicKey.get(), commitmentHash.get()});

        // ensure the elgamal public key instance is set as a fixed base
        elGamalPublicKey->setIsFixedBase(true);

        return make_unique<CiphertextElectionContext>(
          numberOfGuardians, quorum, move(elGamalPublicKey), PARAMETER_BASE_HASH().clone(),
          move(commitmentHash), move(manifestHash), move(cryptoBaseHash),
          move(cryptoExtendedBaseHash), move(extendedData));
    }

    unique_ptr<CiphertextElectionContext> CiphertextElectionContext::make(
      uint64_t numberOfGuardians, uint64_t quorum, unique_ptr<ElementModP> elGamalPublicKey,
      unique_ptr<ElementModQ> commitmentHash, unique_ptr<ElementModQ> manifestHash,
      unique_ptr<ContextConfiguration> config,
      std::unordered_map<std::string, std::string> extendedData)
    {
        // HM = H(HP;01,manifest). Manifest Hash 3.1.4
        auto manifestDigest = hash_elems(
          {PARAMETER_BASE_HASH(), HashPrefix::get_prefix_manifest_hash(), manifestHash.get()});

        // TODO: complete according to spec
        // HB =(HP;02,n,k,date,info,HM). Election Base Hash 3.1.5
        auto cryptoBaseHash = hash_elems({PARAMETER_BASE_HASH(), HashPrefix::get_prefix_base_hash(),
                                          numberOfGuardians, quorum, manifestDigest.get()});

        // HE = H(HB;12,K,K1,0,K1,1,...,K1,k−1,K2,0,...,Kn,k−2,Kn,k−1). // Extended Base Hash 3.2.3
        auto cryptoExtendedBaseHash =
          hash_elems({cryptoBaseHash.get(), HashPrefix::get_prefix_extended_hash(),
                      elGamalPublicKey.get(), commitmentHash.get()});

        // ensure the elgamal public key instance is set as a fixed base
        elGamalPublicKey->setIsFixedBase(true);

        return make_unique<CiphertextElectionContext>(
          numberOfGuardians, quorum, move(elGamalPublicKey), PARAMETER_BASE_HASH().clone(),
          move(commitmentHash), move(manifestHash), move(cryptoBaseHash),
          move(cryptoExtendedBaseHash), move(config), move(extendedData));
    }

    unique_ptr<CiphertextElectionContext> CiphertextElectionContext::make(
      uint64_t numberOfGuardians, uint64_t quorum, const string &elGamalPublicKeyInHex,
      const string &commitmentHashInHex, const string &manifestHashInHex)
    {
        auto elGamalPublicKey = ElementModP::fromHex(elGamalPublicKeyInHex);
        auto commitmentHash = ElementModQ::fromHex(commitmentHashInHex);
        auto manifestHash = ElementModQ::fromHex(manifestHashInHex);

        // ensure the elgamal public key instance is set as a fixed base
        elGamalPublicKey->setIsFixedBase(true);

        return make(numberOfGuardians, quorum, move(elGamalPublicKey), move(commitmentHash),
                    move(manifestHash));
    }

    unique_ptr<CiphertextElectionContext> CiphertextElectionContext::make(
      uint64_t numberOfGuardians, uint64_t quorum, const string &elGamalPublicKeyInHex,
      const string &commitmentHashInHex, const string &manifestHashInHex,
      std::unordered_map<std::string, std::string> extendedData)
    {
        auto elGamalPublicKey = ElementModP::fromHex(elGamalPublicKeyInHex);
        auto commitmentHash = ElementModQ::fromHex(commitmentHashInHex);
        auto manifestHash = ElementModQ::fromHex(manifestHashInHex);

        // ensure the elgamal public key instance is set as a fixed base
        elGamalPublicKey->setIsFixedBase(true);

        return make(numberOfGuardians, quorum, move(elGamalPublicKey), move(commitmentHash),
                    move(manifestHash), move(extendedData));
    }

    unique_ptr<CiphertextElectionContext> CiphertextElectionContext::make(
      uint64_t numberOfGuardians, uint64_t quorum, const string &elGamalPublicKeyInHex,
      const string &commitmentHashInHex, const string &manifestHashInHex,
      unique_ptr<ContextConfiguration> config)
    {
        auto elGamalPublicKey = ElementModP::fromHex(elGamalPublicKeyInHex);
        auto commitmentHash = ElementModQ::fromHex(commitmentHashInHex);
        auto manifestHash = ElementModQ::fromHex(manifestHashInHex);

        // ensure the elgamal public key instance is set as a fixed base
        elGamalPublicKey->setIsFixedBase(true);

        return make(numberOfGuardians, quorum, move(elGamalPublicKey), move(commitmentHash),
                    move(manifestHash), move(config));
    }

    unique_ptr<CiphertextElectionContext> CiphertextElectionContext::make(
      uint64_t numberOfGuardians, uint64_t quorum, const string &elGamalPublicKeyInHex,
      const string &commitmentHashInHex, const string &manifestHashInHex,
      unique_ptr<ContextConfiguration> config,
      std::unordered_map<std::string, std::string> extendedData)
    {
        auto elGamalPublicKey = ElementModP::fromHex(elGamalPublicKeyInHex);
        auto commitmentHash = ElementModQ::fromHex(commitmentHashInHex);
        auto manifestHash = ElementModQ::fromHex(manifestHashInHex);

        // ensure the elgamal public key instance is set as a fixed base
        elGamalPublicKey->setIsFixedBase(true);

        return make(numberOfGuardians, quorum, move(elGamalPublicKey), move(commitmentHash),
                    move(manifestHash), move(config), move(extendedData));
    }

#pragma endregion

} // namespace electionguard
