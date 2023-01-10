# Toolchain file compiling for x64 Windows

# Default to using MSVC on windows
set(USE_MSVC ON)

# Enable cross-compilation
if(NOT ${CMAKE_HOST_SYSTEM_PROCESSOR} MATCHES "x64|X64|amd64|AMD64|EM64T")
    message(STATUS "Cross Compiling for Windows 64")
    set(arch x64)

    # For some reason we have to set the system name here in order to make the
    # CMAKE_SYSTEM_PROCESSOR being picked up correctly.
    set(CMAKE_SYSTEM_NAME Windows)
    set(CMAKE_SYSTEM_VERSION "${CMAKE_HOST_SYSTEM_VERSION}")
    set(CMAKE_SYSTEM_PROCESSOR ${arch})
endif()