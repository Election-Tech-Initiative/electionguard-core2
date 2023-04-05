#ifndef __ELECTIONGUARD_CPP_POLYNOMIAL_HPP_INCLUDED__
#define __ELECTIONGUARD_CPP_POLYNOMIAL_HPP_INCLUDED__

#include "../../src/electionguard/log.hpp" // testing
#include "export.h"
#include "group.hpp"

#include <cstdint>
#include <functional>
#include <memory>
#include <numeric>
#include <set>
#include <vector>

namespace electionguard
{
    /// <summary>
    /// A Polynomial class that is used for calculating values on curves such as Lagrangian coefficients
    /// </summary>
    class EG_API Polynomial
    {
      public:
        Polynomial(const Polynomial &) = delete;
        Polynomial(Polynomial &&) = delete;
        Polynomial &operator=(const Polynomial &) = delete;
        Polynomial &operator=(Polynomial &&other) = delete;

      private:
        Polynomial() {}
        ~Polynomial() {}

      public:
        /// <summary>
        /// Interpolate a polynomial at a given coordinate using Lagrangian coefficients
        /// </summary>
        static std::unique_ptr<ElementModQ> interpolate(uint64_t coordinate,
                                                        const std::vector<uint64_t> &degrees)
        {
            std::vector<ElementModQ> elements;
            for (const auto &degree : degrees) {
                elements.push_back(*ElementModQ::fromUint64(degree));
            }
            auto result = interpolate(coordinate, elements);
            return result;
        }

        /// <summary>
        /// Interpolate a polynomial at a given coordinate using Lagrangian coefficients
        /// </summary>
        static std::unique_ptr<ElementModQ> interpolate(uint64_t coordinate,
                                                        const std::vector<ElementModQ> &degrees)
        {
            auto coordinateElement = ElementModQ::fromUint64(coordinate);
            auto result = interpolate(*coordinateElement, degrees);
            return result;
        }

        /// <summary>
        /// Interpolate a polynomial at a given coordinate using Lagrangian coefficients
        /// </summary>
        static std::unique_ptr<ElementModQ> interpolate(const ElementModQ &coordinate,
                                                        const std::vector<ElementModQ> &degrees)
        {
            // multiply all the degrees together to form the numerator
            auto numerator = mul_mod_q(degrees);

            // multiply all the differences together to form the denominator
            auto denominator = ElementModQ::fromUint64(1UL, true);
            for (const auto &degree : degrees) {
                auto difference = sub_mod_q(degree, coordinate);
                auto res = mul_mod_q(*denominator, *difference);
                denominator.swap(res);
            }

            // divide the numerator by the denominator using the modular inverse exponentiation
            auto result = div_mod_q(*numerator, *denominator);
            return result;
        }

        /// <summary>
        /// Interpolate a collection of lagrange coefficients for each coordinate in the set
        /// </summary>
        static std::vector<ElementModQ> interpolate(const std::set<ElementModQ> &coordinates)
        {
            std::vector<ElementModQ> result;
            // for each coordinate, calculate the lagrange coefficients
            for (const auto &coordinate : coordinates) {
                auto degrees =
                  reduce(coordinate, coordinates,
                         [](const ElementModQ &lhs, const ElementModQ &rhs) { return lhs != rhs; });

                auto res = interpolate(coordinate, degrees);
                result.push_back(*res);
            }
            return result;
        }

      private:
        static std::vector<ElementModQ>
        reduce(const ElementModQ &coordinate, const std::set<ElementModQ> &coordinates,
               std::function<bool(const ElementModQ &, const ElementModQ &)> comparator)
        {
            std::vector<ElementModQ> degrees;
            for (const auto &degree : coordinates) {
                if (comparator(coordinate, degree)) {
                    degrees.push_back(degree);
                }
            }
            return degrees;
        }
    };
} // namespace electionguard

#endif // __ELECTIONGUARD_CPP_POLYNOMIAL_HPP_INCLUDED__