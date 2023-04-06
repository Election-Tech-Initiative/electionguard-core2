.PHONY: all build build-msys2 build-android build-ios build-netstandard build-ui clean clean-netstandard clean-ui environment format memcheck sanitize sanitize-asan sanitize-tsan bench bench-netstandard test test-msys2 test-netstandard test-netstandard-copy-output

.EXPORT_ALL_VARIABLES:
ELECTIONGUARD_BINDING_DIR=$(realpath .)/bindings
ELECTIONGUARD_BINDING_LIB_DIR=$(ELECTIONGUARD_BINDING_DIR)/netstandard/ElectionGuard/ElectionGuard.Encryption
ELECTIONGUARD_BINDING_BENCH_DIR=$(ELECTIONGUARD_BINDING_DIR)/netstandard/ElectionGuard/ElectionGuard.Encryption.Bench
ELECTIONGUARD_BINDING_CLI_DIR=$(ELECTIONGUARD_BINDING_DIR)/netstandard/ElectionGuard/ElectionGuard.Encryption.Cli
ELECTIONGUARD_BINDING_TEST_DIR=$(ELECTIONGUARD_BINDING_DIR)/netstandard/ElectionGuard/ElectionGuard.Encryption.Tests
ELECTIONGUARD_BINDING_UTILS_DIR=$(ELECTIONGUARD_BINDING_DIR)/netstandard/ElectionGuard/ElectionGuard.Encryption.Utils
ELECTIONGUARD_BUILD_DIR=$(subst \,/,$(realpath .))/build
ELECTIONGUARD_BUILD_DIR_WIN=$(subst \c\,C:\,$(subst /,\,$(ELECTIONGUARD_BUILD_DIR)))
ELECTIONGUARD_BUILD_APPS_DIR=$(ELECTIONGUARD_BUILD_DIR)/apps
ELECTIONGUARD_BUILD_BINDING_DIR=$(ELECTIONGUARD_BUILD_DIR)/bindings
ELECTIONGUARD_BUILD_LIBS_DIR=$(ELECTIONGUARD_BUILD_DIR)/libs
CPM_SOURCE_CACHE=$(subst \,/,$(realpath .))/.cache/CPM

# Detect operating system & platform
# These vars can be set from the command line.
# not all platforms can compile all targets.
# valid values:
# OPERATING_SYSTEM: Android, Ios, Linux, Darwin, Windows
# PROCESSOR: arm64, x64, x86
ifeq ($(OS),Windows_NT)
	OPERATING_SYSTEM ?= Windows
	ifeq ($(PROCESSOR_ARCHITECTURE),arm)
		HOST_PROCESSOR:=arm64
        PROCESSOR?=arm64
    endif
	# https://learn.microsoft.com/en-us/windows/win32/winprog64/wow64-implementation-details
	ifeq ($(PROCESSOR_ARCHITEW6432),AMD64)
		HOST_PROCESSOR:=x64
		PROCESSOR?=x64
	endif
	ifeq ($(PROCESSOR_ARCHITECTURE),AMD64)
		HOST_PROCESSOR:=x64
        PROCESSOR?=x64
    endif
    ifeq ($(PROCESSOR_ARCHITECTURE),x86)
		HOST_PROCESSOR:=x86
        PROCESSOR?=x86
    endif
else
	OPERATING_SYSTEM ?= $(shell uname 2>/dev/null || echo Unknown)
	UNAME_M ?= $(shell uname -m)
    ifeq ($(UNAME_M),x86_64)
		HOST_PROCESSOR:=x64
        PROCESSOR?=x64
    endif
    ifneq ($(filter %86,$(UNAME_M)),)
		HOST_PROCESSOR:=x86
        PROCESSOR?=x86
    endif
    ifneq ($(filter arm%,$(UNAME_M)),)
		HOST_PROCESSOR:=arm64
        PROCESSOR?=arm64
    endif
endif

# Default build number
BUILD:=1

# handle setting processor-specific build vars
ifeq ($(PROCESSOR),x86)
# Temporarily disable vale support on x86
TEMP_DISABLE_VALE?=ON
USE_32BIT_MATH?=ON
VSPLATFORM?=Win32
else
TEMP_DISABLE_VALE?=ON
USE_32BIT_MATH?=OFF
VSPLATFORM?=x64
endif

# Debug or Release (capitalized)
TARGET?=Release

