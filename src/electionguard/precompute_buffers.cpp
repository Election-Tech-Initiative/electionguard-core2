#include "electionguard/group.hpp"
#include "log.hpp"
#include "utils.hpp"

#include <array>
#include <cstdint>
#include <electionguard/constants.h>
#include <electionguard/export.h>
#include <electionguard/precompute_buffers.hpp>
#include <iomanip>
#include <iostream>
#include <memory>

using std::begin;
using std::copy;
using std::end;
using std::lock_guard;
using std::make_unique;
using std::unique_ptr;

namespace electionguard
{
#pragma region PrecomputedEncryption
    PrecomputedEncryption::PrecomputedEncryption(unique_ptr<ElementModQ> exp,
                                                 unique_ptr<ElementModP> g_to_exp,
                                                 unique_ptr<ElementModP> pubkey_to_exp)
    {
        this->secret = move(exp);
        this->pad = move(g_to_exp);
        this->blindingFactor = move(pubkey_to_exp);
    }

    PrecomputedEncryption::PrecomputedEncryption(const PrecomputedEncryption &other)
    {
        this->secret = other.secret->clone();
        this->pad = other.pad->clone();
        this->blindingFactor = other.blindingFactor->clone();
    }

    PrecomputedEncryption::PrecomputedEncryption(PrecomputedEncryption &&other)
    {
        this->secret = move(other.secret);
        this->pad = move(other.pad);
        this->blindingFactor = move(other.blindingFactor);
    }

    PrecomputedEncryption::~PrecomputedEncryption() = default;

    PrecomputedEncryption &PrecomputedEncryption::operator=(const PrecomputedEncryption &triple)
    {
        this->secret = triple.secret->clone();
        this->pad = triple.pad->clone();
        this->blindingFactor = triple.blindingFactor->clone();
        return *this;
    }

    PrecomputedEncryption &PrecomputedEncryption::operator=(PrecomputedEncryption &&triple)
    {
        this->secret = move(triple.secret);
        this->pad = move(triple.pad);
        this->blindingFactor = move(triple.blindingFactor);
        return *this;
    }

    void PrecomputedEncryption::generate(const ElementModP &publicKey)
    {
        // generate a random rho
        secret = ElementModQ::fromHex(
          "2B3B025E50E09C119CBA7E9448ACD1CABC9447EF39BF06327D81C665CDD86296"); // rand_q();
        pad = g_pow_p(*secret);                                                // g ^ r
        blindingFactor = pow_mod_p(publicKey, *secret);                        // K ^ r
    }

    unique_ptr<PrecomputedEncryption> PrecomputedEncryption::clone()
    {
        return make_unique<PrecomputedEncryption>(secret->clone(), pad->clone(),
                                                  blindingFactor->clone());
    }

#pragma endregion

#pragma region PrecomputedFakeDisjuctiveCommitments

    PrecomputedFakeDisjuctiveCommitments::PrecomputedFakeDisjuctiveCommitments(
      unique_ptr<ElementModQ> exp1, unique_ptr<ElementModQ> exp2, unique_ptr<ElementModP> g_to_exp1,
      unique_ptr<ElementModP> dataZero, unique_ptr<ElementModP> dataOne)
    {
        this->secret1 = move(exp1);
        this->secret2 = move(exp2);
        this->pad = move(g_to_exp1);
        this->dataZero = move(dataZero);
        this->dataOne = move(dataOne);
    }

    PrecomputedFakeDisjuctiveCommitments::PrecomputedFakeDisjuctiveCommitments(
      const PrecomputedFakeDisjuctiveCommitments &other)
    {
        this->secret1 = other.secret1->clone();
        this->secret2 = other.secret2->clone();
        this->pad = other.pad->clone();
        this->dataZero = other.dataZero->clone();
        this->dataOne = other.dataOne->clone();
    }

    PrecomputedFakeDisjuctiveCommitments::PrecomputedFakeDisjuctiveCommitments(
      PrecomputedFakeDisjuctiveCommitments &&other)
    {
        this->secret1 = move(other.secret1);
        this->secret2 = move(other.secret2);
        this->pad = move(other.pad);
        this->dataZero = move(other.dataZero);
        this->dataOne = move(other.dataOne);
    }

    PrecomputedFakeDisjuctiveCommitments::~PrecomputedFakeDisjuctiveCommitments() = default;

