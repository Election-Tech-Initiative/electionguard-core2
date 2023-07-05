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
    /// It is used when encrypting a selection as an intermediate value
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
    /// PrecomputedFakeDisjuctiveCommitments is a quintuplet-set of precomputed values
    /// that are used to speed up encryption. It works by pregenerating the fake proof
    /// values for an encryption of zero or one. Both fake proofs are made as part of
    /// this precompute object, but only one of them is used. It is up to the caller
    /// to determine which one to use.
    ///
    /// It is used when generating a fake proof as part of proving an encryption
    /// is an encryption of zero or one.
    ///
    /// The five items contained in this object are
    /// - the first random exponent (exp1),
    /// - the second random exponent (exp2)
    /// - g ^ exp1 mod p (g_to_exp1)
    /// - (g ^ exp2 mod p) * (K ^ exp1 mod p)
    /// (g_to_exp2 mult_by_pubkey_to_exp1 - where K is the public key).
    /// </summary>
    class EG_API PrecomputedFakeDisjuctiveCommitments
    {
      public:
        explicit PrecomputedFakeDisjuctiveCommitments(const ElementModP &publicKey)
        {
            generate(publicKey);
        }
        PrecomputedFakeDisjuctiveCommitments(std::unique_ptr<ElementModQ> secret1,
                                             std::unique_ptr<ElementModQ> secret2,
                                             std::unique_ptr<ElementModP> pad,
                                             std::unique_ptr<ElementModP> dataZero,
                                             std::unique_ptr<ElementModP> dataOne);
        PrecomputedFakeDisjuctiveCommitments(const PrecomputedFakeDisjuctiveCommitments &other);
        PrecomputedFakeDisjuctiveCommitments(PrecomputedFakeDisjuctiveCommitments &&other);
        ~PrecomputedFakeDisjuctiveCommitments();

        PrecomputedFakeDisjuctiveCommitments &
        operator=(const PrecomputedFakeDisjuctiveCommitments &other);
        PrecomputedFakeDisjuctiveCommitments &
        operator=(PrecomputedFakeDisjuctiveCommitments &&other);

        std::unique_ptr<PrecomputedFakeDisjuctiveCommitments> clone();

        /// <summary>
        /// The first secret value (either 𝑢0 or 𝑢1 in the spec)
        /// </summary>
        ElementModQ *getSecret1() const { return secret1.get(); }

        /// <summary>
        /// The second secret value (w in the spec)
        /// </summary>
        ElementModQ *getSecret2() const { return secret2.get(); }

        /// <summary>
        /// The pad applied to the ciphertext message (either a0 or a1 in the spec)
        /// </summary>
        ElementModP *getPad() const { return pad.get(); }

        /// <summary>
        /// The fake data proving an encryption of zero (b1 in the spec)
        /// Use this value when proving an encryption of zero.
        /// </summary>
        ElementModP *getDataZero() const { return dataZero.get(); }

        /// <summary>
        /// The pad applied to the ciphertext message (b0 in the spec)
        /// Use this value when proving an encryption of one.
        /// </summary>
        ElementModP *getDataOne() const { return dataOne.get(); }

      protected:
        void generate(const ElementModP &publicKey);

      private:
        std::unique_ptr<ElementModQ> secret1;
        std::unique_ptr<ElementModQ> secret2;
        std::unique_ptr<ElementModP> pad;
        std::unique_ptr<ElementModP> dataZero;
        std::unique_ptr<ElementModP> dataOne;
    };

    /// <summary>
    /// The PrecomputedSelection is a set of precomputed values that are used
    /// to speed up encryption of a selection. It removes most the exponentiations
    /// from the ElGamal encryption of the selection as well as the computation
    /// of the Chaum Pedersen proof.
    ///
    /// This object holds a Precomputed Encryption for the selection, a Precomputed
    /// Encryption for the proof, and a Precomputed Fake Disjunctive Commitment for
    /// the proof.
    /// </summary>
    class EG_API PrecomputedSelection
    {
      public:
        PrecomputedSelection(std::unique_ptr<PrecomputedEncryption> in_triple1,
                             std::unique_ptr<PrecomputedEncryption> in_triple2,
                             std::unique_ptr<PrecomputedFakeDisjuctiveCommitments> in_quad);
        PrecomputedSelection(const PrecomputedSelection &other);
        PrecomputedSelection(PrecomputedSelection &&);
        ~PrecomputedSelection();

        PrecomputedSelection &operator=(const PrecomputedSelection &other);
        PrecomputedSelection &operator=(PrecomputedSelection &&);

        std::unique_ptr<PrecomputedSelection> clone();

        /// <summary>
        /// Get the precomputed encryption for the selection.
        /// </summary>
        PrecomputedEncryption *getPartialEncryption() const { return encryption.get(); }

        /// <summary>
        /// Get the precomputed encryption for the proof.
        /// </summary>
        PrecomputedEncryption *getRealCommitment() const { return proof.get(); }

        /// <summary>
        /// Get the precomputed fake disjunctive commitment for the proof.
        /// </summary>
        PrecomputedFakeDisjuctiveCommitments *getFakeCommitment() const { return fakeProof.get(); }

      private:
        std::unique_ptr<PrecomputedEncryption> encryption;
        std::unique_ptr<PrecomputedEncryption> proof;
        std::unique_ptr<PrecomputedFakeDisjuctiveCommitments> fakeProof;
    };

    // TODO: range proof precompute table?

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
        std::unique_ptr<PrecomputedEncryption> getPrecomputedEncryption();

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
        std::optional<std::unique_ptr<PrecomputedEncryption>> popPrecomputedEncryption();

        /// <summary>
        /// Get the next two triples and a quadruple from the queues.
        /// If no quadruple exists, one is created.
        /// </summary>
        std::unique_ptr<PrecomputedSelection> getPrecomputedSelection();

        /// <summary>
        /// Pop the next quadruple set from the triple queue.
        /// If no quadruple exists, then nullopt is returned.
        ///
        /// This method is called by encryptSelection in order to get
        /// the precomputed values to encrypt the selection and make a
        /// proof for it.
        /// </summary>
        std::optional<std::unique_ptr<PrecomputedSelection>> popPrecomputedSelection();

      protected:
        static std::tuple<std::unique_ptr<PrecomputedEncryption>,
                          std::unique_ptr<PrecomputedEncryption>>
        createTwoPrecomputedEncryptions(const ElementModP &publicKey);
        static std::unique_ptr<PrecomputedSelection>
        createPrecomputedSelection(const ElementModP &publicKey);

      private:
        uint32_t maxQueueSize = DEFAULT_PRECOMPUTE_SIZE;
        bool isRunning = false;
        bool shouldAutoPopulate = false;
        std::mutex encryption_queue_lock;
        std::mutex selection_queue_lock;
        std::unique_ptr<ElementModP> publicKey;
        std::queue<std::unique_ptr<PrecomputedEncryption>> encryption_queue;
        std::queue<std::unique_ptr<PrecomputedSelection>> selection_queue;
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
        static std::unique_ptr<PrecomputedEncryption> getPrecomputedEncryption();

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
        static std::optional<std::unique_ptr<PrecomputedEncryption>> popPrecomputedEncryption();

        /// <summary>
        /// Get the next two triples and a quadruple from the queues.
        /// If no quadruple exists, one is created.
        /// <returns>std::unique_ptr<TwoTriplesAndAQuadruple></returns>
        /// </summary>
        static std::unique_ptr<PrecomputedSelection> getPrecomputedSelection();

        /// <summary>
        /// Pop the next quadruple set from the triple queue.
        /// If no quadruple exists, then nullopt is returned.
        ///
        /// This method is called by encryptSelection in order to get
        /// the precomputed values to encrypt the selection and make a
        /// proof for it.
        /// </summary>
        static std::optional<std::unique_ptr<PrecomputedSelection>> popPrecomputedSelection();

      private:
        std::mutex _mutex;
        std::unique_ptr<PrecomputeBuffer> _instance = nullptr;
    };

} // namespace electionguard

#endif /* __ELECTIONGUARD_CPP_PRECOMPUTE_BUFFERS_HPP_INCLUDED__ */
