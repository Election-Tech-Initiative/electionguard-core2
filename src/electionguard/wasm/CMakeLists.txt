# Set domain variables

set(WASM_TARGET "electionguard.wasm")

if(EMSCRIPTEN)
    include(${PROJECT_SOURCE_DIR}/src/electionguard/wasm/sources.cmake)

    # TODO: assing memory constraits var INITIAL_MEMORY = 16777216;
    # TODO: enable sanitizers https://emscripten.org/docs/debugging/Sanitizers.html#sanitizer-asan

    # Debug configuration
    if(CMAKE_BUILD_TYPE MATCHES Debug)
        message("++ Compiling WASM in RELEASE mode")
        set(EMSCRIPTEN_FLAGS
            "-O0"
            "-g3"
            "-s ASSERTIONS=2"
            "-s SAFE_HEAP=1"
            "-s STACK_OVERFLOW_CHECK=2"
            "-s ALLOW_MEMORY_GROWTH=1"
            "-s MODULARIZE=1"
            "-s DEMANGLE_SUPPORT=1"
            "-s EXPORT_NAME=Electionguard"

            # "-s SINGLE_FILE=1"
            "-s WASM_BIGINT=1"
            "-s ERROR_ON_UNDEFINED_SYMBOLS=0"
            "-s WARN_ON_UNDEFINED_SYMBOLS=1"
            "-s DISABLE_EXCEPTION_CATCHING=0"
            "-s STRICT=1"

            # "-s EXPORT_ES6=1"
            # "-s FILESYSTEM=0"
            "-pthread"
            "--no-entry"
            "-lembind"
        )

    else()
        # prod configuration
        message("++ Compiling WASM in RELEASE mode")
        set(EMSCRIPTEN_FLAGS
            "-Os"
            "-g0"
            "-s ALLOW_MEMORY_GROWTH=1"
            "-s MODULARIZE=1"
            "-s EXPORT_NAME=Electionguard"

            # "-s SINGLE_FILE=1"
            "-s WASM_BIGINT=1"
            "-s ERROR_ON_UNDEFINED_SYMBOLS=0"
            "-s WARN_ON_UNDEFINED_SYMBOLS=1"
            "-s STRICT=1"
            "-pthread"
            "--no-entry"
            "-lembind"
        )
    endif()

    set(FLAGS "")

    foreach(line IN LISTS EMSCRIPTEN_FLAGS)
        set(FLAGS "${FLAGS} ${line}")
    endforeach()

    # Build the WebAssembly module
    add_executable(${WASM_TARGET} ${SOURCES_electionguard_wasm})

    target_link_libraries(${WASM_TARGET} PUBLIC ${META_PROJECT_TARGET})

    set_target_properties(${WASM_TARGET} PROPERTIES LINK_FLAGS ${FLAGS})
    set_target_properties(${WASM_TARGET} PROPERTIES CXX_STANDARD 17 OUTPUT_NAME
        ${WASM_TARGET})
endif()