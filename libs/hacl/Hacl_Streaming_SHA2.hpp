#ifndef __Hacl_STREAMING_SHA2_HPP_INCLUDED__
#define __Hacl_STREAMING_SHA2_HPP_INCLUDED__

#include <cstdint>
#include <memory>
#include <vector>

namespace hacl
{
    enum StreamingSHA2Mode { SHA2_256 = 0, SHA2_384 = 1, SHA2_512 = 2 };

    class StreamingSHA2
    {
      public:
        explicit StreamingSHA2(StreamingSHA2Mode mode = StreamingSHA2Mode::SHA2_256);
        ~StreamingSHA2();

        uint32_t update(uint8_t *input, uint32_t input_len) const;

        void finish(uint8_t *dst) const;

      private:
        struct Impl;
        std::unique_ptr<Impl> pimpl;
    };
} // namespace hacl

#endif /* __Hacl_STREAMING_SHA2_HPP_INCLUDED__ */