# Toolchain file compiling for aarch64 MacCatalyst

set(arch arm64)

# For some reason we have to set the system name here in order to make the
# CMAKE_SYSTEM_PROCESSOR being picked up correctly.
set(CMAKE_SYSTEM_NAME Darwin)
set(CMAKE_SYSTEM_PROCESSOR ${arch})

# Enable cross-compilation
if(NOT ${CMAKE_HOST_SYSTEM_PROCESSOR} MATCHES "aarch64|arm64|arm64v8")
    set(CMAKE_SYSTEM_VERSION "${CMAKE_HOST_SYSTEM_VERSION}")
endif()

# Set the SDK and deployment target
set(CMAKE_OSX_SYSROOT macosx) # Use the macOS SDK
set(CMAKE_OSX_DEPLOYMENT_TARGET 13.1) # Set the minimum deployment target for Mac Catalyst

set(triple ${arch}-apple-ios${CMAKE_OSX_DEPLOYMENT_TARGET}-macabi)

set(CMAKE_C_COMPILER clang)
set(CMAKE_C_COMPILER_TARGET ${triple})
set(CMAKE_CXX_COMPILER clang++)
set(CMAKE_CXX_COMPILER_TARGET ${triple})

# Disable bitcode
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fembed-bitcode-marker")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fembed-bitcode-marker")

# Set the platform
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -isysroot ${CMAKE_OSX_SYSROOT}")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -isysroot ${CMAKE_OSX_SYSROOT}")

# set the target for hacl
set(HACL_TARGET_OS osx)
