#include "electionguard/group.hpp"

#include "../log.hpp"

#include <emscripten/bind.h>
#include <emscripten/html5.h>
#include <iostream>

using namespace emscripten;
using namespace electionguard;
using namespace std;

class GroupFunctions
{
  public:
    static std::unique_ptr<ElementModQ> addModQ(ElementModQ &a, ElementModQ &b)
    {
        //std::cout << "addModQ " << a.toHex() << std::endl;
        auto result = add_mod_q(a, b);
        return std::move(result);
    }

    static std::unique_ptr<ElementModP> randomElementModP() { return rand_p(); }

    static std::unique_ptr<ElementModQ> randomElementModQ() { return rand_q(); }
};

EMSCRIPTEN_BINDINGS(electionguard)
{
    class_<ElementModP>("ElementModP")
      .function("isInBounds", &ElementModP::isInBounds)
      .function("toHex", &ElementModP::toHex)
      .function("copy", &ElementModQ::clone)
      .class_function("fromHex", &ElementModP::fromHex)
      .class_function("fromUint64", &ElementModP::fromUint64);

    class_<ElementModQ>("ElementModQ")
      .function("isInBounds", &ElementModQ::isInBounds)
      .function("toHex", &ElementModQ::toHex)
      .function("copy", &ElementModQ::clone)
      .function("toElementModP", &ElementModQ::toElementModP)
      .class_function("fromHex", &ElementModQ::fromHex)
      .class_function("fromUint64", &ElementModQ::fromUint64);

    class_<GroupFunctions>("GroupFunctions")
      .class_function("addModQ", &GroupFunctions::addModQ)
      .class_function("randomElementModP", &GroupFunctions::randomElementModP)
      .class_function("randomElementModQ", &GroupFunctions::randomElementModQ);
}