# This target is a c++ abstraction on top of the hacl library functions

cmake_minimum_required(VERSION 3.14...3.16 FATAL_ERROR)

# --- Import tools ----
list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/../cmake")

# ---- Dependencies ----
include(../../cmake/CPM.cmake)

if (MSVC)
    set(HACL_CUSTOM_CONFIG_FILE_PATH ${PROJECT_SOURCE_DIR}/libs/hacl/config.msvc.cmake)
else()
    set(HACL_CUSTOM_CONFIG_FILE_PATH ${PROJECT_SOURCE_DIR}/libs/hacl/config.cmake)
endif()

# Fetch the hacl dependency but enforce an explicit GIT_TAG hash
# note, since the hacl migration is still under heavy development
# we pull from an explicit commit instead of a release for now
CPMAddPackage(
    NAME hacl
    GITHUB_REPOSITORY addressxception/hacl-packages
    VERSION 0.6.0
    GIT_TAG 4a465b5fdc2b18ffbca493a0abad40eec015da45
    OPTIONS "HACL_CUSTOM_CONFIG_FILE_PATH ${HACL_CUSTOM_CONFIG_FILE_PATH}"
)

# Variables
set(HACL_CPP_EXPORT_NAME "hacl")
set(HACL_CPP_TARGET_NAME "hacl_cpp")

if(MSVC)
    message("++ Exporting all Symbols")
    set(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS TRUE)
endif()

# ---- Library Sources ----
include(${PROJECT_SOURCE_DIR}/libs/hacl/sources.cmake)

add_library(${HACL_CPP_TARGET_NAME} SHARED ${SOURCES_hacl_cpp})
add_library(${HACL_CPP_EXPORT_NAME}::${HACL_CPP_EXPORT_NAME} ALIAS ${HACL_CPP_TARGET_NAME})

target_compile_features(${HACL_CPP_TARGET_NAME} PRIVATE cxx_std_17)

target_include_directories(${HACL_CPP_TARGET_NAME} PUBLIC ${PROJECT_SOURCE_DIR}/libs/hacl)

# Dependencies


if (MSVC)
    target_include_directories(${HACL_CPP_TARGET_NAME} PRIVATE
        ${hacl_SOURCE_DIR}/include/msvc
        ${hacl_SOURCE_DIR}/build
        ${hacl_SOURCE_DIR}/karamel/include
        ${hacl_SOURCE_DIR}/karamel/krmllib/dist/minimal
        ${hacl_SOURCE_DIR}/vale/include
)
else()
    target_include_directories(${HACL_CPP_TARGET_NAME} PRIVATE
        ${hacl_SOURCE_DIR}/include
        ${hacl_SOURCE_DIR}/build
        ${hacl_SOURCE_DIR}/karamel/include
        ${hacl_SOURCE_DIR}/karamel/krmllib/dist/minimal
        ${hacl_SOURCE_DIR}/vale/include
)
endif()

target_link_libraries(${HACL_CPP_TARGET_NAME} PRIVATE hacl)
