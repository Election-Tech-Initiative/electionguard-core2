#include "electionguard/precompute_buffers.hpp"

#include "../log.hpp"

#include <emscripten/bind.h>
#include <iostream>

using namespace emscripten;
using namespace electionguard;
using namespace std;

// a marker class since the PrecomputeBufferContext is a singleton
class PrecomputeBufferContextFacade
{
};

EMSCRIPTEN_BINDINGS(electionguard)
{
    class_<PrecomputeBufferContextFacade>("PrecomputeBufferContext")
      .class_function("clear", &PrecomputeBufferContext::clear)
      .class_function("initialize", &PrecomputeBufferContext::initialize)
      .class_function("start", select_overload<void()>(&PrecomputeBufferContext::start))
      .class_function("startAsync", &PrecomputeBufferContext::startAsync)
      .class_function("stop", &PrecomputeBufferContext::stop)
      .class_function("getMaxQueueSize", &PrecomputeBufferContext::getMaxQueueSize)
      .class_function("getCurrentQueueSize", &PrecomputeBufferContext::getCurrentQueueSize);
}