# Set the Android NDK for cross-compilation
ifeq ($(OPERATING_SYSTEM),Darwin)
ANDROID_NDK_PATH?=/Users/$(USER)/Library/Android/sdk/ndk/25.1.8937393
DOTNET_PATH?=/usr/local/share/dotnet
endif
ifeq ($(OPERATING_SYSTEM),Linux)
ANDROID_NDK_PATH?=/usr/local/lib/android/sdk/ndk/25.1.8937393
DOTNET_PATH?=/usr/share/dotnet
endif
ifeq ($(OPERATING_SYSTEM),Windows)
ANDROID_NDK_PATH?=C:\Android\android-sdk\ndk-bundle
DOTNET_PATH?=C:\Program Files\dotnet
endif

all: environment build

# Configure Environment

environment:
ifeq ($(OPERATING_SYSTEM),Darwin)
	@echo 🍎 MACOS INSTALL
	brew install wget
	brew install cmake
	brew install cppcheck
	brew install clang-format
	brew install include-what-you-use
	brew install llvm
	brew install ninja
	test -f /usr/local/bin/clang-tidy || sudo ln -s "$(shell brew --prefix llvm)/bin/clang-tidy" "/usr/local/bin/clang-tidy"
endif
ifeq ($(OPERATING_SYSTEM),Linux)
	@echo 🐧 LINUX INSTALL
	sudo apt install -y build-essential
	sudo apt install -y iwyu
	sudo apt install -y llvm
	sudo apt install -y clang-12
	sudo apt install -y cmake
	sudo apt install -y lcov
	sudo apt install -y cppcheck
	sudo apt install -y clang-format
	sudo apt install -y clang-tidy
	sudo apt install -y ninja-build
	sudo apt install -y valgrind
endif
ifeq ($(OPERATING_SYSTEM),Windows)
	@echo 🏁 WINDOWS INSTALL
	choco upgrade wget -y
	choco upgrade unzip -y
	choco upgrade cmake -y
	choco upgrade ninja -y
	choco upgrade vswhere -y
endif
	wget -O cmake/CPM.cmake https://github.com/cpm-cmake/CPM.cmake/releases/download/v0.35.5/CPM.cmake
	make fetch-sample-data
	dotnet tool restore

environment-ui: environment
ifeq ($(OPERATING_SYSTEM),Windows)
	dotnet workload install maui
	dotnet workload restore ./src/electionguard-ui/ElectionGuard.UI/ElectionGuard.UI.csproj && dotnet restore ./src/electionguard-ui/ElectionGuard.UI.sln
else
	sudo dotnet workload install maui
	sudo dotnet workload restore ./src/electionguard-ui/ElectionGuard.UI/ElectionGuard.UI.csproj && dotnet restore ./src/electionguard-ui/ElectionGuard.UI.sln
endif

# Builds

build:
	@echo 🧱 BUILD $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
ifeq ($(OPERATING_SYSTEM),Windows)
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET) \
		-G "Visual Studio 17 2022" -A $(VSPLATFORM) \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DBUILD_SHARED_LIBS=ON \
		-DDISABLE_VALE=$(TEMP_DISABLE_VALE) \
		-DUSE_MSVC=ON \
		-DANDROID_NDK_PATH=$(ANDROID_NDK_PATH) \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/$(PROCESSOR)-$(OPERATING_SYSTEM).cmake
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/ --config $(TARGET)
else
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET) \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DBUILD_SHARED_LIBS=ON \
		-DDISABLE_VALE=$(TEMP_DISABLE_VALE) \
		-DANDROID_NDK_PATH=$(ANDROID_NDK_PATH) \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/$(PROCESSOR)-$(OPERATING_SYSTEM).cmake
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)
endif

build-arm64:
	PROCESSOR=arm64 && make build

build-x86:
	PROCESSOR=x86 VSPLATFORM=Win32 && make build

build-x64:
	PROCESSOR=x64 && make build
	
build-msys2:
	@echo 🖥️ BUILD MSYS2 $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
ifeq ($(OPERATING_SYSTEM),Windows)
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET) -G "MSYS Makefiles" \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DBUILD_SHARED_LIBS=ON \
		-DCAN_USE_VECTOR_INTRINSICS=ON \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/$(PROCESSOR)-$(OPERATING_SYSTEM)-msys2.cmake
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)
else
	echo "MSYS2 builds are only supported on Windows"
endif

build-android:
	PROCESSOR=arm64 OPERATING_SYSTEM=Android && make build

