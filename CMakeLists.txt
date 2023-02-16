cmake_minimum_required(VERSION 3.14...3.16 FATAL_ERROR)

# ---- Project ----
set(META_PROJECT_NAME "ElectionGuard")
set(META_PROJECT_EXPORT "ElectionGuard")
set(META_PROJECT_TARGET "electionguard")
set(META_VERSION_MAJOR "2")
set(META_VERSION_MINOR "0")
set(META_VERSION_PATCH "0")
set(META_VERSION "${META_VERSION_MAJOR}.${META_VERSION_MINOR}.${META_VERSION_PATCH}")

set(LIBRARY_PUBLIC_INCLUDE_DIR ${PROJECT_SOURCE_DIR}/include)

project(${META_PROJECT_NAME}
    VERSION ${META_VERSION}
    LANGUAGES C CXX
)

# Create a version file
file(WRITE "${PROJECT_BINARY_DIR}/VERSION" "${META_PROJECT_NAME} v${META_VERSION}")

# ---- Execution guards ----
if(PROJECT_SOURCE_DIR STREQUAL PROJECT_BINARY_DIR)
    message(FATAL_ERROR "In-source builds not allowed. Please make a new /build/ directory and run CMake from there.")
endif()

# ---- Options ----
include(cmake/options.cmake)

# ---- Modules ----
list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake")

# ---- Dependencies ----
set(CPM_DOWNLOAD_VERSION 0.31.0)
set(CPM_DOWNLOAD_LOCATION "${CMAKE_CURRENT_SOURCE_DIR}/cmake/CPM_${CPM_DOWNLOAD_VERSION}.cmake")

if(NOT(EXISTS ${CPM_DOWNLOAD_LOCATION}))
    message(STATUS "Downloading CPM.cmake")
    file(DOWNLOAD https://github.com/cpm-cmake/CPM.cmake/releases/download/v${CPM_DOWNLOAD_VERSION}/CPM.cmake ${CPM_DOWNLOAD_LOCATION})
endif()

include(${CPM_DOWNLOAD_LOCATION})

# ---- Tools ----
include(cmake/tools.cmake)

# --- Properties ---
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

set(CMAKE_THREAD_PREFER_PTHREAD TRUE)
set(THREADS_PREFER_PTHREAD_FLAG TRUE)

set(CMAKE_POSITION_INDEPENDENT_CODE ON)

# IDE's usually prefer folders
set_property(GLOBAL PROPERTY USE_FOLDERS ON)
set(IDE_FOLDER "")

# ---- Sources ----
add_subdirectory(libs/hacl)
add_subdirectory(src)

if(OPTION_ENABLE_TESTS)
    # TODO: move mocks to their own folder
    add_subdirectory(test)
endif()

if(OPTION_GENERATE_DOCS)
    add_subdirectory(docs)
endif()
