# Set domain variables

set(WASM_TARGET "electionguard.wasm")

if(EMSCRIPTEN)
    include(${PROJECT_SOURCE_DIR}/src/electionguard/wasm/sources.cmake)

    # Debug configuration
    set(EMSCRIPTEN_FLAGS
        "-O0 -s ALLOW_MEMORY_GROWTH=1 -s DEMANGLE_SUPPORT=1 -s SINGLE_FILE=1 -s WASM_BIGINT=1 -s ERROR_ON_UNDEFINED_SYMBOLS=0 -s WARN_ON_UNDEFINED_SYMBOLS=1 -s DISABLE_EXCEPTION_CATCHING=0 -lembind"
    )

    # set(EMSCRIPTEN_FLAGS
    # "-s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s MODULARIZE=1 -s SINGLE_FILE=1 -s -lembind --ts"
    # )

    # set(EMSCRIPTEN_FLAGS
    # "-s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s EXPORT_ES6=1 -s MODULARIZE=1 -s SINGLE_FILE=1 -s -lembind --ts"
    # )

    # set(EMSCRIPTEN_FLAGS "-lembind -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s MODULARIZE=1 -s SINGLE_FILE=1 -s INVOKE_RUN=0")

    # Build the WebAssembly module
    add_executable(${WASM_TARGET} ${SOURCES_electionguard_wasm})

    # target_link_libraries(${WASM_TARGET} PRIVATE hacl_static hacl::hacl)
    target_link_libraries(${WASM_TARGET} PUBLIC ${META_PROJECT_TARGET})

    set_target_properties(${WASM_TARGET} PROPERTIES LINK_FLAGS ${EMSCRIPTEN_FLAGS})
    set_target_properties(${WASM_TARGET} PROPERTIES CXX_STANDARD 17 OUTPUT_NAME
        ${WASM_TARGET})
endif()
