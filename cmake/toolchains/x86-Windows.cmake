# Toolchain file compiling for x86 Windows

# ElectionGuard Options
set(USE_32BIT_MATH, ON)
set(USE_MSVC ON)


# Enable cross-compilation
if(NOT ${CMAKE_HOST_SYSTEM_PROCESSOR} MATCHES "x86|X86|i586|i686")
    message(STATUS "Cross Compiling for Windows x86")
    set(arch x86)

    # For some reason we have to set the system name here in order to make the
    # CMAKE_SYSTEM_PROCESSOR being picked up correctly.
    set(CMAKE_SYSTEM_NAME Windows)
    set(CMAKE_SYSTEM_VERSION "${CMAKE_HOST_SYSTEM_VERSION}")
    set(CMAKE_SYSTEM_PROCESSOR ${arch})
endif()
