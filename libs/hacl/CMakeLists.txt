# This target is a c++ abstraction on top of the hacl library functions

cmake_minimum_required(VERSION 3.14...3.16 FATAL_ERROR)

# --- Import tools ----
list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/../cmake")

# ---- Dependencies ----
include(../../cmake/CPM.cmake)

# Fetch the hacl dependency but enforce an explicit GIT_TAG hash
# note, since the hacl migration is still under heavy development
# we pull from an explicit commit instead of a release for now
CPMAddPackage(
    NAME hacl
    GITHUB_REPOSITORY cryspen/hacl-packages
    VERSION 0.6.0
    GIT_TAG 5cb0015ef6e7c025cb6a6174af2458cf44ff32bc
    OPTIONS
    ""
)

# Variables
set(HACL_CPP_EXPORT_NAME "hacl")
set(HACL_CPP_TARGET_NAME "hacl_cpp")

# ---- Library Sources ----
include(${PROJECT_SOURCE_DIR}/libs/hacl/sources.cmake)

add_library(${HACL_CPP_TARGET_NAME} SHARED ${SOURCES_hacl_cpp})
add_library(${HACL_CPP_EXPORT_NAME}::${HACL_CPP_EXPORT_NAME} ALIAS ${HACL_CPP_TARGET_NAME})

target_compile_features(${HACL_CPP_TARGET_NAME} PRIVATE cxx_std_17)

target_include_directories(${HACL_CPP_TARGET_NAME} PUBLIC ${PROJECT_SOURCE_DIR}/libs/hacl)

# Dependencies
target_include_directories(${HACL_CPP_TARGET_NAME} PRIVATE
    ${hacl_SOURCE_DIR}/include
    ${hacl_SOURCE_DIR}/build
    ${hacl_SOURCE_DIR}/karamel/include
    ${hacl_SOURCE_DIR}/karamel/krmllib/dist/minimal
    ${hacl_SOURCE_DIR}/vale/include
)

target_link_libraries(${HACL_CPP_TARGET_NAME} PRIVATE hacl)
