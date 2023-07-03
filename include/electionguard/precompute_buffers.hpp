#ifndef __ELECTIONGUARD_CPP_PRECOMPUTE_BUFFERS_HPP_INCLUDED__
#define __ELECTIONGUARD_CPP_PRECOMPUTE_BUFFERS_HPP_INCLUDED__

#include "electionguard/group.hpp"

#include <array>
#include <cstdint>
#include <electionguard/constants.h>
#include <electionguard/export.h>
#include <iomanip>
#include <iostream>
#include <memory>
#include <mutex>
#include <optional>
#include <queue>

namespace electionguard
{
    /// <summary>
    /// This object holds the Triple for the entries in the precomputed triple_queue
    /// The three items contained in this object are a random exponent (exp),
    /// g ^ exp mod p (g_to_exp) and K ^ exp mod p (pubkey_to_exp - where K is
    /// the public key).
    /// </summary>
    class EG_API Triple
    {
        std::unique_ptr<ElementModQ> exp;
        std::unique_ptr<ElementModP> g_to_exp;
        std::unique_ptr<ElementModP> pubkey_to_exp;

      public:
        explicit Triple(const ElementModP &publicKey) { generateTriple(publicKey); }
        Triple() {}
        Triple(std::unique_ptr<ElementModQ> exp, std::unique_ptr<ElementModP> g_to_exp,
               std::unique_ptr<ElementModP> pubkey_to_exp);

        Triple(const Triple &triple);
        Triple(Triple &&);
        ~Triple();

        Triple &operator=(const Triple &triple);
        Triple &operator=(Triple &&);

        std::unique_ptr<Triple> clone();

        std::unique_ptr<ElementModQ> clone_exp() { return exp->clone(); }
        std::unique_ptr<ElementModP> clone_g_to_exp() { return g_to_exp->clone(); }
        std::unique_ptr<ElementModP> clone_pubkey_to_exp() { return pubkey_to_exp->clone(); }

        ElementModQ *get_exp() const { return exp.get(); }
        ElementModP *get_g_to_exp() const { return g_to_exp.get(); }
        ElementModP *get_pubkey_to_exp() const { return pubkey_to_exp.get(); }

      protected:
        void generateTriple(const ElementModP &publicKey);
    };

    /// <summary>
    /// This object holds the Quadruple for the entries in the precomputed
    /// quadruple_queue. The four items contained in this object are the
    /// first random exponent (exp1), the second random exponent (exp2)
    /// g ^ exp1 mod p (g_to_exp1) and (g ^ exp2 mod p) * (K ^ exp mod p)
    /// (g_to_exp2 mult_by_pubkey_to_exp1 - where K is the public key).
    /// </summary>
    class EG_API Quadruple
    {
        std::unique_ptr<ElementModQ> exp1;
        std::unique_ptr<ElementModQ> exp2;
        std::unique_ptr<ElementModP> g_to_exp1;
        std::unique_ptr<ElementModP> g_to_exp2_mult_by_pubkey_to_exp1;

      public:
        explicit Quadruple(const ElementModP &publicKey) { generateQuadruple(publicKey); }
        Quadruple(){};
        Quadruple(std::unique_ptr<ElementModQ> exp1, std::unique_ptr<ElementModQ> exp2,
                  std::unique_ptr<ElementModP> g_to_exp1,
                  std::unique_ptr<ElementModP> g_to_exp2_mult_by_pubkey_to_exp1);
        Quadruple(const Quadruple &quadruple);
        Quadruple(Quadruple &&);
        ~Quadruple();

        Quadruple &operator=(const Quadruple &quadruple);
        Quadruple &operator=(Quadruple &&);

        std::unique_ptr<Quadruple> clone();

        std::unique_ptr<ElementModQ> clone_exp1() { return exp1->clone(); }
        std::unique_ptr<ElementModQ> clone_exp2() { return exp2->clone(); }
        std::unique_ptr<ElementModP> clone_g_to_exp1() { return g_to_exp1->clone(); }
        std::unique_ptr<ElementModP> clone_g_to_exp2_mult_by_pubkey_to_exp1()
        {
            return g_to_exp2_mult_by_pubkey_to_exp1->clone();
        }

