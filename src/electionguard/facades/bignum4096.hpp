#ifndef __ELECTIONGUARD_FACADES_BIGNUM4096_HPP_INCLUDED__
#define __ELECTIONGUARD_FACADES_BIGNUM4096_HPP_INCLUDED__

#include "electionguard/export.h"

#include <cstdint>
#include <memory>

namespace electionguard::facades
{
    /// <summary>
    /// Wrapper class for calling 4096-bit hacl functions.
    ///
    /// Supports both 32-bit and 64-bit math operations.
    /// Defaults to 64-bit.
    ///
    /// To use 32-bit: toggle the `USE_32BIT_MATH` flag at compile time
    /// or instantiate the instance with a 32-bit value at runtime.
    ///
    /// Instantiating this class creates a montgomery context
    /// that can be cached and reused to improve performance of mod and modexp functions.
    /// </summary>
    class EG_INTERNAL_API Bignum4096
    {
      public:
        // use 32-bit math
        explicit Bignum4096(uint32_t *elem);

        // use 64-bit math, or use 32-bit if USE_32BIT_MATH is defined
        explicit Bignum4096(uint64_t *elem);

        // derive the math type from the constructor parameter
        explicit Bignum4096(uint64_t *elem, bool prefer32BitMath);
        ~Bignum4096();

        static bool use32BitMath;

        static uint32_t add(uint32_t *a, uint32_t *b, uint32_t *res);
        static uint64_t add(uint64_t *a, uint64_t *b, uint64_t *res);

        static uint32_t sub(uint32_t *a, uint32_t *b, uint32_t *res);
        static uint64_t sub(uint64_t *a, uint64_t *b, uint64_t *res);

        static void mul(uint32_t *a, uint32_t *b, uint32_t *res);
        static void mul(uint64_t *a, uint64_t *b, uint64_t *res);

        static bool mod(uint32_t *n, uint32_t *a, uint32_t *res);
        static bool mod(uint64_t *n, uint64_t *a, uint64_t *res);

        static bool modExp(uint32_t *n, uint32_t *a, uint32_t bBits, uint32_t *b, uint32_t *res,
                           bool useConstTime = false);
        static bool modExp(uint64_t *n, uint64_t *a, uint32_t bBits, uint64_t *b, uint64_t *res,
                           bool useConstTime = false);

        static bool modInvPrime(uint32_t *n, uint32_t *a, uint32_t *res);
        static bool modInvPrime(uint64_t *n, uint64_t *a, uint64_t *res);

        static uint32_t *fromBytes32(uint32_t len, uint8_t *bytes);
        static uint64_t *fromBytes64(uint32_t len, uint8_t *bytes);

        static void toBytes(uint32_t *bytes, uint8_t *res);
        static void toBytes(uint64_t *bytes, uint8_t *res);

        static uint32_t lessThan(uint32_t *a, uint32_t *b);
        static uint64_t lessThan(uint64_t *a, uint64_t *b);

        void mod(uint32_t *a, uint32_t *res) const;
        void mod(uint64_t *a, uint64_t *res) const;

        void modExp(uint32_t *a, uint32_t bBits, uint32_t *b, uint32_t *res,
                    bool useConstTime = false) const;
        void modExp(uint64_t *a, uint32_t bBits, uint64_t *b, uint64_t *res,
                    bool useConstTime = false) const;

        void modInvPrime(uint32_t *a, uint32_t *res) const;
        void modInvPrime(uint64_t *a, uint64_t *res) const;

        void to_montgomery_form(uint32_t *a, uint32_t *aM) const;
        void to_montgomery_form(uint64_t *a, uint64_t *aM) const;

        void from_montgomery_form(uint32_t *aM, uint32_t *a) const;
        void from_montgomery_form(uint64_t *aM, uint64_t *a) const;

        void montgomery_mod_mul_stay_in_mont_form(uint32_t *aM, uint32_t *bM, uint32_t *cM) const;
        void montgomery_mod_mul_stay_in_mont_form(uint64_t *aM, uint64_t *bM, uint64_t *cM) const;

      private:
        struct Impl;
        std::unique_ptr<Impl> pimpl;
    };

    /// <summary>
    /// A Bignum4096 instance initialized with the large prime montgomery context
    /// </summary>
    const EG_INTERNAL_API Bignum4096 &CONTEXT_P();
} // namespace electionguard::facades
#endif /* __Hacl_Bignum4096_HPP_INCLUDED__ */