build-ios:
	@echo 📱 BUILD IOS
ifeq ($(OPERATING_SYSTEM),Darwin)
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/ios/$(TARGET) \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_SYSTEM_NAME=iOS \
		"-DCMAKE_OSX_ARCHITECTURES=arm64;arm64e;x86_64" \
		-DCMAKE_OSX_DEPLOYMENT_TARGET=12.4 \
		-DCMAKE_INSTALL_PREFIX=$(ELECTIONGUARD_BUILD_LIBS_DIR)/ios/$(TARGET) \
		-DCMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH=NO \
		-DCMAKE_IOS_INSTALL_COMBINED=YES
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/ios/$(TARGET) --config $(TARGET) --target install
else
	echo "iOS builds are only supported on MacOS"
endif

build-netstandard:
	@echo 🖥️ BUILD NETSTANDARD $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
	make build
	cd ./bindings/netstandard/ElectionGuard && dotnet restore
	dotnet build --configuration $(TARGET) ./bindings/netstandard/ElectionGuard/ElectionGuard.sln /p:Platform=$(PROCESSOR)
	dotnet build -a $(PROCESSOR) --configuration $(TARGET) ./bindings/netstandard/ElectionGuard/ElectionGuard.ElectionSetup.Tests/ElectionGuard.ElectionSetup.Tests.csproj

build-netstandard-x64:
	PROCESSOR=x64 && make build-netstandard

build-ui: build-netstandard
	@echo 🖥️ BUILD UI $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
	cd ./src/electionguard-ui && dotnet restore
	dotnet build --configuration $(TARGET) ./src/electionguard-ui/ElectionGuard.UI.sln /p:Platform=$(PROCESSOR)


# Clean

clean:
	@echo 🗑️ Cleaning Output Directory
ifeq ($(OPERATING_SYSTEM),Windows)
	pwsh -Command "rm -R -Fo $(ELECTIONGUARD_BUILD_DIR_WIN); $$null"
