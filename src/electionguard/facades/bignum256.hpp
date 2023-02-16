#ifndef __ELECTIONGUARD_FACADES_BIGNUM256_HPP_INCLUDED__
#define __ELECTIONGUARD_FACADES_BIGNUM256_HPP_INCLUDED__

#include "../../../libs/hacl/Hacl_Bignum256.hpp"
#include "electionguard/export.h"

#include <cstdint>
#include <memory>
#include <vector>

namespace electionguard::facades
{
    /// <summary>
    /// A Bignum256 instance initialized with the small prime montgomery context
    /// </summary>
    const EG_INTERNAL_API hacl::Bignum256 &CONTEXT_Q()
    {
        static hacl::Bignum256 instance{const_cast<uint64_t *>(Q_ARRAY_REVERSE)};
        return instance;
    }
} // namespace electionguard::facades

#endif /* __ELECTIONGUARD_FACADES_BIGNUM256_HPP_INCLUDED__ */