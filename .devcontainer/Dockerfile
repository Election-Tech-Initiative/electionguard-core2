FROM mcr.microsoft.com/devcontainers/cpp:ubuntu22.04

ARG REINSTALL_CMAKE_VERSION_FROM_SOURCE="none"

# Optionally install the cmake for vcpkg
COPY ./reinstall-cmake.sh /tmp/

ENV PROCESSOR=x64

RUN 
RUN if [ "${REINSTALL_CMAKE_VERSION_FROM_SOURCE}" != "none" ]; then \
    chmod +x /tmp/reinstall-cmake.sh && /tmp/reinstall-cmake.sh ${REINSTALL_CMAKE_VERSION_FROM_SOURCE}; \
    fi \
    && rm -f /tmp/reinstall-cmake.sh

RUN \
    apt-get update && \
    DEBIAN_FRONTEND=noninteractive && \
    apt-get install \
    build-essential iwyu llvm clang-14 cmake lcov cppcheck clang-format clang-tidy \
    ninja-build valgrind unzip dotnet-sdk-10.0 wget \
    -y
RUN mkdir cmake && wget -O cmake/CPM.cmake https://github.com/cpm-cmake/CPM.cmake/releases/download/v0.38.2/CPM.cmake
RUN	dotnet tool restore

# [Optional] Uncomment this section to install additional vcpkg ports.
# RUN su vscode -c "${VCPKG_ROOT}/vcpkg install <your-port-name-here>"