else
	if [ -d "$(ELECTIONGUARD_BUILD_DIR)" ]; then rm -rf $(ELECTIONGUARD_BUILD_DIR)/*; fi
	if [ ! -d "$(ELECTIONGUARD_BUILD_DIR)" ]; then mkdir $(ELECTIONGUARD_BUILD_DIR); fi

	if [ -d "$(ELECTIONGUARD_BINDING_LIB_DIR)/bin" ]; then rm -rf $(ELECTIONGUARD_BINDING_LIB_DIR)/bin/*; fi
	if [ ! -d "$(ELECTIONGUARD_BINDING_LIB_DIR)/bin" ]; then mkdir $(ELECTIONGUARD_BINDING_LIB_DIR)/bin; fi
	if [ -d "$(ELECTIONGUARD_BINDING_LIB_DIR)/obj" ]; then rm -rf $(ELECTIONGUARD_BINDING_LIB_DIR)/obj/*; fi
	if [ ! -d "$(ELECTIONGUARD_BINDING_LIB_DIR)/obj" ]; then mkdir $(ELECTIONGUARD_BINDING_LIB_DIR)/obj; fi

	if [ -d "$(ELECTIONGUARD_BINDING_BENCH_DIR)/bin" ]; then rm -rf $(ELECTIONGUARD_BINDING_BENCH_DIR)/bin/*; fi
	if [ ! -d "$(ELECTIONGUARD_BINDING_BENCH_DIR)/bin" ]; then mkdir $(ELECTIONGUARD_BINDING_BENCH_DIR)/bin; fi
	if [ -d "$(ELECTIONGUARD_BINDING_BENCH_DIR)/obj" ]; then rm -rf $(ELECTIONGUARD_BINDING_BENCH_DIR)/obj/*; fi
	if [ ! -d "$(ELECTIONGUARD_BINDING_BENCH_DIR)/obj" ]; then mkdir $(ELECTIONGUARD_BINDING_BENCH_DIR)/obj; fi

	if [ -d "$(ELECTIONGUARD_BINDING_CLI_DIR)/bin" ]; then rm -rf $(ELECTIONGUARD_BINDING_CLI_DIR)/bin/*; fi
	if [ ! -d "$(ELECTIONGUARD_BINDING_CLI_DIR)/bin" ]; then mkdir $(ELECTIONGUARD_BINDING_CLI_DIR)/bin; fi
	if [ -d "$(ELECTIONGUARD_BINDING_CLI_DIR)/obj" ]; then rm -rf $(ELECTIONGUARD_BINDING_CLI_DIR)/obj/*; fi
	if [ ! -d "$(ELECTIONGUARD_BINDING_CLI_DIR)/obj" ]; then mkdir $(ELECTIONGUARD_BINDING_CLI_DIR)/obj; fi

	if [ -d "$(ELECTIONGUARD_BINDING_TEST_DIR)/bin" ]; then rm -rf $(ELECTIONGUARD_BINDING_TEST_DIR)/bin/*; fi
	if [ ! -d "$(ELECTIONGUARD_BINDING_TEST_DIR)/bin" ]; then mkdir $(ELECTIONGUARD_BINDING_TEST_DIR)/bin; fi
	if [ -d "$(ELECTIONGUARD_BINDING_TEST_DIR)/obj" ]; then rm -rf $(ELECTIONGUARD_BINDING_TEST_DIR)/obj/*; fi
	if [ ! -d "$(ELECTIONGUARD_BINDING_TEST_DIR)/obj" ]; then mkdir $(ELECTIONGUARD_BINDING_TEST_DIR)/obj; fi

	if [ -d "$(ELECTIONGUARD_BINDING_UTILS_DIR)/bin" ]; then rm -rf $(ELECTIONGUARD_BINDING_UTILS_DIR)/bin/*; fi
	if [ ! -d "$(ELECTIONGUARD_BINDING_UTILS_DIR)/bin" ]; then mkdir $(ELECTIONGUARD_BINDING_UTILS_DIR)/bin; fi
	if [ -d "$(ELECTIONGUARD_BINDING_UTILS_DIR)/obj" ]; then rm -rf $(ELECTIONGUARD_BINDING_UTILS_DIR)/obj/*; fi
	if [ ! -d "$(ELECTIONGUARD_BINDING_UTILS_DIR)/obj" ]; then mkdir $(ELECTIONGUARD_BINDING_UTILS_DIR)/obj; fi

	if [ ! -d "$(ELECTIONGUARD_BUILD_LIBS_DIR)" ]; then mkdir $(ELECTIONGUARD_BUILD_LIBS_DIR); fi

	if [ ! -d "$(ELECTIONGUARD_BUILD_LIBS_DIR)/Android" ]; then mkdir $(ELECTIONGUARD_BUILD_LIBS_DIR)/Android; fi
	if [ ! -d "$(ELECTIONGUARD_BUILD_LIBS_DIR)/Ios" ]; then mkdir $(ELECTIONGUARD_BUILD_LIBS_DIR)/Ios; fi
	if [ ! -d "$(ELECTIONGUARD_BUILD_LIBS_DIR)/Darwin" ]; then mkdir $(ELECTIONGUARD_BUILD_LIBS_DIR)/Darwin; fi
	if [ ! -d "$(ELECTIONGUARD_BUILD_LIBS_DIR)/Linux" ]; then mkdir $(ELECTIONGUARD_BUILD_LIBS_DIR)/Linux; fi
	if [ ! -d "$(ELECTIONGUARD_BUILD_LIBS_DIR)/Windows" ]; then mkdir $(ELECTIONGUARD_BUILD_LIBS_DIR)/Windows; fi
endif
	dotnet clean ./bindings/netstandard/ElectionGuard/ElectionGuard.sln
	dotnet clean ./src/electionguard-ui/ElectionGuard.UI.sln

clean-netstandard:
	@echo 🗑️ CLEAN NETSTANDARD
	dotnet clean ./bindings/netstandard/ElectionGuard/ElectionGuard.sln

clean-ui:
	@echo 🗑️ CLEAN UI
	cd ./src/electionguard-ui/ElectionGuard.UI && dotnet restore ElectionGuard.UI.csproj
	dotnet clean ./src/electionguard-ui/ElectionGuard.UI/ElectionGuard.UI.csproj
	dotnet clean ./src/electionguard-ui/ElectionGuard.UI.Test/ElectionGuard.UI.Test.csproj
	dotnet clean ./bindings/netstandard/ElectionGuard/ElectionGuard.ElectionSetup/ElectionGuard.ElectionSetup.csproj
	dotnet clean ./bindings/netstandard/ElectionGuard/ElectionGuard.ElectionSetup.Tests/ElectionGuard.ElectionSetup.Tests.csproj
	dotnet clean ./src/electionguard-ui/ElectionGuard.UI.Lib/ElectionGuard.UI.Lib.csproj

# Generate

generate-interop:
	cd ./src/interop-generator/ElectionGuard.InteropGenerator && \
		dotnet build && \
		dotnet run -- ./EgInteropClasses.json ../../../ && \
		cd ../../../

# Lint / Format

format: build
	cd $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR) && $(MAKE) format

lint:
	dotnet jb inspectcode -o="lint-results.xml" -f="Xml" --build --verbosity="WARN" --severity="Warning" bindings/netstandard/ElectionGuard/ElectionGuard.sln
	dotnet nvika parsereport "lint-results.xml" --treatwarningsaserrors

lint-ui:
	dotnet jb inspectcode -o="lint-results.xml" -f="Xml" --build --verbosity="WARN" --severity="Warning" src/electionguard-ui/ElectionGuard.UI.sln
	dotnet nvika parsereport "lint-results.xml" --treatwarningsaserrors

# Memcheck

memcheck:
	@echo 🧼 RUN STATIC ANALYSIS
ifeq ($(OPERATING_SYSTEM),Windows)
	@echo "Static analysis is only supported on Linux"
else
	cd $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR) && $(MAKE) memcheck-ElectionGuardTests
endif

# Publish

publish-ui:
	@echo 🧱 BUILD UI
ifeq ($(OPERATING_SYSTEM),Windows)
	dotnet publish -f net7.0-windows10.0.19041.0 -c $(TARGET) /p:ApplicationVersion=$(BUILD) /p:RuntimeIdentifierOverride=win10-x64 src/electionguard-ui/ElectionGuard.UI/ElectionGuard.UI.csproj
endif
ifeq ($(OPERATING_SYSTEM),Darwin)
	dotnet build -f net7.0-maccatalyst -c $(TARGET) /p:CreatePackage=true /p:ApplicationVersion=$(BUILD) src/electionguard-ui/ElectionGuard.UI/ElectionGuard.UI.csproj
endif

# Rebuild

rebuild: clean build

rebuild-ui: clean-ui build-ui

# Sanitizers

sanitize: sanitize-asan sanitize-tsan

sanitize-asan:
	@echo 🧼 SANITIZE ADDRESS AND UNDEFINED $(PROCESSOR)
ifeq ($(OPERATING_SYSTEM),Windows)
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/Debug -G "Visual Studio 17 2022" -A $(PROCESSOR) \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/sanitize.asan.cmake
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/Debug --config Debug
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/Debug/test/Debug/ElectionGuardTests
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/Debug/test/Debug/ElectionGuardCTests
else
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/Debug \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/sanitize.asan.cmake
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/Debug
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/Debug/test/ElectionGuardTests
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/Debug/test/ElectionGuardCTests
endif

sanitize-tsan:
	@echo 🧼 SANITIZE THREADS $(PROCESSOR)
ifeq ($(OPERATING_SYSTEM),Windows)
	echo "Thread sanitizer is only supported on Linux & Mac"
else
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/Debug \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/sanitize.tsan.cmake
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/Debug
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/Debug/test/ElectionGuardTests
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/Debug/test/ElectionGuardCTests
endif

# Start/Stop Docker Services (database)

start-db:
ifeq "${EG_DB_PASSWORD}" ""
	@echo "Set the EG_DB_PASSWORD environment variable"
	exit 1
endif
	docker compose --env-file ./.env -f src/electionguard-db/docker-compose.db.yml up -d

stop-db:
	docker compose --env-file ./.env -f src/electionguard-db/docker-compose.db.yml down

# Benchmarks

bench:
	@echo 🧪 BENCHMARK $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
ifeq ($(OPERATING_SYSTEM),Windows)
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET) -G "MSYS Makefiles" \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/benchmark.cmake
else
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET) \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/benchmark.cmake
endif
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)/test/ElectionGuardBenchmark

bench-netstandard: build-netstandard
	@echo 🧪 BENCHMARK $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET) net7.0

# handle executing benchamrks on different processors
ifeq ($(HOST_PROCESSOR),$(PROCESSOR))
	$(ELECTIONGUARD_BINDING_BENCH_DIR)/bin/$(PROCESSOR)/$(TARGET)/net7.0/ElectionGuard.Encryption.Bench.exe
else
	$(ELECTIONGUARD_BINDING_BENCH_DIR)/bin/$(PROCESSOR)/$(TARGET)/net7.0/ElectionGuard.Encryption.Bench.exe
endif

ifeq ($(OPERATING_SYSTEM),Windows)
	@echo 🧪 BENCHMARK $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET) netstandard2.0
	dotnet run --framework netstandard2.0 -a $(PROCESSOR) --configuration $(TARGET) \	
		--project ./bindings/netstandard/ElectionGuard/ElectionGuard.Encryption.Bench/Electionguard.Encryption.Bench.csproj 	
	# $(DOTNET_PATH)/$(PROCESSOR)/dotnet $(ELECTIONGUARD_BINDING_BENCH_DIR)/bin/$(PROCESSOR)/$(TARGET)/netstandard2.0/ElectionGuard.Encryption.Bench.dll
endif
	# $(DOTNET_PATH)/dotnet exec --runtimeconfig $(ELECTIONGUARD_BINDING_BENCH_DIR)/bin/$(PROCESSOR)/$(TARGET)/net7.0/ElectionGuard.Encryption.Bench.runtimeconfig.json $(ELECTIONGUARD_BINDING_BENCH_DIR)/bin/$(PROCESSOR)/$(TARGET)/net7.0/ElectionGuard.Encryption.Bench.dll
	# dotnet run --framework net7.0 -a $(PROCESSOR) --configuration $(TARGET) \
	# 	--project ./bindings/netstandard/ElectionGuard/ElectionGuard.Encryption.Bench/Electionguard.Encryption.Bench.csproj 
	# @echo net 4.8 $(PROCESSOR)
	# dotnet run --framework netstandard2.0 -a $(PROCESSOR) --configuration $(TARGET) \
	# 	--project ./bindings/netstandard/ElectionGuard/ElectionGuard.Encryption.Bench/Electionguard.Encryption.Bench.csproj 

bench-netstandard-arm64:
	PROCESSOR=arm64 && make bench-netstandard

bench-netstandard-x64:
	PROCESSOR=x64 && make bench-netstandard


	# @echo 🧪 BENCHMARK
	# @echo net 7.0 x64
	# ./bindings/netstandard/ElectionGuard/ElectionGuard.Encryption.Bench/bin/x64/$(TARGET)/net7.0/ElectionGuard.Encryption.Bench
	# @echo netstandard 2.0 x64
	# ./bindings/netstandard/ElectionGuard/ElectionGuard.Encryption.Bench/bin/x64/$(TARGET)/netstandard2.0/ElectionGuard.Encryption.Bench

# Test

test:
	@echo 🧪 TEST $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
ifeq ($(OPERATING_SYSTEM),Windows)
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET) \
		-G "Visual Studio 17 2022" -A $(VSPLATFORM) \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DDISABLE_VALE=$(TEMP_DISABLE_VALE) \
		-DUSE_MSVC=ON \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/test.cmake
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/ --config $(TARGET)
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/test/$(TARGET)/ElectionGuardTests
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/test/$(TARGET)/ElectionGuardCTests
else
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET) \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DDISABLE_VALE=$(TEMP_DISABLE_VALE) \
		-DUSE_32BIT_MATH=$(USE_32BIT_MATH) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/test.cmake
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)/test/ElectionGuardTests
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)/test/ElectionGuardCTests
endif

test-arm64:
	PROCESSOR=arm64 && make test

test-x64:
	PROCESSOR=x64 && make test

test-x86:
	PROCESSOR=x86 USE_32BIT_MATH=ON VSPLATFORM=Win32 && make test

test-msys2:
	@echo 🧪 TEST MSYS2 $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
ifeq ($(OPERATING_SYSTEM),Windows)
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET) -G "MSYS Makefiles" \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/test.cmake
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)/test/ElectionGuardTests
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)/test/ElectionGuardCTests
endif

test-netstandard: build-netstandard
	@echo 🧪 TEST NETSTANDARD $(PROCESSOR) $(TARGET)
	# make test-netstandard-copy-output
	dotnet test -a $(PROCESSOR) --configuration $(TARGET) ./bindings/netstandard/ElectionGuard/ElectionGuard.ElectionSetup.Tests/ElectionGuard.ElectionSetup.Tests.csproj
	dotnet test -a $(PROCESSOR) --configuration $(TARGET) ./bindings/netstandard/ElectionGuard/ElectionGuard.Encryption.Tests/ElectionGuard.Encryption.Tests.csproj
	dotnet test -a $(PROCESSOR) --configuration $(TARGET) ./bindings/netstandard/ElectionGuard/ElectionGuard.Decryption.Tests/ElectionGuard.Decryption.Tests.csproj

test-netstandard-arm64:
	PROCESSOR=arm64 && make test-netstandard

test-netstandard-x64:
	PROCESSOR=x64 && make test-netstandard

test-netstandard-x86:
ifeq ($(OPERATING_SYSTEM),Darwin)
	echo "x86 builds are not supported on MacOS"
else
	PROCESSOR=x86 && make test-netstandard
endif

# copy the build output from the processor builds to the default build for the current platform (which enables debugging in vscode using code lens)
test-netstandard-copy-output:
	@echo 🧪 TEST NETSTANDARD COPY OUTPUT $(PROCESSOR) $(TARGET)
ifeq ($(OPERATING_SYSTEM),Windows)
	mkdir ./bindings/netstandard/ElectionGuard/ElectionGuard.Decryption.Tests/bin/$(TARGET)/net7.0/win-$(PROCESSOR) /p 
	mkdir ./bindings/netstandard/ElectionGuard/ElectionGuard.ElectionSetup.Tests/bin/$(TARGET)/net7.0/win-$(PROCESSOR) /p 
	mkdir ./bindings/netstandard/ElectionGuard/ElectionGuard.Encryption.Tests/bin/$(TARGET)/net7.0/win-$(PROCESSOR) /p 

	mkdir ./bindings/netstandard/ElectionGuard/ElectionGuard.Decryption.Tests/bin/$(TARGET)/net7.0/runtimes/win/native /p 
	mkdir ./bindings/netstandard/ElectionGuard/ElectionGuard.ElectionSetup.Tests/bin/$(TARGET)/net7.0/runtimes/win/native /p 
	mkdir ./bindings/netstandard/ElectionGuard/ElectionGuard.Encryption.Tests/bin/$(TARGET)/net7.0/runtimes/win/native /p 

	xcopy "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/src/$(TARGET)/electionguard.dll" "bindings/netstandard/ElectionGuard/ElectionGuard.Decryption.Tests/bin/$(TARGET)/net7.0/win-$(PROCESSOR)/electionguard.dll" /s 
	xcopy "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/src/$(TARGET)/electionguard.dll" "bindings/netstandard/ElectionGuard/ElectionGuard.ElectionSetup.Tests/bin/$(TARGET)/net7.0/win-$(PROCESSOR)/electionguard.dll" /s 
	xcopy "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/src/$(TARGET)/electionguard.dll" "bindings/netstandard/ElectionGuard/ElectionGuard.Encryption.Tests/bin/$(TARGET)/net7.0/win-$(PROCESSOR)/electionguard.dll" /s 

	xcopy "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/libs/hacl/$(TARGET)/hacl_cpp.dll" "bindings/netstandard/ElectionGuard/ElectionGuard.Decryption.Tests/bin/$(TARGET)/net7.0/win-$(PROCESSOR)/hacl_cpp.dll" /s 
	xcopy "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/libs/hacl/$(TARGET)/hacl_cpp.dll" "bindings/netstandard/ElectionGuard/ElectionGuard.ElectionSetup.Tests/bin/$(TARGET)/net7.0/win-$(PROCESSOR)/hacl_cpp.dll" /s 
	xcopy "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/libs/hacl/$(TARGET)/hacl_cpp.dll" "bindings/netstandard/ElectionGuard/ElectionGuard.Encryption.Tests/bin/$(TARGET)/net7.0/win-$(PROCESSOR)/hacl_cpp.dll" /s 

	xcopy "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/_deps/hacl-build/$(TARGET)/hacl.dll" "bindings/netstandard/ElectionGuard/ElectionGuard.Decryption.Tests/bin/$(TARGET)/net7.0/win-$(PROCESSOR)/hacl.dll" /s 
	xcopy "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/_deps/hacl-build/$(TARGET)/hacl.dll" "bindings/netstandard/ElectionGuard/ElectionGuard.ElectionSetup.Tests/bin/$(TARGET)/net7.0/win-$(PROCESSOR)/hacl.dll" /s 
	xcopy "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/_deps/hacl-build/$(TARGET)/hacl.dll" "bindings/netstandard/ElectionGuard/ElectionGuard.Encryption.Tests/bin/$(TARGET)/net7.0/win-$(PROCESSOR)/hacl.dll" /s 
endif
ifeq ($(OPERATING_SYSTEM),Darwin)
	TARGET_FOLDERS="./bindings/netstandard/ElectionGuard/ElectionGuard.Decryption.Tests/bin/$(TARGET)/net7.0/osx-$(PROCESSOR) ./bindings/netstandard/ElectionGuard/ElectionGuard.ElectionSetup.Tests/bin/$(TARGET)/net7.0/osx-$(PROCESSOR) ./bindings/netstandard/ElectionGuard/ElectionGuard.Encryption.Tests/bin/$(TARGET)/net7.0/osx-$(PROCESSOR)"
	NATIVE_FOLDERS="./bindings/netstandard/ElectionGuard/ElectionGuard.Decryption.Tests/bin/$(TARGET)/net7.0/runtimes/osx/native ./bindings/netstandard/ElectionGuard/ElectionGuard.ElectionSetup.Tests/bin/$(TARGET)/net7.0/runtimes/osx/native ./bindings/netstandard/ElectionGuard/ElectionGuard.Encryption.Tests/bin/$(TARGET)/net7.0/runtimes/osx/native"
	ALL_FOLDERS=$(TARGET_FOLDERS) $(NATIVE_FOLDERS)
	echo $(ALL_FOLDERS) | xargs -n 1 mkdir -p

	echo $(ALL_FOLDERS) | xargs cp -n 1 -r "./build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/src/libelectionguard.dylib" \
		"./build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/libs/hacl/libhacl_cpp.dylib" \
		"./build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/_deps/hacl-build/libhacl.dylib" 
endif

test-netstandard-copy-output-ui:
	@echo 🧪 TEST NETSTANDARD COPY UI OUTPUT $(PROCESSOR) $(TARGET)
ifeq ($(OPERATING_SYSTEM),Windows)
	cp -r "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/src/$(TARGET)/electionguard.dll" "src/electionguard-ui/electionGuard.UI.Test/bin/$(TARGET)/net7.0/win-$(PROCESSOR)/electionguard.dll"
	cp -r "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/libs/hacl/$(TARGET)/hacl_cpp.dll" "src/electionguard-ui/electionGuard.UI.Test/bin/$(TARGET)/net7.0/win-$(PROCESSOR)/hacl_cpp.dll"
	cp -r "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/_deps/hacl-build/$(TARGET)/hacl.dll" "src/electionguard-ui/electionGuard.UI.Test/bin/$(TARGET)/net7.0/win-$(PROCESSOR)/hacl.dll"
endif
ifeq ($(OPERATING_SYSTEM),Darwin)
	cp -r "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/src/libelectionguard.dylib" "src/electionguard-ui/electionGuard.UI.Test/bin/$(TARGET)/net7.0/osx-$(PROCESSOR)"
	cp -r "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/libs/hacl/libhacl_cpp.dylib" "src/electionguard-ui/electionGuard.UI.Test/bin/$(TARGET)/net7.0/osx-$(PROCESSOR)"
	cp -r "build/libs/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/_deps/hacl-build/libhacl.dylib" "src/electionguard-ui/electionGuard.UI.Test/bin/$(TARGET)/net7.0/osx-$(PROCESSOR)"
endif

test-ui: build-ui
	@echo 🧪 TEST UI $(PROCESSOR) $(TARGET)
	# dotnet build -a $(PROCESSOR) --configuration $(TARGET) ./src/electionguard-ui/electionGuard.UI.Test/ElectionGuard.UI.Test.csproj
	
	make build-netstandard
	make test-netstandard-copy-output
	make test-netstandard-copy-output-ui

	dotnet test -a $(PROCESSOR) --no-build --configuration $(TARGET) ./src/electionguard-ui/ElectionGuard.UI.Test/ElectionGuard.UI.Test.csproj

# Coverage

coverage:
	@echo ✅ CHECK COVERAGE $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
ifeq ($(OPERATING_SYSTEM),Windows)
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET) -G "MSYS Makefiles" \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DCODE_COVERAGE=ON \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/test.cmake
else
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET) \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DCODE_COVERAGE=ON \
		-DUSE_STATIC_ANALYSIS=ON \
		-DDISABLE_VALE=$(TEMP_DISABLE_VALE) \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/test.cmake
endif
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)/test/ElectionGuardTests
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)/test/ElectionGuardCTests

# Sample Data

fetch-sample-data:
	@echo ⬇️ FETCH Sample Data
	wget -O sample-data-1-0.zip https://github.com/microsoft/electionguard/releases/download/v1.0/sample-data.zip
	unzip -o sample-data-1-0.zip

generate-sample-data:
	@echo Generate Sample Data
