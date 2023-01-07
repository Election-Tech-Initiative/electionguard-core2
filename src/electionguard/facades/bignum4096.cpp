#include "bignum4096.hpp"

#include "../../../libs/hacl/Hacl_Bignum4096.hpp"
#include "../../../libs/hacl/Hacl_Bignum4096_32.hpp"

#include <electionguard/constants.h>
#include <memory>
#include <variant>

// using HaclBignum4096_32 = hacl::Bignum4096_32;
// using HaclBignum4096 = hacl::Bignum4096;
using std::get;
using std::make_unique;
using std::move;
using std::unique_ptr;

namespace electionguard::facades
{
#ifdef USE_32BIT_MATH
    bool Bignum4096::use32BitMath = true;
#else
    bool Bignum4096::use32BitMath = false;
#endif

    using HaclBignumType =
      std::variant<unique_ptr<hacl::Bignum4096>, unique_ptr<hacl::Bignum4096_32>>;

    // using HaclBignumType =
    //   std::variant<hacl::Bignum4096, hacl::Bignum4096_32>;

    // struct CallMod32 {
    //     void operator()(const hacl::Bignum4096_32 &d) {}
    //     void operator()(const hacl::Bignum4096 &ed) {}
    // };

    struct Bignum4096::Impl {
        HaclBignumType element;
        bool prefer32BitMath = false;

        // Impl(uint32_t *elem) { this->prefer32BitMath = true; }
        // Impl(uint64_t *elem) { this->prefer32BitMath = true; }

        Impl(uint32_t *elem) : element(make_unique<hacl::Bignum4096_32>(move(elem)))
        {
            this->prefer32BitMath = true;
        }

        Impl(uint64_t *elem) : element(make_unique<hacl::Bignum4096>(move(elem)))
        {
            this->prefer32BitMath = Bignum4096::use32BitMath;
        }
    };

    Bignum4096::Bignum4096(uint32_t *elem) : pimpl(new Impl(elem)) {}
    Bignum4096::Bignum4096(uint64_t *elem) : pimpl(new Impl(elem)) {}
    Bignum4096::~Bignum4096() {}

#pragma region Static Members

    // Add
    uint32_t Bignum4096::add(uint32_t *a, uint32_t *b, uint32_t *res)
    {
        return hacl::Bignum4096_32::add(a, b, res);
    }
    uint64_t Bignum4096::add(uint64_t *a, uint64_t *b, uint64_t *res)
    {
        if (use32BitMath) {
            return add(reinterpret_cast<uint32_t *>(a), reinterpret_cast<uint32_t *>(b),
                       reinterpret_cast<uint32_t *>(res));
        }
        // return hacl::Bignum4096::add(a, b, res);
    }

    // Sub
    uint32_t Bignum4096::sub(uint32_t *a, uint32_t *b, uint32_t *res)
    {

        // return hacl::Bignum4096_32::sub(a, b, res);
    }
    uint64_t Bignum4096::sub(uint64_t *a, uint64_t *b, uint64_t *res)
    {
        if (use32BitMath) {
            return sub(reinterpret_cast<uint32_t *>(a), reinterpret_cast<uint32_t *>(b),
                       reinterpret_cast<uint32_t *>(res));
        }
        //  return hacl::Bignum4096::sub(a, b, res);
    }

    // Mul
    void Bignum4096::mul(uint32_t *a, uint32_t *b, uint32_t *res)
    {
        //  hacl::Bignum4096_32::mul(a, b, res);
    }
    void Bignum4096::mul(uint64_t *a, uint64_t *b, uint64_t *res)
    {
        if (use32BitMath) {
            return mul(reinterpret_cast<uint32_t *>(a), reinterpret_cast<uint32_t *>(b),
                       reinterpret_cast<uint32_t *>(res));
        }
        //  hacl::Bignum4096::mul(a, b, res);
    }

    // Mod
    bool Bignum4096::mod(uint32_t *n, uint32_t *a, uint32_t *res)
    {
        // return hacl::Bignum4096_32::mod(n, a, res);
    }
    bool Bignum4096::mod(uint64_t *n, uint64_t *a, uint64_t *res)
    {
        if (use32BitMath) {
            return mod(reinterpret_cast<uint32_t *>(n), reinterpret_cast<uint32_t *>(a),
                       reinterpret_cast<uint32_t *>(res));
        }
        //  return hacl::Bignum4096::mod(n, a, res);
    }