        ElementModQ *get_exp1() const { return exp1.get(); }
        ElementModQ *get_exp2() const { return exp2.get(); }
        ElementModP *get_g_to_exp1() const { return g_to_exp1.get(); }
        ElementModP *get_g_to_exp2_mult_by_pubkey_to_exp1() const
        {
            return g_to_exp2_mult_by_pubkey_to_exp1.get();
        }

      protected:
        void generateQuadruple(const ElementModP &publicKey);
    };

    /// <summary>
    /// This object holds the two Triples and a Quadruple of precomputed
    /// values that are used to speed up encryption of a selection.
    /// Since the values are precomputed it removes all the exponentiations
    /// from the ElGamal encryption of the selection as well as the
    /// computation of the Chaum Pedersen proof.
    /// </summary>
    class EG_API TwoTriplesAndAQuadruple
    {
        std::unique_ptr<Triple> triple1;
        std::unique_ptr<Triple> triple2;
        std::unique_ptr<Quadruple> quad;

      public:
        explicit TwoTriplesAndAQuadruple() {}
        TwoTriplesAndAQuadruple(std::unique_ptr<Triple> in_triple1,
                                std::unique_ptr<Triple> in_triple2,
                                std::unique_ptr<Quadruple> in_quad);
        TwoTriplesAndAQuadruple(const TwoTriplesAndAQuadruple &other);
        TwoTriplesAndAQuadruple(TwoTriplesAndAQuadruple &&);
        ~TwoTriplesAndAQuadruple();

        TwoTriplesAndAQuadruple &operator=(const TwoTriplesAndAQuadruple &other);
        TwoTriplesAndAQuadruple &operator=(TwoTriplesAndAQuadruple &&);

        std::unique_ptr<TwoTriplesAndAQuadruple> clone();

        std::unique_ptr<Triple> clone_triple1() { return triple1->clone(); }
        std::unique_ptr<Triple> clone_triple2() { return triple2->clone(); }
        std::unique_ptr<Quadruple> clone_quad() { return quad->clone(); }

        Triple *get_triple1() const { return triple1.get(); }
        Triple *get_triple2() const { return triple2.get(); }
        Quadruple *get_quad() const { return quad.get(); }
    };

    /// <summary>
    /// A buffer of precomputed values that are used to speed up encryption
    /// of a selection. Since the values are precomputed it removes many the
    /// exponentiations from the ElGamal encryption of the selection as well
    /// as the computation of the Chaum Pedersen proof.
    ///
    /// The precompute buffer is a queue of TwoTriplesAndAQuadruple objects.
    /// The queue is filled by a background thread. The background thread
    /// will fill the queue until it reaches the max queue size. The max
    /// queue size is set by the caller and defaults to 5000. The queue
    /// size is set by the caller in the init method.
    ///
    /// This class is initialized against a specific public key and is thread safe.
    /// </summary>
    class EG_API PrecomputeBuffer
    {
      public:
        /// <summary>
        /// The init method initializes the precompute and allows the queue
        /// size to be set.
        ///
        /// <param name="publicKey">the elgamal public key for the election</param>
        /// <param name="maxQueueSize">by default the quad queue size is 5000, so
        ///                             10000 triples, if the caller wants the
        ///                             queue size to be different then this
        ///                             parameter is used</param>
        /// <param name="shouldAutoPopulate">controls whether the
        ///                                           precompute buffer should
        ///                                           automatically populate
        ///                                           itself</param>
        /// </summary>
        PrecomputeBuffer(const ElementModP &publicKey, uint32_t maxQueueSize = 0,
                         bool shouldAutoPopulate = false);

        PrecomputeBuffer(const PrecomputeBuffer &other) = delete;
        PrecomputeBuffer(PrecomputeBuffer &&other) = delete;
        PrecomputeBuffer &operator=(const PrecomputeBuffer &) = delete;
        PrecomputeBuffer &operator=(PrecomputeBuffer &&) = delete;
        ~PrecomputeBuffer();

      public:
        /// <summary>
        /// clear the precomputations queues
        /// </summary>
        void clear();

        /// <summary>
        /// The start method populates the precomputations queues with
        /// values used by encryptSelection. The function is stopped by calling
        /// stop. Pre-computed values are currently computed by generating
        /// two triples and a quad. We do this because two triples and a quad
        /// are need for an encryptSelection.
        /// <returns>once the queue is populated</returns>
        /// </summary>
        void start();

