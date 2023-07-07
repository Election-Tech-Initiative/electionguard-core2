#include "electionguard/discrete_log.hpp"

#include "electionguard/constants.h"
#include "electionguard/group.hpp"
#include "log.hpp"

#include <array>
#include <cmath>
#include <cstdint>
#include <iomanip>
#include <iostream>
#include <memory>
#include <string>
#include <unordered_map>
#include <vector>

using std::array;
using std::make_unique;
using std::move;
using std::out_of_range;
using std::string;
using std::to_string;

namespace electionguard
{
    uint64_t DiscreteLog::getAsync(const ElementModP &element) { return getAsync(element, G()); }
    uint64_t DiscreteLog::getAsync(const ElementModP &element, const ElementModP &base)
    {
        // TODO: Issue #217: implement multithreading

        // if the base matches, try to find the element in the cache
        auto existingBase = getInstance().base.get();
        if (existingBase != nullptr && base == *existingBase) {
            // search for the existing element and return it if found
            auto iter = getInstance().cache.find(element);
            if (iter != getInstance().cache.end()) {
                return iter->second;
            }
        }

        {
            // otherwise, calculate the discrete log value
            auto cached = getInstance().computeCache(element, base);
            return cached;
        }
    }

    uint64_t DiscreteLog::computeCache(const ElementModP &element)
    {
        return computeCache(element, G());
    }

    uint64_t DiscreteLog::computeCache(const ElementModP &element, const ElementModP &base)
    {
        // TODO: ISSUE #360: handle multiple bases

        // if the base does not match, reset the table
        if (this->base.get() == nullptr || base != *this->base) {
            // clear the cache if the base has changed
            this->base = base.clone();
            this->cache.clear();
        }

        // initialize the cache with the first element
        if (cache.empty()) {
            cache[ONE_MOD_P()] = 0;
        }

        // find the element with the largest exponent
        // using a naive iteration over the cache
        const auto *lastElementPtr = &cache.begin()->first;
        uint64_t exponent = cache.begin()->second;
        for (const auto &pair : cache) {
            if (pair.second > exponent) {
                lastElementPtr = &pair.first;
                exponent = pair.second;
            }
        }

        auto lastElement = *lastElementPtr;

        // loop until we find the element or we reach the max size
        while (element != lastElement) {
            // increment the exponent
            exponent++;

            // check if the exponent is larger than the max
            if (exponent > DLOG_MAX_SIZE) {
                throw std::out_of_range("computeCache: size is larger than max.");
            }

            // multiply the last element by g
            lastElement = *mul_mod_p(*this->base, lastElement);

            // add the new element to the cache
            cache[lastElement] = exponent;
        }

        // return the exponent since we already have the value
        return exponent;
    }
} // namespace electionguard