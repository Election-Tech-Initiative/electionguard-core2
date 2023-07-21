# sources for tests

set(SOURCES_electionguard_test_utils
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/generators/ballot.h
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/generators/ballot.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/generators/ballot.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/generators/election.h
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/generators/election.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/generators/election.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/generators/manifest.h
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/generators/manifest.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/generators/manifest.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/utils/byte_logger.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/utils/constants.hpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/utils/utils.h
)

set(SOURCES_electionguard_test_benchmark
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/benchmark/main.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/benchmark/bench_chaum_pedersen.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/benchmark/bench_elgamal.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/benchmark/bench_encrypt.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/benchmark/bench_group.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/benchmark/bench_hacl.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/benchmark/bench_hash.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/benchmark/bench_hashed_elgamal.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/benchmark/bench_nonces.cpp

    # ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/benchmark/bench_precompute.cpp
)

set(SOURCES_electionguard_test_cpp_tests
    ${CMAKE_CURRENT_SOURCE_DIR}/main.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_ballot_code.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_ballot_compact.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_ballot.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_chaum_pedersen.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_constants.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_discrete_log.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_election.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_elgamal.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_encrypt.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_encrypt_compact.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_group.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_hacl.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_hash.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_nonces.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_manifest.cpp
)

set(SOURCES_electionguard_test_c_tests
    ${CMAKE_CURRENT_SOURCE_DIR}/main.c
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_ballot_code.c
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_ballot.c
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_chaum_pedersen.c
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_collections.c
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_election.c
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_elgamal.c
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_encrypt_compact.c
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_encrypt.c
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_group.c
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_hash.c
    ${CMAKE_CURRENT_SOURCE_DIR}/electionguard/test_manifest.c
)
