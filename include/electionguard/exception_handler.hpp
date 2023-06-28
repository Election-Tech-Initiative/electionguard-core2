#ifndef __ELECTIONGUARD_CPP_EXCEPTION_HANDLER_HPP_INCLUDED__
#define __ELECTIONGUARD_CPP_EXCEPTION_HANDLER_HPP_INCLUDED__

#include "constants.h"
#include "export.h"
#include "group.hpp"

#include <array>
#include <cstdint>
#include <electionguard/async.hpp>
#include <memory>
#include <string>
#include <unordered_map>

namespace electionguard
{
    /// <Summary>
    /// A cache of discrete log values for the group G_q
    /// </Summary>
    class EG_API ExceptionHandler
    {
      public:
        ExceptionHandler(const ExceptionHandler &) = delete;
        ExceptionHandler(ExceptionHandler &&) = delete;
        ExceptionHandler &operator=(const ExceptionHandler &) = delete;
        ExceptionHandler &operator=(ExceptionHandler &&other) = delete;

      private:
        ExceptionHandler() {}
        ~ExceptionHandler() {}

      public:
        static ExceptionHandler &getInstance()
        {
            static ExceptionHandler instance;
            return instance;
        }

        static std::string getFunction();
        static std::string getMessage();
        static uint64_t getCode();
        static void setData(const std::string &function, uint64_t code, const std::exception &e);

      private:
        AsyncSemaphore task_lock;
        static std::string function;
        static std::string message;
        static uint64_t code;
    };

} // namespace electionguard

#endif // __ELECTIONGUARD_CPP_EXCEPTION_HANDLER_HPP_INCLUDED__