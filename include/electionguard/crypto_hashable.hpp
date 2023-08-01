#ifndef __ELECTIONGUARD_CPP_CRYPTO_HASHABLE_HPP_INCLUDED__
#define __ELECTIONGUARD_CPP_CRYPTO_HASHABLE_HPP_INCLUDED__

#include "export.h"
#include "group.hpp"

#include <memory>

namespace electionguard
{
    /// <summary>
    /// Indicates that the derived class can be hashed
    /// </summary>
    class EG_API CryptoHashable
    {
      public:
        virtual ~CryptoHashable(){};

        /// <summary>
        /// Generates a hash given the fields on the implementing instance.
        /// </summary>
        virtual std::unique_ptr<ElementModQ> crypto_hash()
        {
            throw "CryptoHashable crypto_hash not implemented";
        };

        /// <summary>
        /// Generates a hash given the fields on the implementing instance.
        /// </summary>
        virtual std::unique_ptr<ElementModQ> crypto_hash() const
        {
            throw "CryptoHashable const crypto_hash not implemented";
        };
    };

} // namespace electionguard

#endif /* __ELECTIONGUARD_CPP_CRYPTO_HASHABLE_HPP_INCLUDED__ */
