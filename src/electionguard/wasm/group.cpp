#include "electionguard/group.hpp"

#include "../log.hpp"

#include <emscripten/bind.h>
#include <emscripten/html5.h>
#include <iostream>

using electionguard::add_mod_q;
using electionguard::ElementModQ;
using electionguard::Log;

using namespace emscripten;

class GroupFunctions
{
  public:
    static std::unique_ptr<ElementModQ> addModQ(ElementModQ &a, ElementModQ &b)
    {
        std::cout << "add_mod_q " << a.toHex() << " + " << b.toHex() << std::endl;
        auto result = add_mod_q(a, b);
        std::cout << "add_mod_q " << result->toHex() << std::endl;
        // throws an error
        // Log::info(": ", result.get(), "say_hello");
        return std::move(result);
    }
};

EMSCRIPTEN_BINDINGS(electionguard)
{
    class_<ElementModQ>("ElementModQ")
      //   .constructor<const std::vector<uint64_t>, bool>()
      // .smart_ptr<std::unique_ptr<ElementModQ>>("std::unique_ptr<ElementModQ>")
      //   .function("get", &ElementModQ::get, allow_raw_pointers())
      //   .function("length", &ElementModQ::length)
      //   .function("isInBounds", &ElementModQ::isInBounds)
      //   .function("toBytes", &ElementModQ::toBytes)
      .function("toHex", &ElementModQ::toHex)
      //   .function("clone", &ElementModQ::clone)
      .class_function("fromHex", &ElementModQ::fromHex)
      .class_function("fromUint64", &ElementModQ::fromUint64);

    // register_vector<uint8_t>("std::vector<uint8_t>");
    // register_vector<uint64_t>("std::vector<uint64_t>");

    class_<GroupFunctions>("GroupFunctions").class_function("addModQ", &GroupFunctions::addModQ);
}