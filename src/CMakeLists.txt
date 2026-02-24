cmake_minimum_required(VERSION 3.14...3.16 FATAL_ERROR)

# --- Import tools ----
list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/../cmake")

# ---- Dependencies ----
include(../cmake/CPM_0.31.0.cmake)

CPMAddPackage(
    NAME date
    GITHUB_REPOSITORY HowardHinnant/date
    VERSION 3.0.1
)

CPMAddPackage(
    NAME nlohmann_json
    VERSION 3.11.2
    URL https://github.com/nlohmann/json/releases/download/v3.11.2/include.zip
    URL_HASH SHA256=e5c7a9f49a16814be27e4ed0ee900ecd0092bfb7dbfca65b5a421b774dccaaed
)

CPMAddPackage(
    NAME spdlog
    GITHUB_REPOSITORY gabime/spdlog
    VERSION 1.9.2
    OPTIONS "SPDLOG_INSTALL ON"
)

find_package(Threads REQUIRED)

# ---- Library Sources ----
include(${PROJECT_SOURCE_DIR}/src/electionguard/sources.cmake)

set(PROJECT_SOURCE_FILES
    ${FACADES_electionguard}
    ${SOURCES_electionguard}
    ${INCLUDES_electionguard_hpp}
    ${INCLUDES_electionguard_h}
)

add_library(${META_PROJECT_TARGET} ${PROJECT_SOURCE_FILES})
add_library(${META_PROJECT_TARGET}::${META_PROJECT_TARGET} ALIAS ${META_PROJECT_TARGET})

# ---- Compile ----

# Set the target compile features rather than setting CMAKE_CXX_STANDARD
# so we don't pollute other targets that may compile differently
target_compile_features(${META_PROJECT_TARGET} PRIVATE cxx_std_17)
target_compile_options(${META_PROJECT_TARGET} PUBLIC "$<$<BOOL:${MSVC}>:/permissive->")

# define dll export markers
if(BUILD_SHARED_LIBS AND NOT EMSCRIPTEN)
    message("++ Building Shared Libs")
    add_compile_definitions(ELECTIONGUARD_BUILD_SHARED)
else()
    message("++ Building Static Libs")
    add_compile_definitions(ELECTIONGUARD_BUILD_STATIC)
endif()

if(WIN32 AND CMAKE_SYSTEM_PROCESSOR MATCHES "x86" AND NOT USE_32BIT_MATH)
    message("++ Warning buidling for WIN32 with 64-bit math. Consider setting USE_32BIT_MATH for optimized performance")
endif()

if(MINGW)
    message("++ Building with MINGW")

    # Remove lib prefix
    set_target_properties(${META_PROJECT_TARGET} PROPERTIES PREFIX "")
endif()

if(MSVC)
    message("++ Building with MSVC")
    target_compile_options(${META_PROJECT_TARGET} PUBLIC /wd4067)
endif()

if(IOS AND CMAKE_IOS_INSTALL_COMBINED)
    # Tell XCode how to merge the fat binary
    # see: https://github.com/leetal/ios-cmake/issues/12#issuecomment-421425541
    set_target_properties(${META_PROJECT_TARGET} PROPERTIES
        XCODE_ATTRIBUTE_ARCHS[sdk=iphonesimulator*] x86_64
        XCODE_ATTRIBUTE_VALID_ARCHS[sdk=iphonesimulator*] x86_64
    )
endif()

# Set the public include directory depending on
# if the target is being exported or installed
target_include_directories(${META_PROJECT_TARGET}
    SYSTEM PUBLIC
    $<INSTALL_INTERFACE:include/${META_PROJECT_EXPORT}/${PROJECT_VERSION}>

    # Headers used from /build
    $<BUILD_INTERFACE:${PROJECT_SOURCE_DIR}/include>
    PRIVATE
    ${PROJECT_SOURCE_DIR}/src/${META_PROJECT_TARGET}
)

# ---- Dependencies ----

# Include the karamel sources from hacl directly in the library
# since some of the headers are still referenced directly
# the rest of hacl is loaded as part of the libs/hacl cpp module
target_include_directories(${META_PROJECT_TARGET}
    PRIVATE
    ${hacl_SOURCE_DIR}/karamel/include
    ${hacl_SOURCE_DIR}/karamel/krmllib/dist/minimal
)

add_library(nlohmann_json INTERFACE IMPORTED)
target_include_directories(nlohmann_json INTERFACE ${nlohmann_json_SOURCE_DIR}/include)

if(MINGW)
    target_link_libraries(${META_PROJECT_TARGET}
        PRIVATE
        date::date
        hacl::hacl
        nlohmann_json
        spdlog::spdlog_header_only
        Threads::Threads)

    # Statically link in libstdc++
    set(CMAKE_EXE_LINKER_FLAGS " -static")
    target_link_libraries(${META_PROJECT_TARGET} PRIVATE -static-libstdc++)