        /// <summary>
        /// The start method populates the precomputations queues with
        /// values used by encryptSelection. The function is stopped by calling
        /// stop. Pre-computed values are currently computed by generating
        /// two triples and a quad. We do this because two triples and a quad
        /// are need for an encryptSelection.
        /// <returns>immediately and schedules work in the background</returns>
        /// </summary>
        void startAsync();

        /// <summary>
        /// The stopPopulating method stops the population of the
        /// precomputations queues started by the populate method.
        /// </summary>
        void stop();

        /// <summary>
        /// Get the currently set maximum queue size for the number
        /// of quadruples to generate. The number of triples in
        /// the triple_queue will be twice this.
        /// </summary>
        uint32_t getMaxQueueSize();

        /// <summary>
        /// Get the current number of quadruples in the quadruple_queue,
        /// the number of triples in the triple_queue will be twice this.
        /// </summary>
        uint32_t getCurrentQueueSize();

        /// <summary>
        /// Get the public key that the precompute buffer is initialized against.
        /// </summary>
        ElementModP *getPublicKey();

        /// <summary>
        /// Get the next triple from the triple queue.
        /// If no triple exists, one is created.
        ///
        /// This method is called by hashedElgamalEncrypt in order to get
        /// the precomputed value to perform the hashed elgamal encryption.
        /// </summary>
        std::unique_ptr<Triple> getTriple();

        /// <summary>
        /// Pop the next triple from the triple queue.
        /// If no triple exists, then nullopt is returned.
        /// </summary>
        std::optional<std::unique_ptr<Triple>> popTriple();

        /// <summary>
        /// Get the next two triples and a quadruple from the queues.
        /// If no quadruple exists, one is created.
        ///
        /// This method is called by encryptSelection in order to get
        /// the precomputed values to encrypt the selection and make a
        /// proof for it.
        /// </summary>
        std::unique_ptr<TwoTriplesAndAQuadruple> getTwoTriplesAndAQuadruple();

        /// <summary>
        /// Pop the next quadruple set from the triple queue.
        /// If no quadruple exists, then nullopt is returned.
        /// </summary>
        std::optional<std::unique_ptr<TwoTriplesAndAQuadruple>> popTwoTriplesAndAQuadruple();

      protected:
        static std::tuple<std::unique_ptr<Triple>, std::unique_ptr<Triple>>
        createTwoTriples(const ElementModP &publicKey);
        static std::unique_ptr<TwoTriplesAndAQuadruple>
        createTwoTriplesAndAQuadruple(const ElementModP &publicKey);

      private:
        uint32_t maxQueueSize = DEFAULT_PRECOMPUTE_SIZE;
        bool isRunning = false;
        bool shouldAutoPopulate = false;
        std::mutex triple_queue_lock;
        std::mutex quad_queue_lock;
        std::unique_ptr<ElementModP> publicKey;
        std::queue<std::unique_ptr<Triple>> triple_queue;
        std::queue<std::unique_ptr<TwoTriplesAndAQuadruple>> twoTriplesAndAQuadruple_queue;
    };

    /// <summary>
    /// A singleton context for a collection of precomputed triples and quadruples.
    ///
    /// When initializing the context, the caller can specify the maximum number of
    /// quadruples to precompute. The number of triples in the triple queue will be
    /// twice this.
    ///
    /// The context is initialized against a specific public key. There can only be one context
    /// initialized at a time. If the caller attempts to initialize the context with a different
    /// public key, then the context will be cleared and re-initialized.
    ///
    /// The context is thread safe.
    /// </summary>
    class EG_API PrecomputeBufferContext
    {
      public:
        PrecomputeBufferContext(const PrecomputeBufferContext &) = delete;
        PrecomputeBufferContext(PrecomputeBufferContext &&) = delete;
        PrecomputeBufferContext &operator=(const PrecomputeBufferContext &) = delete;
        PrecomputeBufferContext &operator=(PrecomputeBufferContext &&) = delete;

      private:
        PrecomputeBufferContext() {}
        ~PrecomputeBufferContext() {}