    // ModExp
    bool Bignum4096::modExp(uint32_t *n, uint32_t *a, uint32_t bBits, uint32_t *b, uint32_t *res,
                            bool useConstTime /* = true */)
    {
        // return hacl::Bignum4096_32::modExp(n, a, bBits, b, res, useConstTime);
    }
    bool Bignum4096::modExp(uint64_t *n, uint64_t *a, uint32_t bBits, uint64_t *b, uint64_t *res,
                            bool useConstTime /* = true */)
    {
        if (use32BitMath) {
            return modExp(reinterpret_cast<uint32_t *>(n), reinterpret_cast<uint32_t *>(a), bBits,
                          reinterpret_cast<uint32_t *>(b), reinterpret_cast<uint32_t *>(res));
        }
        //  return hacl::Bignum4096::modExp(n, a, bBits, b, res, useConstTime);
    }

    // Mod Inv
    bool Bignum4096::modInvPrime(uint32_t *n, uint32_t *a, uint32_t *res)
    {
        // return hacl::Bignum4096_32::modInvPrime(n, a, res);
    }

    bool Bignum4096::modInvPrime(uint64_t *n, uint64_t *a, uint64_t *res)
    {
        if (use32BitMath) {
            return modInvPrime(reinterpret_cast<uint32_t *>(n), reinterpret_cast<uint32_t *>(a),
                               reinterpret_cast<uint32_t *>(res));
        }
        // return hacl::Bignum4096::modInvPrime(n, a, res);
    }

    // FromBytes
    uint32_t *Bignum4096::fromBytes32(uint32_t len, uint8_t *bytes)
    {
        // return hacl::Bignum4096_32::fromBytes(len, bytes);
    }
    uint64_t *Bignum4096::fromBytes64(uint32_t len, uint8_t *bytes)
    {
        if (use32BitMath) {
            return reinterpret_cast<uint64_t *>(fromBytes32(len, bytes));
        }
        //  return hacl::Bignum4096::fromBytes(len, bytes);
    }

    // ToBytes
    void Bignum4096::toBytes(uint32_t *bytes, uint8_t *res)
    {
        // return hacl::Bignum4096_32::toBytes(bytes, res);
    }
    void Bignum4096::toBytes(uint64_t *bytes, uint8_t *res)
    {
        if (use32BitMath) {
            toBytes(reinterpret_cast<uint32_t *>(bytes), res);
            return;
        }
        //  return hacl::Bignum4096::toBytes(bytes, res);
    }

    // LessThan
    uint32_t Bignum4096::lessThan(uint32_t *a, uint32_t *b)
    {
        // return hacl::Bignum4096_32::lessThan(a, b);
    }
    uint64_t Bignum4096::lessThan(uint64_t *a, uint64_t *b)
    {
        //  return hacl::Bignum4096::lessThan(a, b);
    }

#pragma endregion

    template <class... Fs> struct overload : Fs... {
        template <class... Ts> overload(Ts &&...ts) : Fs{std::forward<Ts>(ts)}... {}

        using Fs::operator()...;
    };

    template <class... Ts> overload(Ts &&...) -> overload<std::remove_reference_t<Ts>...>;

    // helper type for the visitor #4
    template <class... Ts> struct overloaded : Ts... {
        using Ts::operator()...;
    };
    // explicit deduction guide (not needed as of C++20)
    template <class... Ts> overloaded(Ts...) -> overloaded<Ts...>;

    // template <class... Ts> struct overload : Ts... {
    //     using Ts::operator()...;
    // };
    // template <class... Ts> overload(Ts...) -> overload<Ts...>;

#pragma region Instance Members

    //     struct CallHaclMod {
    //     void operator()(const Derived& d) { d.PrintName(intro); }
    //     void operator()(const ExtraDerived& ed) { ed.PrintName(intro); }

    //     std::string_view intro;
    // };

    // void Bignum4096::mod(uint32_t *a, uint32_t *res) const
    // {
    //     //std::get<hacl::Bignum4096_32 *>(pimpl->element)->mod(a, res);

    //     std::visit(overloaded{
    //                  [a, res](hacl::Bignum4096_32 &element) { element.mod(a, res); },
    //                  [a, res](hacl::Bignum4096 &element) {
    //                      element.mod(reinterpret_cast<uint64_t *>(a),
    //                                  reinterpret_cast<uint64_t *>(res));
    //                  },
    //                },
    //                pimpl->element);
    // }
    // void Bignum4096::mod(uint64_t *a, uint64_t *res) const
    // {
    //     if (pimpl->prefer32BitMath) {
    //         mod(reinterpret_cast<uint32_t *>(a), reinterpret_cast<uint32_t *>(res));
    //         return;
    //     }
    //     std::get<hacl::Bignum4096 *>(pimpl->element)->mod(a, res);
    // }