else()
    target_link_libraries(${META_PROJECT_TARGET}
        PRIVATE
        date::date
        hacl::hacl
        nlohmann_json
        spdlog::spdlog_header_only
        Threads::Threads)
endif()

# ---- Install ----

# Install the electionguard library in the default location,
# and associate electionguard with the ElectionGuard export
install(
    TARGETS ${META_PROJECT_TARGET}
    EXPORT ${META_PROJECT_EXPORT}
)

# Install public header files
install(
    DIRECTORY ${PROJECT_SOURCE_DIR}/include/${META_PROJECT_EXPORT}/${PROJECT_VERSION}
    TYPE INCLUDE
    FILES_MATCHING PATTERN "*.h*"
)

# export the spdlog config
export(EXPORT spdlog FILE ${project_config_out})

# Generate the build-tree ElectionGuardConfig.cmake for use
# in other cmake projects without needing to install
export(
    EXPORT ${META_PROJECT_EXPORT}
    FILE ${PROJECT_BINARY_DIR}/${META_PROJECT_EXPORT}Config.cmake
)

# Generate the install-tree ElectionGuardConfig.cmake for use
# in other cmake projects after this library has been installed
install(
    EXPORT ${META_PROJECT_EXPORT}
    FILE ${META_PROJECT_EXPORT}Config.cmake
    DESTINATION lib/cmake/${META_PROJECT_EXPORT}/${PROJECT_VERSION}
)

# Compiler configuration
if(EMSCRIPTEN)
    message("++ Configuring compile options for Emscripten")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -pthread")
    target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-unqualified-std-cast-call)
endif()

if(MSVC)
    message("++ Configuring compile options for MSVC")
endif()

if(CMAKE_CXX_COMPILER_ID MATCHES "Clang|GNU")
    message("++ Configuring compile options for Clang|GNU")
    target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-unqualified-std-cast-call)
endif()

if(CMAKE_CXX_COMPILER_ID MATCHES "Clang")
    message("++ Configuring compile options for Clang")

elseif(CMAKE_CXX_COMPILER_ID MATCHES "GNU")
    message("++ Configuring compile options for GNU")
else()
    message("++ Configuring compile options for unknown compiler" CMAKE_CXX_COMPILER_ID)
endif()

# ---- Test ----
if(OPTION_ENABLE_TESTS)
    message("++ Building for tests")

    # Expose the internal headers to tests
    target_include_directories(${META_PROJECT_TARGET}
        SYSTEM PUBLIC
        $<BUILD_INTERFACE:${PROJECT_SOURCE_DIR}/src>
    )

    # When running tests, set some specific compiler flags
    # which are different from compiling in release mode
    if(MSVC)
        message("++ Configuring TEST compile options for MSVC")

        # TODO: renenable warnings as errors when the correct ones are defined
        # target_compile_options(${META_PROJECT_TARGET} PUBLIC /W4 /WX)C5059
        target_compile_options(${META_PROJECT_TARGET} PUBLIC /Zi) # Generates complete debugging information.
    endif()

    if(CMAKE_CXX_COMPILER_ID MATCHES "Clang|GNU")
        message("++ Configuring TEST compile options for Clang|GNU")

        # TODO: look at other options in WMost and add what makes sense
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wmisleading-indentation)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wparentheses)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wswitch)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wswitch-bool)

        # TODO: target_compile_options(${META_PROJECT_TARGET} PUBLIC -Werror)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wextra)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -pedantic)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -pedantic-errors)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wmissing-field-initializers)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-deprecated-declarations)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-unused-variable)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-redundant-decls)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-unknown-pragmas)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-reserved-macro-identifier)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-reserved-identifier)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-header-hygiene)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-unused-parameter)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-newline-eof)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-static-in-inline)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-vla-extension)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-global-constructors)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-padded)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-suggest-override)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-weak-vtables)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-used-but-marked-unused)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-exit-time-destructors)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-unsafe-buffer-usage)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-old-style-cast)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-sign-conversion)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-error)
    endif()

    if(CMAKE_CXX_COMPILER_ID MATCHES "Clang")
        message("++ Configuring TEST compile options for Clang")
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-static-in-inline)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-vla-extension)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wpacked-non-pod)
    elseif(CMAKE_CXX_COMPILER_ID MATCHES "GNU")
        message("++ Configuring TEST compile options for GNU")
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-effc++)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-old-style-cast)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-unused-but-set-parameter)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-unused-but-set-variable)
        target_compile_options(${META_PROJECT_TARGET} PUBLIC -Wno-useless-cast)
    endif()
endif()

if(CODE_COVERAGE)
    message("++ Building with coverage")
    target_code_coverage(${META_PROJECT_TARGET} AUTO)
endif()

#if(USE_FORMATTING)
#    message("++ Building with formatting")
#    clang_format(format ${PROJECT_SOURCE_FILES})
#endif()
