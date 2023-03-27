#include "../../src/electionguard/log.hpp"

#include <doctest/doctest.h>
#include <electionguard/discrete_log.hpp>
#include <electionguard/elgamal.hpp>
#include <electionguard/group.hpp>
#include <unordered_map>

using namespace electionguard;
using namespace std;

TEST_CASE("Can find discrete log valid value")
{
    // Arrange
    auto plantext = ElementModQ::fromUint64(100UL);
    auto exponent = g_pow_p(*plantext);

    // Act
    auto result = DiscreteLog::getAsync(*exponent);

    // Assert
    CHECK(result == 100UL);
}
