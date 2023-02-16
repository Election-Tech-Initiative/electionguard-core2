#include "Hacl_Bignum4096_32.hpp"

#include "Hacl_Bignum4096_32.h"
#include "Hacl_GenericField32.h"

#include <stdexcept>

using std::out_of_range;

namespace hacl
{
    struct handle_destructor {
        void operator()(Hacl_Bignum_MontArithmetic_bn_mont_ctx_u32 *handle) const
        {
            Hacl_Bignum4096_32_mont_ctx_free(handle);
        }
    };

    typedef std::unique_ptr<Hacl_Bignum_MontArithmetic_bn_mont_ctx_u32, handle_destructor>
      HaclBignumContext4096_32;

    struct Bignum4096_32::Impl {

        HaclBignumContext4096_32 context;
        Impl(uint32_t *elem)
        {
            HaclBignumContext4096_32 ctx{Hacl_Bignum4096_32_mont_ctx_init(elem)};
            context = std::move(ctx);
        }
    };

    Bignum4096_32::Bignum4096_32(uint32_t *elem) : pimpl(new Impl(elem)) {}
    Bignum4096_32::~Bignum4096_32() {}

    uint64_t Bignum4096_32::add(uint32_t *a, uint32_t *b, uint32_t *res)
    {
        return Hacl_Bignum4096_32_add(a, b, res);
    }

    uint64_t Bignum4096_32::sub(uint32_t *a, uint32_t *b, uint32_t *res)
    {
        return Hacl_Bignum4096_32_sub(a, b, res);
    }

    void Bignum4096_32::mul(uint32_t *a, uint32_t *b, uint32_t *res)
    {
        Hacl_Bignum4096_32_mul(a, b, res);
    }

    bool Bignum4096_32::mod(uint32_t *n, uint32_t *a, uint32_t *res)
    {
        return Hacl_Bignum4096_32_mod(n, a, res);
    }

    bool Bignum4096_32::modExp(uint32_t *n, uint32_t *a, uint32_t bBits, uint32_t *b, uint32_t *res,
                               bool useConstTime /* = true */)
    {
        if (bBits <= 0) {
            return false;
        }
        if (useConstTime) {
            return Hacl_Bignum4096_32_mod_exp_consttime(n, a, bBits, b, res);
        }
        return Hacl_Bignum4096_32_mod_exp_vartime(n, a, bBits, b, res);
    }

    bool Bignum4096_32::modInvPrime(uint32_t *n, uint32_t *a, uint32_t *res)
    {
        return Hacl_Bignum4096_32_mod_inv_prime_vartime(n, a, res);
    }

    uint32_t *Bignum4096_32::fromBytes(uint32_t len, uint8_t *bytes)
    {
        return Hacl_Bignum4096_32_new_bn_from_bytes_be(len, bytes);
    }

    void Bignum4096_32::toBytes(uint32_t *bytes, uint8_t *res)
    {
        return Hacl_Bignum4096_32_bn_to_bytes_be(bytes, res);
    }

    uint32_t Bignum4096_32::lessThan(uint32_t *a, uint32_t *b)
    {
        return Hacl_Bignum4096_32_lt_mask(a, b);
    }

    void Bignum4096_32::mod(uint32_t *a, uint32_t *res) const
    {
        Hacl_Bignum4096_32_mod_precomp(pimpl->context.get(), a, res);
    }

    void Bignum4096_32::modExp(uint32_t *a, uint32_t bBits, uint32_t *b, uint32_t *res,
                               bool useConstTime /* = true */) const
    {
        if (bBits <= 0) {
            throw out_of_range("hacl::Bignum4096_32->modExp:: bbits <= 0");
        }
        if (useConstTime) {
            return Hacl_Bignum4096_32_mod_exp_consttime_precomp(pimpl->context.get(), a, bBits, b,
                                                                res);
        }
        return Hacl_Bignum4096_32_mod_exp_vartime_precomp(pimpl->context.get(), a, bBits, b, res);
    }

    void Bignum4096_32::modInvPrime(uint32_t *a, uint32_t *res) const
    {
        Hacl_Bignum4096_32_mod_inv_prime_vartime_precomp(pimpl->context.get(), a, res);
    }

    void Bignum4096_32::to_montgomery_form(uint32_t *a, uint32_t *aM) const
    {
        Hacl_GenericField32_to_field(pimpl->context.get(), a, aM);
    }

    void Bignum4096_32::from_montgomery_form(uint32_t *aM, uint32_t *a) const
    {
        Hacl_GenericField32_from_field(pimpl->context.get(), aM, a);
    }

    void Bignum4096_32::montgomery_mod_mul_stay_in_mont_form(uint32_t *aM, uint32_t *bM,
                                                             uint32_t *cM) const
    {
        Hacl_GenericField32_mul(pimpl->context.get(), aM, bM, cM);
    }
} // namespace hacl
