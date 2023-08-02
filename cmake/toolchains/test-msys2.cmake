# Common cmake variables when running tests
set(BUILD_SHARED_LIBS ON)
set(EXPORT_INTERNALS ON)
set(USE_TEST_PRIMES OFF)
set(OPTION_ENABLE_TESTS ON)
set(LOG_LEVEL debug)
SET(CMAKE_C_COMPILER_WORKS 1)
SET(CMAKE_CXX_COMPILER_WORKS 1)

# if 32 bit math is set, we assume we need to set the compiler to 32 bit
# this is because we want to execute 32 bit tests on a 64 bit machine
if(USE_32BIT_MATH)
    set(USE_32BIT_MATH ON)
    set(arch x86)
    set(CMAKE_SYSTEM_NAME Windows)
    set(CMAKE_SYSTEM_VERSION "${CMAKE_HOST_SYSTEM_VERSION}")
    set(CMAKE_SYSTEM_PROCESSOR ${arch})
    set(CMAKE_C_COMPILER clang) 
    set(CMAKE_CXX_COMPILER clang++)
else()
    set(CMAKE_C_COMPILER clang) 
    set(CMAKE_CXX_COMPILER clang++)
endif()
