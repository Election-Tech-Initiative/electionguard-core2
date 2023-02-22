#ifndef __Hacl_Bignum4096_HPP_INCLUDED__
#define __Hacl_Bignum4096_HPP_INCLUDED__

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
    class Bignum4096
    {
      public:
        explicit Bignum4096(uint64_t *elem);
        ~Bignum4096();

        /**
        Write `a + b mod 2^4096` in `res`.

        This functions returns the carry.

        The arguments a, b and res are meant to be 4096-bit bignums, i.e. uint64_t[64]
        */
        static uint64_t add(uint64_t *a, uint64_t *b, uint64_t *res);

        /**
        Write `(a + b) mod n` in `res`.

          The arguments a, b, n and the outparam res are meant to be 4096-bit bignums, i.e. uint64_t[64].

          Before calling this function, the caller will need to ensure that the following
          preconditions are observed.
          • a < n
          • b < n
        */
        static void addMod(uint64_t *n, uint64_t *a, uint64_t *b, uint64_t *res);

        /**
        Write `a - b mod 2^4096` in `res`.

        This functions returns the carry.

        The arguments a, b and res are meant to be 4096-bit bignums, i.e. uint64_t[64]
        */
        static uint64_t sub(uint64_t *a, uint64_t *b, uint64_t *res);

        /**
        Write `(a - b) mod n` in `res`.

          The arguments a, b, n and the outparam res are meant to be 4096-bit bignums, i.e. uint64_t[64].

          Before calling this function, the caller will need to ensure that the following
          preconditions are observed.
          • a < n
          • b < n
        */
        static void subMod(uint64_t *n, uint64_t *a, uint64_t *b, uint64_t *res);

        /**
        Write `a * b` in `res`.

        The arguments a and b are meant to be 4096-bit bignums, i.e. uint64_t[64].
        The outparam res is meant to be a 8192-bit bignum, i.e. uint64_t[128].
        */
        static void mul(uint64_t *a, uint64_t *b, uint64_t *res);

        /**
        Write `a mod n` in `res`.

        The argument a is meant to be a 8192-bit bignum, i.e. uint64_t[128].
        The argument n and the outparam res are meant to be 4096-bit bignums, i.e. uint64_t[64].

        The function returns false if any of the following preconditions are violated,
        true otherwise.
        • 1 < n
        • n % 2 = 1 
        */
        static bool mod(uint64_t *n, uint64_t *a, uint64_t *res);

        /**
        Write `a ^ b mod n` in `res`.

        The arguments a, n and the outparam res are meant to be 4096-bit bignums, i.e. uint64_t[64].

        The argument b is a bignum of any size, and bBits is an upper bound on the
        number of significant bits of b. A tighter bound results in faster execution
        time. When in doubt, the number of bits for the bignum size is always a safe
        default, e.g. if b is a 4096-bit bignum, bBits should be 4096.

        if useConstTime = false
        The function is *NOT* constant-time on the argument b. See the
        mod_exp_consttime_* functions for constant-time variants.

        useConstTime = true
        This function is constant-time over its argument b, at the cost of a slower
        execution time than mod_exp_vartime.

        The function returns false if any of the following preconditions are violated,
        true otherwise.
        • n % 2 = 1
        • 1 < n
        • b < pow2 bBits
        • a < n
        */
        static bool modExp(uint64_t *n, uint64_t *a, uint32_t bBits, uint64_t *b, uint64_t *res,
                           bool useConstTime = false);

        /**
        Write `a ^ (-1) mod n` in `res`.

          The arguments a, n and the outparam res are meant to be 4096-bit bignums, i.e. uint64_t[64].

          Before calling this function, the caller will need to ensure that the following
          preconditions are observed.
          • n is a prime

          The function returns false if any of the following preconditions are violated, true otherwise.
          • n % 2 = 1
          • 1 < n
          • 0 < a
          • a < n
        */
        static bool modInvPrime(uint64_t *n, uint64_t *a, uint64_t *res);

        static uint64_t *fromBytes(uint32_t len, uint8_t *bytes);

        static void toBytes(uint64_t *bytes, uint8_t *res);

        static uint64_t lessThan(uint64_t *a, uint64_t *b);

        /**
        Write `a mod n` in `res`.

        The argument a is meant to be a 8192-bit bignum, i.e. uint64_t[128].
        The argument n and the outparam res are meant to be 4096-bit bignums, i.e. uint64_t[64].

        The function returns false if any of the following preconditions are violated,
        true otherwise.
        • 1 < n
        • n % 2 = 1 
        */
        void mod(uint64_t *a, uint64_t *res) const;

        /**
        Write `a ^ b mod n` in `res`.

        The arguments a, n and the outparam res are meant to be 4096-bit bignums, i.e. uint64_t[64].

        The argument b is a bignum of any size, and bBits is an upper bound on the
        number of significant bits of b. A tighter bound results in faster execution
        time. When in doubt, the number of bits for the bignum size is always a safe
        default, e.g. if b is a 4096-bit bignum, bBits should be 4096.

        if useConstTime = false
        The function is *NOT* constant-time on the argument b. See the
        mod_exp_consttime_* functions for constant-time variants.

        useConstTime = true
        This function is constant-time over its argument b, at the cost of a slower
        execution time than mod_exp_vartime.

        The function returns false if any of the following preconditions are violated,
        true otherwise.
        • n % 2 = 1
        • 1 < n
        • b < pow2 bBits
        • a < n
        */
        void modExp(uint64_t *a, uint32_t bBits, uint64_t *b, uint64_t *res,
                    bool useConstTime = false) const;

        /**
        Write `a ^ (-1) mod n` in `res`.

          The arguments a, n and the outparam res are meant to be 4096-bit bignums, i.e. uint64_t[64].

          Before calling this function, the caller will need to ensure that the following
          preconditions are observed.
          • n is a prime

          The function returns false if any of the following preconditions are violated, true otherwise.
          • n % 2 = 1
          • 1 < n
          • 0 < a
          • a < n
        */
        void modInvPrime(uint64_t *a, uint64_t *res) const;

        void to_montgomery_form(uint64_t *a, uint64_t *aM) const;

        void from_montgomery_form(uint64_t *aM, uint64_t *a) const;

        void montgomery_mod_mul_stay_in_mont_form(uint64_t *aM, uint64_t *bM, uint64_t *cM) const;

      private:
        struct Impl;
        std::unique_ptr<Impl> pimpl;
    };
} // namespace hacl

#endif /* __Hacl_Bignum4096_HPP_INCLUDED__ */