    PrecomputedFakeDisjuctiveCommitments &PrecomputedFakeDisjuctiveCommitments::operator=(
      const PrecomputedFakeDisjuctiveCommitments &other)
    {
        this->secret1 = other.secret1->clone();
        this->secret2 = other.secret2->clone();
        this->pad = other.pad->clone();
        this->dataZero = other.dataZero->clone();
        this->dataOne = other.dataOne->clone();
        return *this;
    }

    PrecomputedFakeDisjuctiveCommitments &
    PrecomputedFakeDisjuctiveCommitments::operator=(PrecomputedFakeDisjuctiveCommitments &&other)
    {
        this->secret1 = move(other.secret1);
        this->secret2 = move(other.secret2);
        this->pad = move(other.pad);
        this->dataZero = move(other.dataZero);
        this->dataOne = move(other.dataOne);
        return *this;
    }

    void PrecomputedFakeDisjuctiveCommitments::generate(const ElementModP &publicKey)
    {
        // generate a random sigma and rho
        secret1 = rand_q();
        secret2 = rand_q();
        pad = g_pow_p(*secret1);
        dataZero = pow_mod_p(publicKey, *sub_mod_q(*secret1, *secret2)); // K^(𝑢1-w) mod p
        dataOne = pow_mod_p(publicKey, *add_mod_q(*secret1, *secret2));  // K^(w+𝑢0) mod p
    }

    unique_ptr<PrecomputedFakeDisjuctiveCommitments> PrecomputedFakeDisjuctiveCommitments::clone()
    {
        return make_unique<PrecomputedFakeDisjuctiveCommitments>(
          secret1->clone(), secret2->clone(), pad->clone(), dataZero->clone(), dataOne->clone());
    }

#pragma endregion

#pragma region TwoTriplesAndAQuadruple

    PrecomputedSelection::PrecomputedSelection(
      unique_ptr<PrecomputedEncryption> triple1, unique_ptr<PrecomputedEncryption> triple2,
      unique_ptr<PrecomputedFakeDisjuctiveCommitments> quad)
    {
        this->encryption = move(triple1);
        this->proof = move(triple2);
        this->fakeProof = move(quad);
    }

    PrecomputedSelection::PrecomputedSelection(const PrecomputedSelection &other)
    {
        this->encryption = other.encryption->clone();
        this->proof = other.proof->clone();
        this->fakeProof = other.fakeProof->clone();
    }

    PrecomputedSelection::PrecomputedSelection(PrecomputedSelection &&other)
    {
        this->encryption = move(other.encryption);
        this->proof = move(other.proof);
        this->fakeProof = move(other.fakeProof);
    }

    PrecomputedSelection::~PrecomputedSelection() = default;

    PrecomputedSelection &PrecomputedSelection::operator=(const PrecomputedSelection &other)
    {
        this->encryption = other.encryption->clone();
        this->proof = other.proof->clone();
        this->fakeProof = other.fakeProof->clone();
        return *this;
    }

    PrecomputedSelection &PrecomputedSelection::operator=(PrecomputedSelection &&other)
    {
        this->encryption = move(other.encryption);
        this->proof = move(other.proof);
        this->fakeProof = move(other.fakeProof);
        return *this;
    }

    unique_ptr<PrecomputedSelection> PrecomputedSelection::clone()
    {
        return make_unique<PrecomputedSelection>(encryption->clone(), proof->clone(),
                                                 fakeProof->clone());
    }

#pragma endregion

#pragma region PrecomputeBuffer

    // Lifecycle Methods

    PrecomputeBuffer::PrecomputeBuffer(const ElementModP &publicKey, uint32_t maxQueueSize,
                                       bool shouldAutoPopulate)
        : maxQueueSize(maxQueueSize == 0 ? DEFAULT_PRECOMPUTE_SIZE : maxQueueSize),
          shouldAutoPopulate(shouldAutoPopulate), publicKey(publicKey.clone())
    {
    }
    PrecomputeBuffer::~PrecomputeBuffer() = default;

    void PrecomputeBuffer::clear()
    {
        stop();

        std::lock_guard<std::mutex> lock1(encryption_queue_lock);
        uint32_t triple_size = encryption_queue.size();
        for (int i = 0; i < (int)triple_size; i++) {
            encryption_queue.pop();
        }

        std::lock_guard<std::mutex> lock2(selection_queue_lock);
        uint32_t twoTriplesAndAQuadruple_size = selection_queue.size();
        for (int i = 0; i < (int)twoTriplesAndAQuadruple_size; i++) {
            selection_queue.pop();
        }
    }

