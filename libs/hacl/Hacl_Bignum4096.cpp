#include "Hacl_Bignum4096.hpp"

#include "Hacl_Bignum4096.h"
#include "Hacl_GenericField64.h"

#include <stdexcept>

using std::out_of_range;

namespace hacl
{
    struct handle_destructor {
        void operator()(Hacl_Bignum_MontArithmetic_bn_mont_ctx_u64 *handle) const
        {
            Hacl_Bignum4096_mont_ctx_free(handle);
        }
    };

    typedef std::unique_ptr<Hacl_Bignum_MontArithmetic_bn_mont_ctx_u64, handle_destructor>
      HaclBignumContext4096;

    struct Bignum4096::Impl {

        HaclBignumContext4096 context;
        Impl(uint64_t *elem)
        {
            HaclBignumContext4096 ctx{Hacl_Bignum4096_mont_ctx_init(elem)};
            context = std::move(ctx);
        }
    };

    Bignum4096::Bignum4096(uint64_t *elem) : pimpl(new Impl(elem)) {}
    Bignum4096::~Bignum4096() {}

    uint64_t Bignum4096::add(uint64_t *a, uint64_t *b, uint64_t *res)
    {
        return Hacl_Bignum4096_add(a, b, res);
    }

    void Bignum4096::addMod(uint64_t *n, uint64_t *a, uint64_t *b, uint64_t *res)
    {
        Hacl_Bignum4096_add_mod(n, a, b, res);
    }

    uint64_t Bignum4096::sub(uint64_t *a, uint64_t *b, uint64_t *res)
    {
        return Hacl_Bignum4096_sub(a, b, res);
    }

    void Bignum4096::subMod(uint64_t *n, uint64_t *a, uint64_t *b, uint64_t *res)
    {
        Hacl_Bignum4096_sub_mod(n, a, b, res);
    }

    void Bignum4096::mul(uint64_t *a, uint64_t *b, uint64_t *res)
    {
        Hacl_Bignum4096_mul(a, b, res);
    }

    bool Bignum4096::mod(uint64_t *n, uint64_t *a, uint64_t *res)
    {
        return Hacl_Bignum4096_mod(n, a, res);
    }

    bool Bignum4096::modExp(uint64_t *n, uint64_t *a, uint32_t bBits, uint64_t *b, uint64_t *res,
                            bool useConstTime /* = true */)
    {
        if (bBits <= 0) {
            return false;
        }
        if (useConstTime) {
            return Hacl_Bignum4096_mod_exp_consttime(n, a, bBits, b, res);
        }
        return Hacl_Bignum4096_mod_exp_vartime(n, a, bBits, b, res);
    }

    bool Bignum4096::modInvPrime(uint64_t *n, uint64_t *a, uint64_t *res)
    {
        return Hacl_Bignum4096_mod_inv_prime_vartime(n, a, res);
    }

    uint64_t *Bignum4096::fromBytes(uint32_t len, uint8_t *bytes)
    {
        return Hacl_Bignum4096_new_bn_from_bytes_be(len, bytes);
    }

    void Bignum4096::toBytes(uint64_t *bytes, uint8_t *res)
    {
        return Hacl_Bignum4096_bn_to_bytes_be(bytes, res);
    }

    uint64_t Bignum4096::lessThan(uint64_t *a, uint64_t *b)
    {
        return Hacl_Bignum4096_lt_mask(a, b);
    }

    void Bignum4096::mod(uint64_t *a, uint64_t *res) const
    {
        Hacl_Bignum4096_mod_precomp(pimpl->context.get(), a, res);
    }

    void Bignum4096::modExp(uint64_t *a, uint32_t bBits, uint64_t *b, uint64_t *res,
                            bool useConstTime /* = true */) const
    {
        if (bBits <= 0) {
            throw out_of_range("hacl::Bignum4096->modExp:: bbits <= 0");
        }
        if (useConstTime) {
            return Hacl_Bignum4096_mod_exp_consttime_precomp(pimpl->context.get(), a, bBits, b,
                                                             res);
        }
        return Hacl_Bignum4096_mod_exp_vartime_precomp(pimpl->context.get(), a, bBits, b, res);
    }

    void Bignum4096::modInvPrime(uint64_t *a, uint64_t *res) const
    {
        Hacl_Bignum4096_mod_inv_prime_vartime_precomp(pimpl->context.get(), a, res);
    }

    void Bignum4096::to_montgomery_form(uint64_t *a, uint64_t *aM) const
    {
        Hacl_GenericField64_to_field(pimpl->context.get(), a, aM);
    }

    void Bignum4096::from_montgomery_form(uint64_t *aM, uint64_t *a) const
    {
        Hacl_GenericField64_from_field(pimpl->context.get(), aM, a);
    }

    void Bignum4096::montgomery_mod_mul_stay_in_mont_form(uint64_t *aM, uint64_t *bM,
                                                          uint64_t *cM) const
    {
        Hacl_GenericField64_mul(pimpl->context.get(), aM, bM, cM);
    }
} // namespace hacl
