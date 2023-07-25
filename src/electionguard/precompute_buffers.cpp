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
#pragma region Triple
    Triple::Triple(unique_ptr<ElementModQ> exp, unique_ptr<ElementModP> g_to_exp,
                   unique_ptr<ElementModP> pubkey_to_exp)
    {
        this->exp = move(exp);
        this->g_to_exp = move(g_to_exp);
        this->pubkey_to_exp = move(pubkey_to_exp);
    }

    Triple::Triple(const Triple &triple)
    {
        this->exp = triple.exp->clone();
        this->g_to_exp = triple.g_to_exp->clone();
        this->pubkey_to_exp = triple.pubkey_to_exp->clone();
    }

    Triple::Triple(Triple &&triple)
    {
        this->exp = move(triple.exp);
        this->g_to_exp = move(triple.g_to_exp);
        this->pubkey_to_exp = move(triple.pubkey_to_exp);
    }

    Triple::~Triple() = default;

    Triple &Triple::operator=(const Triple &triple)
    {
        this->exp = triple.exp->clone();
        this->g_to_exp = triple.g_to_exp->clone();
        this->pubkey_to_exp = triple.pubkey_to_exp->clone();
        return *this;
    }

    Triple &Triple::operator=(Triple &&triple)
    {
        this->exp = move(triple.exp);
        this->g_to_exp = move(triple.g_to_exp);
        this->pubkey_to_exp = move(triple.pubkey_to_exp);
        return *this;
    }

    void Triple::generateTriple(const ElementModP &publicKey)
    {
        // generate a random rho
        exp = rand_q();
        g_to_exp = g_pow_p(*exp);
        pubkey_to_exp = pow_mod_p(publicKey, *exp);
    }

    unique_ptr<Triple> Triple::clone()
    {
        return make_unique<Triple>(exp->clone(), g_to_exp->clone(), pubkey_to_exp->clone());
    }

#pragma endregion

#pragma region Quadruple

    Quadruple::Quadruple(unique_ptr<ElementModQ> exp1, unique_ptr<ElementModQ> exp2,
                         unique_ptr<ElementModP> g_to_exp1,
                         unique_ptr<ElementModP> g_to_exp2_mult_by_pubkey_to_exp1)
    {
        this->exp1 = move(exp1);
        this->exp2 = move(exp2);
        this->g_to_exp1 = move(g_to_exp1);
        this->g_to_exp2_mult_by_pubkey_to_exp1 = move(g_to_exp2_mult_by_pubkey_to_exp1);
    }

    Quadruple::Quadruple(const Quadruple &quadruple)
    {
        this->exp1 = quadruple.exp1->clone();
        this->exp2 = quadruple.exp2->clone();
        this->g_to_exp1 = quadruple.g_to_exp1->clone();
        this->g_to_exp2_mult_by_pubkey_to_exp1 =
          quadruple.g_to_exp2_mult_by_pubkey_to_exp1->clone();
    }

    Quadruple::Quadruple(Quadruple &&quadruple)
    {
        this->exp1 = move(quadruple.exp1);
        this->exp2 = move(quadruple.exp2);
        this->g_to_exp1 = move(quadruple.g_to_exp1);
        this->g_to_exp2_mult_by_pubkey_to_exp1 = move(quadruple.g_to_exp2_mult_by_pubkey_to_exp1);
    }

    Quadruple::~Quadruple() = default;

    Quadruple &Quadruple::operator=(const Quadruple &quadruple)
    {
        this->exp1 = quadruple.exp1->clone();
        this->exp2 = quadruple.exp2->clone();
        this->g_to_exp1 = quadruple.g_to_exp1->clone();
        this->g_to_exp2_mult_by_pubkey_to_exp1 =
          quadruple.g_to_exp2_mult_by_pubkey_to_exp1->clone();
        return *this;
    }

    Quadruple &Quadruple::operator=(Quadruple &&quadruple)
    {
        this->exp1 = move(quadruple.exp1);
        this->exp2 = move(quadruple.exp2);
        this->g_to_exp1 = move(quadruple.g_to_exp1);
        this->g_to_exp2_mult_by_pubkey_to_exp1 = move(quadruple.g_to_exp2_mult_by_pubkey_to_exp1);
        return *this;
    }

    void Quadruple::generateQuadruple(const ElementModP &publicKey)
    {
        // generate a random sigma and rho
        exp1 = rand_q();
        exp2 = rand_q();
        g_to_exp1 = g_pow_p(*exp1);
        g_to_exp2_mult_by_pubkey_to_exp1 = mul_mod_p(*g_pow_p(*exp2), *pow_mod_p(publicKey, *exp1));
    }

    unique_ptr<Quadruple> Quadruple::clone()
    {
        return make_unique<Quadruple>(exp1->clone(), exp2->clone(), g_to_exp1->clone(),
                                      g_to_exp2_mult_by_pubkey_to_exp1->clone());
    }