      private:
        static PrecomputeBufferContext &getInstance()
        {
            static PrecomputeBufferContext instance;
            return instance;
        }

      public:
        /// <summary>
        /// clear the precomputations queues
        /// </summary>
        static void clear();

        /// <summary>
        /// The init method initializes the precompute and allows the queue
        /// size to be set.
        ///
        /// <param name="publicKey">the elgamal public key for the election</param>
        /// <param name="maxQueueSize">by default the quad queue size is 5000, so
        ///                             10000 triples, if the caller wants the
        ///                             queue size to be different then this
        ///                             parameter is used</param>
        /// </summary>
        static void initialize(const ElementModP &publicKey, uint32_t maxQueueSize = 0);

        /// <summary>
        /// The start method populates the precomputations queues with
        /// values used by encryptSelection. The function is stopped by calling
        /// stop. Pre-computed values are currently computed by generating
        /// two triples and a quad. We do this because two triples and a quad
        /// are need for an encryptSelection.
        /// <returns>once the queue is populated</returns>
        /// </summary>
        static void start();

        /// <summary>
        /// The start method populates the precomputations queues with
        /// values used by encryptSelection. The function is stopped by calling
        /// stop. Pre-computed values are currently computed by generating
        /// two triples and a quad. We do this because two triples and a quad
        /// are need for an encryptSelection.
        ///
        /// calling this override will re-initialize the context with the
        /// provided public key.
        ///
        /// <param name="publicKey">the elgamal public key for the election</param>
        /// <returns>once the queue is populated</returns>
        /// </summary>
        static void start(const ElementModP &publicKey);

        /// <summary>
        /// The start method populates the precomputations queues with
        /// values used by encryptSelection. The function is stopped by calling
        /// stop. Pre-computed values are currently computed by generating
        /// two triples and a quad. We do this because two triples and a quad
        /// are need for an encryptSelection.
        /// <returns>immediately and schedules work in the background</returns>
        /// </summary>
        static void startAsync(const ElementModP &publicKey);

        /// <summary>
        /// The stopPopulating method stops the population of the
        /// precomputations queues started by the populate method.
        /// <returns>void</returns>
        /// </summary>
        static void stop();

        /// <summary>
        /// Get the currently set maximum queue size for the number
        /// of quadruples to generate. The number of triples in
        /// the triple_queue will be twice this.
        /// <returns>uint32_t</returns>
        /// </summary>
        static uint32_t getMaxQueueSize();

        /// <summary>
        /// Get the current number of quadruples in the quadruple_queue,
        /// the number of triples in the triple_queue will be twice this.
        /// </summary>
        static uint32_t getCurrentQueueSize();

        /// <summary>
        /// Get the public key that the precompute buffer is initialized against.
        /// </summary>
        static ElementModP *getPublicKey();

        /// <summary>
        /// Get the next triple from the triple queue.
        /// This method is called by hashedElgamalEncrypt in order to get
        /// the precomputed value to perform the hashed elgamal encryption.
        /// </summary>
        static std::unique_ptr<Triple> getTriple();

        /// <summary>
        /// Pop the next triple from the triple queue.
        /// If no triple exists, then nullopt is returned.
        /// </summary>
        static std::optional<std::unique_ptr<Triple>> popTriple();

        /// <summary>
        /// Get the next two triples and a quadruple from the queues.
        /// This method is called by encryptSelection in order to get
        /// the precomputed values to encrypt the selection and make a
        /// proof for it.
        /// <returns>std::unique_ptr<TwoTriplesAndAQuadruple></returns>
        /// </summary>
        static std::unique_ptr<TwoTriplesAndAQuadruple> getTwoTriplesAndAQuadruple();

        /// <summary>
        /// Pop the next quadruple set from the triple queue.
        /// If no quadruple exists, then nullopt is returned.
        /// </summary>
        static std::optional<std::unique_ptr<TwoTriplesAndAQuadruple>> popTwoTriplesAndAQuadruple();

      private:
        std::mutex _mutex;
        std::unique_ptr<PrecomputeBuffer> _instance = nullptr;
    };

} // namespace electionguard

#endif /* __ELECTIONGUARD_CPP_PRECOMPUTE_BUFFERS_HPP_INCLUDED__ */
