#ifndef __ELECTIONGUARD_CPP_RANDOM_HPP_INCLUDED__
#define __ELECTIONGUARD_CPP_RANDOM_HPP_INCLUDED__

#include <ctime>
#include <electionguard/export.h>
#include <iomanip>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>

using std::bad_alloc;
using std::put_time;
using std::stringstream;
using std::vector;

namespace electionguard
{
    enum ByteSize {
        CHAR = 8U,
        SHORT = 16U,
        INT = 32U,
        LONG = 64U,
        SHA256 = 256U,
        SHA384 = 384U,
        SHA512 = 512U
    };

    /// <summary>
    /// A convenience wrapper around the HACL* HMAC Deterministic Random Bit Generator
    /// that supports retrieving an arbitrary number of pseudo-random bytes as specified.
    ///
    /// This implementation currently does not support reseeding the same instantiation
    /// and expects the caller to initialize a new instance before the entropy pool is exhausted
    ///
    /// Please refer to the NIST publication for more information:
    /// https://nvlpubs.nist.gov/nistpubs/SpecialPublications/NIST.SP.800-90Ar1.pdf
    /// </summary>
    class EG_INTERNAL_API Random
    {
      public:
        /// <summary>
        /// Get pseudo-random bytes for the specified ByteSize.
        ///
        /// Note:
        /// This implmenetation expects the OS random source to have at least `size * 2`
        /// available entropy to seed the DRBG properly.  Consumers should consider the
        /// available entropy before calling this function.
        /// </summary>
        static vector<uint8_t> getBytes(ByteSize size = SHA256);

      private:
        Random() {}
    };
} // namespace electionguard

#endif /* __ELECTIONGUARD_CPP_RANDOM_HPP_INCLUDED__ */