    void Bignum4096::mod(uint32_t *a, uint32_t *res) const
    {
        // TODO: this one wa working
        // auto &guy = pimpl->element;
        // std::get<unique_ptr<hacl::Bignum4096_32>>(guy)->mod(a, res);
    }
    void Bignum4096::mod(uint64_t *a, uint64_t *res) const
    {
        // if (pimpl->prefer32BitMath) {
        //     mod(reinterpret_cast<uint32_t *>(a), reinterpret_cast<uint32_t *>(res));
        //     return;
        // }
        // std::get<hacl::Bignum4096>(pimpl->element).mod(a, res);
    }

    void Bignum4096::modExp(uint32_t *a, uint32_t bBits, uint32_t *b, uint32_t *res,
                            bool useConstTime /* = true */) const
    {
        //  std::get<hacl::Bignum4096_32>(pimpl->element).modExp(a, bBits, b, res, useConstTime);
    }
    void Bignum4096::modExp(uint64_t *a, uint32_t bBits, uint64_t *b, uint64_t *res,
                            bool useConstTime /* = true */) const
    {
        // if (pimpl->prefer32BitMath) {
        //     modExp(reinterpret_cast<uint32_t *>(a), bBits, reinterpret_cast<uint32_t *>(b),
        //            reinterpret_cast<uint32_t *>(res), useConstTime);
        //     return;
        // }

        // std::get<hacl::Bignum4096>(pimpl->element).modExp(a, bBits, b, res, useConstTime);
    }

    // ModInv
    void Bignum4096::modInvPrime(uint32_t *a, uint32_t *res) const {}
    void Bignum4096::modInvPrime(uint64_t *a, uint64_t *res) const {}

    void Bignum4096::to_montgomery_form(uint32_t *a, uint32_t *aM) const
    {
        //  std::get<hacl::Bignum4096_32>(pimpl->element).to_montgomery_form(a, aM);
    }
    void Bignum4096::to_montgomery_form(uint64_t *a, uint64_t *aM) const
    {
        // if (pimpl->prefer32BitMath) {
        //     to_montgomery_form(reinterpret_cast<uint32_t *>(a), reinterpret_cast<uint32_t *>(aM));
        // }
        // std::get<hacl::Bignum4096>(pimpl->element).to_montgomery_form(a, aM);
    }

    void Bignum4096::from_montgomery_form(uint32_t *aM, uint32_t *a) const
    {
        //  std::get<hacl::Bignum4096_32>(pimpl->element).from_montgomery_form(aM, a);
    }
    void Bignum4096::from_montgomery_form(uint64_t *aM, uint64_t *a) const
    {
        // if (pimpl->prefer32BitMath) {
        //     from_montgomery_form(reinterpret_cast<uint32_t *>(aM), reinterpret_cast<uint32_t *>(a));
        // }
        // std::get<hacl::Bignum4096>(pimpl->element).from_montgomery_form(aM, a);
    }

    void Bignum4096::montgomery_mod_mul_stay_in_mont_form(uint32_t *aM, uint32_t *bM,
                                                          uint32_t *cM) const
    {
        // std::get<hacl::Bignum4096_32>(pimpl->element)
        //   .montgomery_mod_mul_stay_in_mont_form(aM, bM, cM);
    }
    void Bignum4096::montgomery_mod_mul_stay_in_mont_form(uint64_t *aM, uint64_t *bM,
                                                          uint64_t *cM) const
    {
        // if (pimpl->prefer32BitMath) {
        //     montgomery_mod_mul_stay_in_mont_form(reinterpret_cast<uint32_t *>(aM),
        //                                          reinterpret_cast<uint32_t *>(bM),
        //                                          reinterpret_cast<uint32_t *>(cM));
        // }
        // std::get<hacl::Bignum4096>(pimpl->element).montgomery_mod_mul_stay_in_mont_form(aM, bM, cM);
    }

#pragma endregion

    const Bignum4096 &CONTEXT_P()
    {
#ifdef USE_32BIT_MATH
        static Bignum4096 instance{(uint32_t *)(P_ARRAY_REVERSE)};
#else
        static Bignum4096 instance{const_cast<uint64_t *>(P_ARRAY_REVERSE)};
#endif // _WIN32
        return instance;
    }
} // namespace electionguard::facades