    void PrecomputeBuffer::start()
    {
        if (publicKey == nullptr) {
            throw std::runtime_error("PrecomputeBufferContext::start() - elgamalPublicKey is null");
        }

        isRunning = true;

        // This loop goes through until the queues are full but can be stopped
        // between generations of two triples and a quad. By full it means
        // we check how many quads are in the queue, to start with we will
        // try 5000 and see how that works. If the vendor wanted to pass the
        // queue size in we could use that.
        // for now we just go through the loop once
        int iteration_count = 0;
        int iterationCountToGenerateTwoTriples = 3;
        do {
            std::lock_guard<std::mutex> lock1(encryption_queue_lock);
            std::lock_guard<std::mutex> lock2(selection_queue_lock);

            // generate two triples and a quadruple
            // auto quad = createPrecomputedSelection(*publicKey);
            // selection_queue.push(move(quad));

            // This is very rudimentary. We can add a more complex algorithm in
            // the future, that would look at the queues and increase production if one
            // is getting lower than expected.
            // Every third iteration we generate two extra triples, one for use with
            // the contest constant chaum pedersen proof and one for hashed elgamal encryption
            // we need less of these because this exponentiation is done only every contest
            // encryption whereas the two triples and a quadruple is used every selection
            // encryption. The generating two triples every third iteration is a guess
            // on how many precomputes we will need.
            if ((iteration_count % iterationCountToGenerateTwoTriples) == 0) {
                auto tuple = createTwoPrecomputedEncryptions(*publicKey);
                // encryption_queue.push(move(std::get<0>(tuple)));
                // encryption_queue.push(move(std::get<1>(tuple)));
            }
            iteration_count++;

        } while (isRunning && selection_queue.size() < maxQueueSize);
    }

    void PrecomputeBuffer::startAsync()
    {
        // TODO: Issue #217: implement this
        start();
    }

    void PrecomputeBuffer::stop() { isRunning = false; }

    uint32_t PrecomputeBuffer::getMaxQueueSize() { return maxQueueSize; }

    uint32_t PrecomputeBuffer::getCurrentQueueSize() { return selection_queue.size(); }

    ElementModP *PrecomputeBuffer::getPublicKey() { return publicKey.get(); }

    std::unique_ptr<PrecomputedEncryption> PrecomputeBuffer::getPrecomputedEncryption()
    {
        if (!encryption_queue.empty()) {
            return popPrecomputedEncryption().value();
        }
        return make_unique<PrecomputedEncryption>(*publicKey);
    }

    std::optional<std::unique_ptr<PrecomputedEncryption>>
    PrecomputeBuffer::popPrecomputedEncryption()
    {
        unique_ptr<PrecomputedEncryption> result = nullptr;
        std::lock_guard<std::mutex> lock(encryption_queue_lock);

        // make sure there are enough in the queues
        if (!encryption_queue.empty()) {
            result = std::move(encryption_queue.front());
            encryption_queue.pop();
        }

        return result;
    }

    std::unique_ptr<PrecomputedSelection> PrecomputeBuffer::getPrecomputedSelection()
    {
        if (!selection_queue.empty()) {
            return popPrecomputedSelection().value();
        }

        return createPrecomputedSelection(*publicKey);
    }

    std::optional<std::unique_ptr<PrecomputedSelection>> PrecomputeBuffer::popPrecomputedSelection()
    {
        unique_ptr<PrecomputedSelection> result = nullptr;
        std::lock_guard<std::mutex> lock(selection_queue_lock);

        // make sure there are enough in the queues
        if (!selection_queue.empty()) {
            result = std::move(selection_queue.front());
            selection_queue.pop();
        }

        return result;
    }