#pragma endregion

#pragma region TwoTriplesAndAQuadruple

    TwoTriplesAndAQuadruple::TwoTriplesAndAQuadruple(unique_ptr<Triple> triple1,
                                                     unique_ptr<Triple> triple2,
                                                     unique_ptr<Quadruple> quad)
    {
        this->triple1 = move(triple1);
        this->triple2 = move(triple2);
        this->quad = move(quad);
    }

    TwoTriplesAndAQuadruple::TwoTriplesAndAQuadruple(const TwoTriplesAndAQuadruple &other)
    {
        this->triple1 = other.triple1->clone();
        this->triple2 = other.triple2->clone();
        this->quad = other.quad->clone();
    }

    TwoTriplesAndAQuadruple::TwoTriplesAndAQuadruple(TwoTriplesAndAQuadruple &&other)
    {
        this->triple1 = move(other.triple1);
        this->triple2 = move(other.triple2);
        this->quad = move(other.quad);
    }

    TwoTriplesAndAQuadruple::~TwoTriplesAndAQuadruple() = default;

    TwoTriplesAndAQuadruple &
    TwoTriplesAndAQuadruple::operator=(const TwoTriplesAndAQuadruple &other)
    {
        this->triple1 = other.triple1->clone();
        this->triple2 = other.triple2->clone();
        this->quad = other.quad->clone();
        return *this;
    }

    TwoTriplesAndAQuadruple &TwoTriplesAndAQuadruple::operator=(TwoTriplesAndAQuadruple &&other)
    {
        this->triple1 = move(other.triple1);
        this->triple2 = move(other.triple2);
        this->quad = move(other.quad);
        return *this;
    }

    unique_ptr<TwoTriplesAndAQuadruple> TwoTriplesAndAQuadruple::clone()
    {
        return make_unique<TwoTriplesAndAQuadruple>(triple1->clone(), triple2->clone(),
                                                    quad->clone());
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

        std::lock_guard<std::mutex> lock1(triple_queue_lock);
        uint32_t triple_size = triple_queue.size();
        for (int i = 0; i < (int)triple_size; i++) {
            triple_queue.pop();
        }

        std::lock_guard<std::mutex> lock2(quad_queue_lock);
        uint32_t twoTriplesAndAQuadruple_size = twoTriplesAndAQuadruple_queue.size();
        for (int i = 0; i < (int)twoTriplesAndAQuadruple_size; i++) {
            twoTriplesAndAQuadruple_queue.pop();
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
            std::lock_guard<std::mutex> lock1(triple_queue_lock);
            std::lock_guard<std::mutex> lock2(quad_queue_lock);

            // generate two triples and a quadruple
            auto quad = createTwoTriplesAndAQuadruple(*publicKey);
            twoTriplesAndAQuadruple_queue.push(move(quad));

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
                auto tuple = createTwoTriples(*publicKey);
                triple_queue.push(move(std::get<0>(tuple)));
                triple_queue.push(move(std::get<1>(tuple)));
            }
            iteration_count++;

        } while (isRunning && twoTriplesAndAQuadruple_queue.size() < maxQueueSize);
    }

    void PrecomputeBuffer::startAsync()
    {
        // TODO: Issue #217: implement this
        start();
    }

    void PrecomputeBuffer::stop() { isRunning = false; }

    uint32_t PrecomputeBuffer::getMaxQueueSize() { return maxQueueSize; }

    uint32_t PrecomputeBuffer::getCurrentQueueSize()
    {
        return twoTriplesAndAQuadruple_queue.size();
    }

    ElementModP *PrecomputeBuffer::getPublicKey() { return publicKey.get(); }

    std::unique_ptr<Triple> PrecomputeBuffer::getTriple()
    {
        if (!triple_queue.empty()) {
            return popTriple().value();
        }
        return make_unique<Triple>(*publicKey);
    }

    std::optional<std::unique_ptr<Triple>> PrecomputeBuffer::popTriple()
    {
        unique_ptr<Triple> result = nullptr;
        std::lock_guard<std::mutex> lock(triple_queue_lock);

        // make sure there are enough in the queues
        if (!triple_queue.empty()) {
            result = std::move(triple_queue.front());
            triple_queue.pop();
        }

        return result;
    }

    std::unique_ptr<TwoTriplesAndAQuadruple> PrecomputeBuffer::getTwoTriplesAndAQuadruple()
    {
        if (!twoTriplesAndAQuadruple_queue.empty()) {
            return popTwoTriplesAndAQuadruple().value();
        }

        return createTwoTriplesAndAQuadruple(*publicKey);
    }

    std::optional<std::unique_ptr<TwoTriplesAndAQuadruple>>
    PrecomputeBuffer::popTwoTriplesAndAQuadruple()
    {
        unique_ptr<TwoTriplesAndAQuadruple> result = nullptr;
        std::lock_guard<std::mutex> lock(quad_queue_lock);

        // make sure there are enough in the queues
        if (!twoTriplesAndAQuadruple_queue.empty()) {
            result = std::move(twoTriplesAndAQuadruple_queue.front());
            twoTriplesAndAQuadruple_queue.pop();
        }

        return result;
    }

    std::tuple<std::unique_ptr<Triple>, std::unique_ptr<Triple>>
    PrecomputeBuffer::createTwoTriples(const ElementModP &publicKey)
    {
        auto triple1 = make_unique<Triple>(publicKey);
        auto triple2 = make_unique<Triple>(publicKey);
        return std::make_tuple(move(triple1), move(triple2));
    }
    unique_ptr<TwoTriplesAndAQuadruple>
    PrecomputeBuffer::createTwoTriplesAndAQuadruple(const ElementModP &publicKey)
    {
        auto triple1 = make_unique<Triple>(publicKey);
        auto triple2 = make_unique<Triple>(publicKey);
        auto quad = make_unique<Quadruple>(publicKey);
        return make_unique<TwoTriplesAndAQuadruple>(move(triple1), move(triple2), move(quad));
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
        return getInstance()._instance->getMaxQueueSize();
    }

    uint32_t PrecomputeBufferContext::getCurrentQueueSize()
    {
        return getInstance()._instance->getCurrentQueueSize();
    }

    std::unique_ptr<Triple> PrecomputeBufferContext::getTriple()
    {
        if (getInstance()._instance != nullptr) {
            return getInstance()._instance->getTriple();
        }
        return nullptr;
    }

    std::optional<std::unique_ptr<Triple>> PrecomputeBufferContext::popTriple()
    {
        if (getInstance()._instance != nullptr) {
            return getInstance()._instance->popTriple();
        }
        return std::nullopt;
    }

    std::unique_ptr<TwoTriplesAndAQuadruple> PrecomputeBufferContext::getTwoTriplesAndAQuadruple()
    {
        if (getInstance()._instance != nullptr) {
            return getInstance()._instance->getTwoTriplesAndAQuadruple();
        }
        return nullptr;
    }

    std::optional<std::unique_ptr<TwoTriplesAndAQuadruple>>
    PrecomputeBufferContext::popTwoTriplesAndAQuadruple()
    {
        if (getInstance()._instance != nullptr) {
            return getInstance()._instance->popTwoTriplesAndAQuadruple();
        }
        return std::nullopt;
    }

    //std::mutex PrecomputeBufferContext::_lock;

#pragma endregion

} // namespace electionguard
