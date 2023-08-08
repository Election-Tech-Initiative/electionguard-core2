#include "Hacl_Streaming_SHA2.hpp"

#include "Hacl_Hash_SHA2.h"

#include <memory>

using std::unique_ptr;

namespace hacl
{
    struct handle_destructor {
        void operator()(Hacl_Streaming_SHA2_state_sha2_224 *handle) const
        {
            Hacl_Streaming_SHA2_free_256(handle);
        }
    };

    typedef unique_ptr<Hacl_Streaming_SHA2_state_sha2_224, handle_destructor> StreamingSHA2Context;

    struct StreamingSHA2::Impl {
        StreamingSHA2Mode mode;
        StreamingSHA2Context state;

        Impl(StreamingSHA2Mode mode) : mode(mode)
        {
            // TODO: support other modes
            state = unique_ptr<Hacl_Streaming_SHA2_state_sha2_224, handle_destructor>(
              Hacl_Streaming_SHA2_create_in_256());
        }
    };

    StreamingSHA2::StreamingSHA2(StreamingSHA2Mode mode /* = StreamingSHA2Mode::SHA2_256 */)
        : pimpl(new Impl(mode))
    {
    }
    StreamingSHA2::~StreamingSHA2() {}

    uint32_t StreamingSHA2::update(uint8_t *input, uint32_t input_len) const
    {
        return Hacl_Streaming_SHA2_update_256(pimpl->state.get(), input, input_len);
    }

    void StreamingSHA2::finish(uint8_t *dst) const
    {
        Hacl_Streaming_SHA2_finish_256(pimpl->state.get(), dst);
    }
} // namespace hacl