    std::tuple<std::unique_ptr<PrecomputedEncryption>, std::unique_ptr<PrecomputedEncryption>>
    PrecomputeBuffer::createTwoPrecomputedEncryptions(const ElementModP &publicKey)
    {
        auto triple1 = make_unique<PrecomputedEncryption>(publicKey);
        //auto triple2 = make_unique<PrecomputedEncryption>(publicKey);
        return std::make_tuple(nullptr, nullptr); //move(triple1), move(triple2));
    }
    unique_ptr<PrecomputedSelection>
    PrecomputeBuffer::createPrecomputedSelection(const ElementModP &publicKey)
    {
        auto triple1 = make_unique<PrecomputedEncryption>(publicKey);
        auto triple2 = make_unique<PrecomputedEncryption>(publicKey);
        auto quad = make_unique<PrecomputedFakeDisjuctiveCommitments>(publicKey);
        return make_unique<PrecomputedSelection>(move(triple1), move(triple2), move(quad));
    }

#pragma endregion

#pragma region PrecomputeBufferContext

    void PrecomputeBufferContext::clear()
    {
        if (getInstance()._instance != nullptr) {
            getInstance()._instance->clear();
            getInstance()._instance = nullptr;
        }
    }

    void PrecomputeBufferContext::initialize(const ElementModP &publicKey,
                                             uint32_t maxQueueSize /* = 0 */)
    {
        clear();
        getInstance()._instance = make_unique<PrecomputeBuffer>(publicKey, maxQueueSize);
    }

    void PrecomputeBufferContext::start()
    {
        if (getInstance()._instance == nullptr) {
            throw std::runtime_error("PrecomputeBufferContext::start() called before "
                                     "PrecomputeBufferContext::initialize()");
        }
        getInstance()._instance->start();
    }

    void PrecomputeBufferContext::start(const ElementModP &elgamalPublicKey)
    {
        if (getInstance()._instance == nullptr) {
            getInstance()._instance = make_unique<PrecomputeBuffer>(elgamalPublicKey);
        } else if (*getInstance()._instance->getPublicKey() != elgamalPublicKey) {
            getInstance()._instance->clear();
            getInstance()._instance = make_unique<PrecomputeBuffer>(elgamalPublicKey);
        }
        getInstance()._instance->start();
    }

    void PrecomputeBufferContext::startAsync(const ElementModP &elgamalPublicKey)
    {
        if (getInstance()._instance == nullptr) {
            getInstance()._instance = make_unique<PrecomputeBuffer>(elgamalPublicKey);
        } else if (*getInstance()._instance->getPublicKey() != elgamalPublicKey) {
            getInstance()._instance->clear();
            getInstance()._instance = make_unique<PrecomputeBuffer>(elgamalPublicKey);
        }
        getInstance()._instance->startAsync();
    }

    void PrecomputeBufferContext::stop()
    {
        if (getInstance()._instance != nullptr) {
            getInstance()._instance->stop();
        }
    }

    uint32_t PrecomputeBufferContext::getMaxQueueSize()
    {
        if (getInstance()._instance == nullptr) {
            return 0;
        }
        return getInstance()._instance->getMaxQueueSize();
    }

    uint32_t PrecomputeBufferContext::getCurrentQueueSize()
    {
        if (getInstance()._instance == nullptr) {
            return 0;
        }
        return getInstance()._instance->getCurrentQueueSize();
    }

    ElementModP *PrecomputeBufferContext::getPublicKey()
    {
        if (getInstance()._instance != nullptr) {
            return getInstance()._instance->getPublicKey();
        }
        return nullptr;
    }

    std::unique_ptr<PrecomputedEncryption> PrecomputeBufferContext::getPrecomputedEncryption()
    {
        if (getInstance()._instance != nullptr) {
            return getInstance()._instance->getPrecomputedEncryption();
        }
        return nullptr;
    }

    std::optional<std::unique_ptr<PrecomputedEncryption>>
    PrecomputeBufferContext::popPrecomputedEncryption()
    {
        if (getInstance()._instance != nullptr) {
            return getInstance()._instance->popPrecomputedEncryption();
        }
        return std::nullopt;
    }

    std::unique_ptr<PrecomputedSelection> PrecomputeBufferContext::getPrecomputedSelection()
    {
        if (getInstance()._instance != nullptr) {
            return getInstance()._instance->getPrecomputedSelection();
        }
        return nullptr;
    }

    std::optional<std::unique_ptr<PrecomputedSelection>>
    PrecomputeBufferContext::popPrecomputedSelection()
    {
        if (getInstance()._instance != nullptr) {
            return getInstance()._instance->popPrecomputedSelection();
        }
        return std::nullopt;
    }

    //std::mutex PrecomputeBufferContext::_lock;

#pragma endregion

} // namespace electionguard
