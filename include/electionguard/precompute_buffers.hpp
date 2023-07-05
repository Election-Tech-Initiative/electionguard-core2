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
    /// A PrecomputedEncryption is a triplet-set of precomputed values
    /// that are used to speed up encryption.
    ///
    /// it is used when encrypting a selection as an intermediate value
    /// and it is used when generating a proof for a selection.
    ///
    /// The three items contained in this object are
    /// - a random secret exponent r (secret - where r is [0,Q) and Q is the small prime modulus),
    /// - g^r mod p (pad - where g is the generator and p is the large prime modulus)
    /// - K^r mod p (blindingFactor - where K is the public key).
    /// </summary>
    class EG_API PrecomputedEncryption
    {
      public:
        explicit PrecomputedEncryption(const ElementModP &publicKey) { generate(publicKey); }
        PrecomputedEncryption(std::unique_ptr<ElementModQ> secret, std::unique_ptr<ElementModP> pad,
                              std::unique_ptr<ElementModP> blindingFactor);

        PrecomputedEncryption(const PrecomputedEncryption &other);
        PrecomputedEncryption(PrecomputedEncryption &&);
        ~PrecomputedEncryption();

        PrecomputedEncryption &operator=(const PrecomputedEncryption &other);
        PrecomputedEncryption &operator=(PrecomputedEncryption &&other);

        std::unique_ptr<PrecomputedEncryption> clone();

        /// <summary>
        /// the secret value (the expontent usually r, s, u, v, or w in the spec)
        /// </summary>
        ElementModQ *getSecret() const { return secret.get(); }

        /// <summary>
        /// the pad applied to the ciphertext message (g^r)
        /// </summary>
        ElementModP *getPad() const { return pad.get(); }

        /// <summary>
        /// the blinding factor applied to the message during encryption ( K^r)
        /// </summary>
        ElementModP *getBlindingFactor() const { return blindingFactor.get(); }

      protected:
        void generate(const ElementModP &publicKey);

      private:
        std::unique_ptr<ElementModQ> secret;
        std::unique_ptr<ElementModP> pad;
        std::unique_ptr<ElementModP> blindingFactor;
    };

    /// <summary>
    /// This object holds the Quadruple for the entries in the precomputed
    /// quadruple_queue. The four items contained in this object are
    /// - the first random exponent (exp1),
    /// - the second random exponent (exp2)
    /// - g ^ exp1 mod p (g_to_exp1)
    /// - (g ^ exp2 mod p) * (K ^ exp1 mod p)
    /// (g_to_exp2 mult_by_pubkey_to_exp1 - where K is the public key).
    /// </summary>
    class EG_API Quadruple // TODO: rename to PrecomputedFakeProof
    {
        std::unique_ptr<ElementModQ> secret1;
        std::unique_ptr<ElementModQ> secret2;
        std::unique_ptr<ElementModP> pad;
        std::unique_ptr<ElementModP> dataZero;
        std::unique_ptr<ElementModP> dataOne;

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

        ElementModQ *get_exp1() const { return secret1.get(); }
        ElementModQ *get_exp2() const { return secret2.get(); }
        ElementModP *get_g_to_exp1() const { return pad.get(); }
        ElementModP *get_g_to_exp2_mult_by_pubkey_to_exp1() const { return dataZero.get(); }

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
    class EG_API TwoTriplesAndAQuadruple // TODO: rename to PrecomputedSelection
    {
        std::unique_ptr<PrecomputedEncryption> encryption;
        std::unique_ptr<PrecomputedEncryption> proof;
        std::unique_ptr<Quadruple> fakeProof;

      public:
        explicit TwoTriplesAndAQuadruple() {}
        TwoTriplesAndAQuadruple(std::unique_ptr<PrecomputedEncryption> in_triple1,
                                std::unique_ptr<PrecomputedEncryption> in_triple2,
                                std::unique_ptr<Quadruple> in_quad);
        TwoTriplesAndAQuadruple(const TwoTriplesAndAQuadruple &other);
        TwoTriplesAndAQuadruple(TwoTriplesAndAQuadruple &&);
        ~TwoTriplesAndAQuadruple();

        TwoTriplesAndAQuadruple &operator=(const TwoTriplesAndAQuadruple &other);
        TwoTriplesAndAQuadruple &operator=(TwoTriplesAndAQuadruple &&);

        std::unique_ptr<TwoTriplesAndAQuadruple> clone();

        PrecomputedEncryption *get_triple1() const { return encryption.get(); }
        PrecomputedEncryption *get_triple2() const { return proof.get(); }
        Quadruple *get_quad() const { return fakeProof.get(); }
    };

    // TODO: range proof precompute table

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
        /// </summary>
        std::unique_ptr<PrecomputedEncryption> getTriple();

        /// <summary>
        /// Pop the next triple from the triple queue. If there is no triple
        /// in the queue, then nullopt is returned.
        ///
        /// This method is called by hashedElgamalEncrypt in order to get
        /// the precomputed value to perform the hashed elgamal encryption.
        ///
        /// This method is also called by ConstantChaumPedersenProof::make
        /// in order to get the precomputed value to make the proof.
        /// </summary>
        std::optional<std::unique_ptr<PrecomputedEncryption>> popTriple();

        /// <summary>
        /// Get the next two triples and a quadruple from the queues.
        /// If no quadruple exists, one is created.
        /// </summary>
        std::unique_ptr<TwoTriplesAndAQuadruple> getTwoTriplesAndAQuadruple();

        /// <summary>
        /// Pop the next quadruple set from the triple queue.
        /// If no quadruple exists, then nullopt is returned.
        ///
        /// This method is called by encryptSelection in order to get
        /// the precomputed values to encrypt the selection and make a
        /// proof for it.
        /// </summary>
        std::optional<std::unique_ptr<TwoTriplesAndAQuadruple>> popTwoTriplesAndAQuadruple();

      protected:
        static std::tuple<std::unique_ptr<PrecomputedEncryption>,
                          std::unique_ptr<PrecomputedEncryption>>
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
        std::queue<std::unique_ptr<PrecomputedEncryption>> triple_queue;
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
        /// Get the next triple from the triple queue. If there is no triple
        /// in the queue, then one is created.
        /// </summary>
        static std::unique_ptr<PrecomputedEncryption> getTriple();

        /// <summary>
        /// Pop the next triple from the triple queue. If there is no triple
        /// in the queue, then nullopt is returned.
        ///
        /// This method is called by hashedElgamalEncrypt in order to get
        /// the precomputed value to perform the hashed elgamal encryption.
        ///
        /// This method is also called by ConstantChaumPedersenProof::make
        /// in order to get the precomputed value to make the proof.
        /// </summary>
        static std::optional<std::unique_ptr<PrecomputedEncryption>> popTriple();

        /// <summary>
        /// Get the next two triples and a quadruple from the queues.
        /// If no quadruple exists, one is created.
        /// <returns>std::unique_ptr<TwoTriplesAndAQuadruple></returns>
        /// </summary>
        static std::unique_ptr<TwoTriplesAndAQuadruple> getTwoTriplesAndAQuadruple();

        /// <summary>
        /// Pop the next quadruple set from the triple queue.
        /// If no quadruple exists, then nullopt is returned.
        ///
        /// This method is called by encryptSelection in order to get
        /// the precomputed values to encrypt the selection and make a
        /// proof for it.
        /// </summary>
        static std::optional<std::unique_ptr<TwoTriplesAndAQuadruple>> popTwoTriplesAndAQuadruple();

      private:
        std::mutex _mutex;
        std::unique_ptr<PrecomputeBuffer> _instance = nullptr;
    };

} // namespace electionguard

#endif /* __ELECTIONGUARD_CPP_PRECOMPUTE_BUFFERS_HPP_INCLUDED__ */
