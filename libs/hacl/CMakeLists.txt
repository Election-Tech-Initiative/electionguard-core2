# This target is a c++ abstraction on top of the hacl library functions

cmake_minimum_required(VERSION 3.14...3.16 FATAL_ERROR)

# --- Import tools ----
list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/../cmake")

# ---- Dependencies ----
include(../../cmake/CPM_0.31.0.cmake)

if(MSVC)
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
    GIT_TAG 04883d6ce3384ee5c8c31db907ca556ed8b45571
    OPTIONS "HACL_CUSTOM_CONFIG_FILE_PATH ${HACL_CUSTOM_CONFIG_FILE_PATH}"
)

# Variables
set(HACL_CPP_EXPORT_NAME "hacl")
set(HACL_CPP_ALIAS_NAME "hacl::hacl")
set(HACL_CPP_TARGET_NAME "hacl_cpp")

# if(WIN32)
message("++ Exporting all Symbols")
set(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS TRUE)

# endif()

# ---- Library Sources ----
include(${PROJECT_SOURCE_DIR}/libs/hacl/sources.cmake)

if(BUILD_SHARED_LIBS)
    add_library(${HACL_CPP_TARGET_NAME} SHARED ${SOURCES_hacl_cpp})
else()
    add_library(${HACL_CPP_TARGET_NAME} STATIC ${SOURCES_hacl_cpp})
endif()

add_library(${HACL_CPP_ALIAS_NAME} ALIAS ${HACL_CPP_TARGET_NAME})
target_compile_features(${HACL_CPP_TARGET_NAME} PUBLIC cxx_std_17)

# Dependencies
if(MSVC)
    target_include_directories(${HACL_CPP_TARGET_NAME}
        PRIVATE
        ${hacl_SOURCE_DIR}/include/msvc
        ${hacl_SOURCE_DIR}/build
        ${hacl_SOURCE_DIR}/karamel/include
        ${hacl_SOURCE_DIR}/karamel/krmllib/dist/minimal
        ${hacl_SOURCE_DIR}/vale/include
    )
else()
    target_include_directories(${HACL_CPP_TARGET_NAME}
        PRIVATE
        ${hacl_SOURCE_DIR}/include
        ${hacl_SOURCE_DIR}/build
        ${hacl_SOURCE_DIR}/karamel/include
        ${hacl_SOURCE_DIR}/karamel/krmllib/dist/minimal
        ${hacl_SOURCE_DIR}/vale/include
    )
endif()

if(BUILD_SHARED_LIBS)
    target_link_libraries(${HACL_CPP_TARGET_NAME} PRIVATE hacl)
else()
    target_link_libraries(${HACL_CPP_TARGET_NAME} PRIVATE hacl_static)
endif()

# Export
install(
    TARGETS ${HACL_CPP_TARGET_NAME}
    EXPORT ${HACL_CPP_TARGET_NAME}
)

export(
    EXPORT ${HACL_CPP_TARGET_NAME}
    FILE ${PROJECT_BINARY_DIR}/${HACL_CPP_TARGET_NAME}Config.cmake
)

install(
    EXPORT ${HACL_CPP_TARGET_NAME}
    FILE ${HACL_CPP_TARGET_NAME}Config.cmake
    DESTINATION lib/cmake/${HACL_CPP_TARGET_NAME}/${PROJECT_VERSION}
)

# Copy the hacl library to the same directory as the hacl_cpp library
if(MSVC)
    add_custom_target(
        copy_hacl_dll ALL
        COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${hacl_BINARY_DIR}/${CMAKE_BUILD_TYPE}
        $<TARGET_FILE_DIR:${HACL_CPP_TARGET_NAME}>
    )
    add_dependencies(copy_hacl_dll ${HACL_CPP_TARGET_NAME})
else()
    add_custom_target(
        copy_hacl_dll ALL
        COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${hacl_BINARY_DIR}
        $<TARGET_FILE_DIR:${HACL_CPP_TARGET_NAME}>
    )
    add_dependencies(copy_hacl_dll ${HACL_CPP_TARGET_NAME})
endif()
