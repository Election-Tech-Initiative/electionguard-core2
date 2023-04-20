#ifndef __Hacl_Bignum4096_32_HPP_INCLUDED__
#define __Hacl_Bignum4096_32_HPP_INCLUDED__

#include <cstdint>
#include <memory>

namespace hacl
{
    /// <summary>
    /// wrapper class for calling 4096-bit hacl functions.
    //
    /// Instantiating this class creates a montgomery context
    /// that can be cached and reused to improve performance of mod and modexp functions
    /// </summary>
    class Bignum4096_32
    {
      public:
        explicit Bignum4096_32(uint32_t *elem);
        ~Bignum4096_32();

        // size of a 4096-bit bignum, i.e. uint32_t[128].
        static constexpr size_t size = 128;

        /**
        Write `a + b mod 2^256` in `res`.

        This functions returns the carry.

        The arguments a, b and res are meant to be 256-bit bignums, i.e. uint32_t[8]
        */
        static uint64_t add(uint32_t *a, uint32_t *b, uint32_t *res);

        /**
        Write `a - b mod 2^256` in `res`.

        This functions returns the carry.

        The arguments a, b and res are meant to be 256-bit bignums, i.e. uint32_t[8]
        */
        static uint64_t sub(uint32_t *a, uint32_t *b, uint32_t *res);

        /**
        Write `a * b` in `res`.

        The arguments a and b are meant to be 256-bit bignums, i.e. uint32_t[8].
        The outparam res is meant to be a 512-bit bignum, i.e. uint32_t[16].
        */
        static void mul(uint32_t *a, uint32_t *b, uint32_t *res);

        /**
        Write `a mod n` in `res`.

        The argument a is meant to be a 512-bit bignum, i.e. uint32_t[16].
        The argument n and the outparam res are meant to be 256-bit bignums, i.e. uint32_t[8].

        The function returns false if any of the following preconditions are violated,
        true otherwise.
        • 1 < n
        • n % 2 = 1
        */
        static bool mod(uint32_t *n, uint32_t *a, uint32_t *res);

        /**
        Write `a ^ b mod n` in `res`.

        The arguments a, n and the outparam res are meant to be 256-bit bignums, i.e. uint32_t[8].

        The argument b is a bignum of any size, and bBits is an upper bound on the
        number of significant bits of b. A tighter bound results in faster execution
        time. When in doubt, the number of bits for the bignum size is always a safe
        default, e.g. if b is a 256-bit bignum, bBits should be 256.

        when useConstTime == false:
        The function is *NOT* constant-time on the argument b. See the
        mod_exp_consttime_* functions for constant-time variants.

        when useConstTime == true:
        This function is constant-time over its argument b, at the cost of a slower
        execution time than mod_exp_vartime.

        The function returns false if any of the following preconditions are violated,
        true otherwise.
        • n % 2 = 1
        • 1 < n
        • b < pow2 bBits
        • a < n
        */
        static bool modExp(uint32_t *n, uint32_t *a, uint32_t bBits, uint32_t *b, uint32_t *res,
                           bool useConstTime = false);

        /**
        Write `a ^ (-1) mod n` in `res`.

          The arguments a, n and the outparam res are meant to be 4096-bit bignums, i.e. uint32_t[128].

          Before calling this function, the caller will need to ensure that the following
          preconditions are observed.
          • n is a prime

          The function returns false if any of the following preconditions are violated, true otherwise.
          • n % 2 = 1
          • 1 < n
          • 0 < a
          • a < n
        */
        static bool modInvPrime(uint32_t *n, uint32_t *a, uint32_t *res);

        static uint32_t *fromBytes(uint32_t len, uint8_t *bytes);

        static void toBytes(uint32_t *bytes, uint8_t *res);

        static uint32_t lessThan(uint32_t *a, uint32_t *b);

        /**
        Write `a mod n` in `res`.

        The argument a is meant to be a 512-bit bignum, i.e. uint32_t[16].
        The argument n and the outparam res are meant to be 256-bit bignums, i.e. uint32_t[8].

        The function returns false if any of the following preconditions are violated,
        true otherwise.
        • 1 < n
        • n % 2 = 1
        */
        void mod(uint32_t *a, uint32_t *res) const;

        /**
        Write `a ^ b mod n` in `res`.

        The arguments a, n and the outparam res are meant to be 256-bit bignums, i.e. uint32_t[8].

        The argument b is a bignum of any size, and bBits is an upper bound on the
        number of significant bits of b. A tighter bound results in faster execution
        time. When in doubt, the number of bits for the bignum size is always a safe
        default, e.g. if b is a 256-bit bignum, bBits should be 256.

        when useConstTime == false:
        The function is *NOT* constant-time on the argument b. See the
        mod_exp_consttime_* functions for constant-time variants.

        when useConstTime == true:
        This function is constant-time over its argument b, at the cost of a slower
        execution time than mod_exp_vartime.

        The function returns false if any of the following preconditions are violated,
        true otherwise.
        • n % 2 = 1
        • 1 < n
        • b < pow2 bBits
        • a < n
        */
        void modExp(uint32_t *a, uint32_t bBits, uint32_t *b, uint32_t *res,
                    bool useConstTime = false) const;

        /**
        Write `a ^ (-1) mod n` in `res`.

          The argument a and the outparam res are meant to be 4096-bit bignums, i.e. uint32_t[128].
          The argument k is a montgomery context obtained through Hacl_Bignum4096_mont_ctx_init.

          Before calling this function, the caller will need to ensure that the following
          preconditions are observed.
          • n is a prime
          • 0 < a
          • a < n
        */
        void modInvPrime(uint32_t *a, uint32_t *res) const;

        void to_montgomery_form(uint32_t *a, uint32_t *aM) const;

        void from_montgomery_form(uint32_t *aM, uint32_t *a) const;

        void montgomery_mod_mul_stay_in_mont_form(uint32_t *aM, uint32_t *bM, uint32_t *cM) const;

      private:
        struct Impl;
        std::unique_ptr<Impl> pimpl;
    };
} // namespace hacl

#endif /* __Hacl_Bignum4096_32_HPP_INCLUDED__ */
