#ifndef __ELECTIONGUARD_CPP_DISCRETE_LOG_HPP_INCLUDED__
#define __ELECTIONGUARD_CPP_DISCRETE_LOG_HPP_INCLUDED__

#include "async.hpp"
#include "constants.h"
#include "export.h"
#include "group.hpp"

#include <array>
#include <cstdint>
#include <memory>
#include <string>
#include <unordered_map>

namespace electionguard
{
    /// <Summary>
    /// Key hash function for looking up values in the DiscreteLogTable
    /// using a simplification of the MurmurHash3 algorithm (https://en.wikipedia.org/wiki/MurmurHash)
    /// which should be fast and provide a good distribution of values
    /// for the discrete log table up to 5 million entries (default DLOG_MAX_SIZE)
    /// for larger tables, we may want to consider a different hash function
    /// such as siphash (https://en.wikipedia.org/wiki/SipHash)
    /// </Summary>
    struct KeyHash {
        std::size_t operator()(const ElementModP &key) const
        {
            std::size_t hash = 0;
            for (int i = 0; i < MAX_P_LEN; i++) {
                hash ^= key.get()[i] + 0x9e3779b9 + (hash << 6) + (hash >> 2);
            }
            return hash;
        }
    };

    /// <Summary>
    /// Key Equality comparator for looking up values in the DiscreteLogTable
    /// </Summary>
    struct KeyEqual {
        bool operator()(const ElementModP &lhs, const ElementModP &rhs) const { return lhs == rhs; }
    };

    /// <Summary>
    /// A cache of discrete log values for the group G_q
    /// </Summary>
    using DiscreteLogTable = std::unordered_map<ElementModP, uint64_t, KeyHash, KeyEqual>;

    /// <Summary>
    /// A cache of discrete log values for the group G_q
    /// </Summary>
    class EG_API DiscreteLog
    {
      public:
        DiscreteLog(const DiscreteLog &) = delete;
        DiscreteLog(DiscreteLog &&) = delete;
        DiscreteLog &operator=(const DiscreteLog &) = delete;
        DiscreteLog &operator=(DiscreteLog &&other) = delete;

      private:
        DiscreteLog() {}
        ~DiscreteLog() {}

      public:
        static DiscreteLog &getInstance()
        {
            static DiscreteLog instance;
            return instance;
        }

        /// <Summary>
        /// Get the discrete log value for the given element
        /// </Summary>
        static uint64_t getAsync(const ElementModP &element);

      protected:
        uint64_t computeCache(const ElementModP &element);

      private:
        AsyncSemaphore task_lock;
        DiscreteLogTable cache = DiscreteLogTable(DLOG_MAX_SIZE);
    };

} // namespace electionguard

#endif // __ELECTIONGUARD_CPP_DISCRETE_LOG_HPP_INCLUDED__