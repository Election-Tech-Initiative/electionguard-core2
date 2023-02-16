# Options to configure the library build

option(CMAKE_BUILD_TYPE "Build with or without debug symbols" Release)
option(BUILD_SHARED_LIBS "Build SHARED libraries" OFF)
option(EXPORT_INTERNALS "Export Internal Headers (useful for testing, do not use in prod)" OFF)
option(USE_32BIT_MATH "Use the 32 bit optimized math impl" OFF)
option(USE_TEST_PRIMES "Use the smaller test primes (useful for testing, do not use in prod)" OFF)
option(OPTION_ENABLE_TESTS "Enable support for testing private headers" OFF)
option(TEST_SPEC_VERSION "Use this spec version for tests" "0.95.0")
option(TEST_USE_SAMPLE "the sample to use, full, hamilton-general, minimal, small" "hamilton-general")
option(CODE_COVERAGE "Use code coverage" OFF)
option(OPTION_GENERATE_DOCS "Generate documentation" OFF)
option(USE_DYNAMIC_ANALYSIS "Enable Dynamic tools" OFF)

# Set a DEBUG definition
if(CMAKE_BUILD_TYPE MATCHES Debug)
    message("++ Setting DEBUG during compile")
    add_compile_definitions(DEBUG)
    add_compile_definitions(LOG_DEBUG)
    add_compile_definitions(JSON_DIAGNOSTICS=1)
endif()

# Allow explicitly setting debug logs in release builds
if(LOG_LEVEL MATCHES debug)
    message("++ Using DEBUG Logs")
    add_compile_definitions(LOG_DEBUG)
elseif(LOG_LEVEL MATCHES trace)
    message("++ Using TRACE Logs")
    add_compile_definitions(LOG_TRACE)
endif()

if(EXPORT_INTERNALS)
    message("++ Exporting Internal Headers")
    add_compile_definitions(EXPORT_INTERNALS)
endif()

if(USE_32BIT_MATH)
    message("++ Using 32-bit math operations")
    add_compile_definitions(USE_32BIT_MATH)
endif()

if(USE_TEST_PRIMES)
    message("++ Using Test Primes. Do not use in production.")
    add_compile_definitions(USE_TEST_PRIMES)
else()
    message("++ Using Standard Primes.")
    add_compile_definitions(USE_STANDARD_PRIMES)
endif()

# HACK: Disable explicit bzero on android
if(DEFINED CMAKE_ANDROID_ARCH_ABI)
    add_compile_definitions(LINUX_NO_EXPLICIT_BZERO)
endif()