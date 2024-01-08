.PHONY: all build build-msys2 build-android build-ios build-netstandard build-ui build-wasm build-npm clean clean-netstandard clean-ui clean-wasm environment environment-wasm format memcheck sanitize sanitize-asan sanitize-tsan bench bench-netstandard test test-msys2 test-netstandard test-netstandard-copy-output generate-sample-election-record verify

.EXPORT_ALL_VARIABLES:
ELECTIONGUARD_CACHE=$(subst \,/,$(realpath .))/.cache
ELECTIONGUARD_APPS_DIR=$(realpath .)/apps
ELECTIONGUARD_BINDING_DIR=$(realpath .)/bindings
ELECTIONGUARD_DATA_DIR=$(realpath .)/data
ELECTIONGUARD_APP_ADMIN_DIR=src/electionguard-ui
ELECTIONGUARD_APP_CLI_DIR=$(ELECTIONGUARD_APPS_DIR)/electionguard-cli
ELECTIONGUARD_BINDING_NETSTANDARD_DIR=$(ELECTIONGUARD_BINDING_DIR)/netstandard/ElectionGuard
ELECTIONGUARD_BINDING_LIB_DIR=$(ELECTIONGUARD_BINDING_NETSTANDARD_DIR)/ElectionGuard.Encryption
ELECTIONGUARD_BINDING_BENCH_DIR=$(ELECTIONGUARD_BINDING_NETSTANDARD_DIR)/ElectionGuard.Encryption.Bench
ELECTIONGUARD_BINDING_TEST_DIR=$(ELECTIONGUARD_BINDING_NETSTANDARD_DIR)/ElectionGuard.Encryption.Tests
ELECTIONGUARD_BINDING_UTILS_DIR=$(ELECTIONGUARD_BINDING_NETSTANDARD_DIR)/ElectionGuard.Encryption.Utils
ELECTIONGUARD_BINDING_TYPESCRIPT_DIR=$(ELECTIONGUARD_BINDING_DIR)/typescript
ELECTIONGUARD_BUILD_DIR=$(subst \,/,$(realpath .))/build
ELECTIONGUARD_BUILD_DIR_WIN=$(subst \c\,C:\,$(subst /,\,$(ELECTIONGUARD_BUILD_DIR)))
ELECTIONGUARD_BUILD_LIBS_DIR=$(ELECTIONGUARD_BUILD_DIR)/libs
ELECTIONGUARD_PUBLISH_DIR=$(subst \,/,$(realpath .))/publish
CPM_SOURCE_CACHE=$(ELECTIONGUARD_CACHE)/CPM
EMSCRIPTEN_VERSION?=3.1.35
EMSDK?=$(ELECTIONGUARD_CACHE)/emscripten

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
BUILD_NUMBER:=1
BUILD_VERSION:=$(shell git describe --tags --always)

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

# Set default OS paths for cross-compilation
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
	test -f /usr/local/bin/clang-tidy || sudo ln -sf "$(shell brew --prefix llvm)/bin/clang-tidy" "/usr/local/bin/clang-tidy"
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
	sudo apt install -y unzip
endif
ifeq ($(OPERATING_SYSTEM),Windows)
	@echo 🏁 WINDOWS INSTALL
	choco upgrade wget -y
	choco upgrade unzip -y
	choco upgrade cmake -y
	choco upgrade ninja -y
	choco upgrade vswhere -y
endif
	wget -O cmake/CPM.cmake https://github.com/cpm-cmake/CPM.cmake/releases/download/v0.38.2/CPM.cmake
	make fetch-sample-data
	dotnet tool restore

environment-msys2:
ifeq ($(OPERATING_SYSTEM),Windows)
	@echo 🏁 MSYS2 INSTALL
	pacman -S --noconfirm --needed --overwrite \
			base-devel \
            mingw-w64-x86_64-gcc \
            mingw-w64-clang-x86_64-clang \
            mingw-w64-clang-x86_64-toolchain \
            mingw-w64-clang-x86_64-cmake \
            mingw-w64-clang-x86_64-llvm \
            make \
            git
endif

devcontainer: fetch-sample-data environment-ui
	dotnet tool restore

environment-ui:
ifeq ($(OPERATING_SYSTEM),Windows)
	dotnet workload install maui
	dotnet workload restore ./src/electionguard-ui/ElectionGuard.UI/ElectionGuard.UI.csproj && dotnet restore ./src/electionguard-ui/ElectionGuard.UI.sln
endif
ifeq ($(OPERATING_SYSTEM),Darwin)
	sudo dotnet workload install maui
	sudo dotnet workload restore ./src/electionguard-ui/ElectionGuard.UI/ElectionGuard.UI.csproj && dotnet restore ./src/electionguard-ui/ElectionGuard.UI.sln
endif
ifeq ($(OPERATING_SYSTEM),Linux)
	sudo dotnet workload install maui-windows
	sudo dotnet workload restore ./src/electionguard-ui/ElectionGuard.UI/ElectionGuard.UI.csproj && dotnet restore ./src/electionguard-ui/ElectionGuard.UI.sln
endif
	dotnet tool restore
	npm i -g appcenter-cli

environment-wasm:
	@echo 🌐 WASM INSTALL
ifeq ($(OPERATING_SYSTEM),Windows)
	@echo currently only supported on posix systems
else
	./cmake/install-emscripten.sh $(EMSCRIPTEN_VERSION)
endif

# Builds
build:
	@echo 🧱 BUILD $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET) $(VSPLATFORM)
ifeq ($(OPERATING_SYSTEM),Windows)
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET) \
		-G "Visual Studio 17 2022" -A $(VSPLATFORM) \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DBUILD_SHARED_LIBS=ON \
		-DDISABLE_VALE=$(TEMP_DISABLE_VALE) \
		-DUSE_MSVC=ON \
		-DUSE_32BIT_MATH=$(USE_32BIT_MATH) \
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
ifeq ($(OPERATING_SYSTEM),Windows)
	PROCESSOR=arm64 && make build
else
	PROCESSOR=arm64 && make build
endif

build-x64:
ifeq ($(OPERATING_SYSTEM),Windows)
	PROCESSOR=x64 && make build
else
	PROCESSOR=x64 && make build
endif

build-x86:
ifeq ($(OPERATING_SYSTEM),Windows)
	PROCESSOR=x86 VSPLATFORM=Win32 USE_32BIT_MATH=ON && make build
else
	PROCESSOR=x86 VSPLATFORM=Win32 USE_32BIT_MATH=ON && make build
endif
	
build-msys2:
	@echo 🖥️ BUILD MSYS2 $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
ifeq ($(OPERATING_SYSTEM),Windows)
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET) -G "MSYS Makefiles" \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DBUILD_SHARED_LIBS=ON \
		-DDISABLE_VALE=$(TEMP_DISABLE_VALE) \
		-DUSE_32BIT_MATH=$(USE_32BIT_MATH) \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/$(PROCESSOR)-$(OPERATING_SYSTEM)-msys2.cmake
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)
else
	echo "MSYS2 builds are only supported on Windows"
endif

build-msys2-x86:
ifeq ($(OPERATING_SYSTEM),Windows)
	PROCESSOR=x86 USE_32BIT_MATH=ON && make build-msys2
else
	echo "MSYS2 builds are only supported on Windows"
endif

build-android:
	@echo 📱 BUILD ANDROID
ifeq ($(OPERATING_SYSTEM),Windows)
	PROCESSOR=arm64 OPERATING_SYSTEM=Android && make build
else
	PROCESSOR=arm64 OPERATING_SYSTEM=Android && make build
endif

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

build-maccatalyst:
# TODO: support maccatalyst-x64 builds
ifeq ($(OPERATING_SYSTEM),Darwin)
	PROCESSOR=arm64 OPERATING_SYSTEM=MacCatalyst && make build
else
	echo "MacCatalyst builds are only supported on MacOS"
endif

build-netstandard: build
	@echo 🖥️ BUILD NETSTANDARD $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
	cd ./bindings/netstandard/ElectionGuard && dotnet restore
	dotnet build -c $(TARGET) ./bindings/netstandard/ElectionGuard/ElectionGuard.sln /p:Platform=$(PROCESSOR)

build-netstandard-x64:
ifeq ($(OPERATING_SYSTEM),Windows)
	PROCESSOR=x64 && make build-netstandard
else
	PROCESSOR=x64 && make build-netstandard
endif

build-netstandard-x86:
ifeq ($(OPERATING_SYSTEM),Windows)
	PROCESSOR=x86 VSPLATFORM=Win32 USE_32BIT_MATH=ON && make build-netstandard
endif

build-cli:
	@echo 🖥️ BUILD CLI $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
	cd ./apps/electionguard-cli && dotnet restore
	dotnet build -c $(TARGET) $(ELECTIONGUARD_APP_CLI_DIR)/ElectionGuard.CLI.sln /p:Platform=$(PROCESSOR)

build-ui:
	@echo 🖥️ BUILD UI $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
	cd ./src/electionguard-ui && dotnet restore
	dotnet build -c $(TARGET) ./src/electionguard-ui/ElectionGuard.UI.sln /p:Platform=$(PROCESSOR) /p:APPCENTER_SECRET_UWP=$(APPCENTER_SECRET_UWP) /p:APPCENTER_SECRET_MACOS=$(APPCENTER_SECRET_MACOS)

build-wasm:
	@echo 🌐 BUILD WASM $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
ifeq ($(OPERATING_SYSTEM),Windows)
	echo "wasm builds are only supported on MacOS and Linux"
else
	# HACK temparily disable 64-bit math for emscripten
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/wasm/$(TARGET) \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DUSE_32BIT_MATH=ON \
		-DDISABLE_VALE=$(TEMP_DISABLE_VALE) \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=$(EMSDK)/upstream/emscripten/cmake/Modules/Platform/Emscripten.cmake
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/wasm/$(TARGET)
	cp $(ELECTIONGUARD_BUILD_LIBS_DIR)/wasm/$(TARGET)/src/electionguard/wasm/electionguard.wasm.js $(ELECTIONGUARD_BINDING_TYPESCRIPT_DIR)/src/wasm/electionguard.wasm.js
	#cp $(ELECTIONGUARD_BUILD_LIBS_DIR)/wasm/$(TARGET)/src/electionguard/wasm/electionguard.wasm.wasm $(ELECTIONGUARD_BINDING_TYPESCRIPT_DIR)/src/wasm/electionguard.wasm.wasm
	#cp $(ELECTIONGUARD_BUILD_LIBS_DIR)/wasm/$(TARGET)/src/electionguard/wasm/electionguard.wasm.worker.js $(ELECTIONGUARD_BINDING_TYPESCRIPT_DIR)/src/wasm/electionguard.wasm.worker.js
endif

build-npm: build-wasm
	@echo 🌐 BUILD NPM $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
	cd $(ELECTIONGUARD_BINDING_TYPESCRIPT_DIR) && npm install
	cd $(ELECTIONGUARD_BINDING_TYPESCRIPT_DIR) && npm run prepare
	
# Clean

clean-build:
	@echo 🗑️ Cleaning Output Directory
ifeq ($(OPERATING_SYSTEM),Windows)
	pwsh -Command "rm -R -Fo $(ELECTIONGUARD_BUILD_DIR_WIN); $$null"
else
	if [ -d "$(ELECTIONGUARD_BUILD_DIR)" ]; then rm -rf $(ELECTIONGUARD_BUILD_DIR)/*; fi
	if [ ! -d "$(ELECTIONGUARD_BUILD_DIR)" ]; then mkdir $(ELECTIONGUARD_BUILD_DIR); fi

	@echo 🗑️ Creating Output Directories
	if [ ! -d "$(ELECTIONGUARD_BUILD_LIBS_DIR)" ]; then mkdir $(ELECTIONGUARD_BUILD_LIBS_DIR); fi
	if [ ! -d "$(ELECTIONGUARD_BUILD_LIBS_DIR)/Android" ]; then mkdir $(ELECTIONGUARD_BUILD_LIBS_DIR)/Android; fi
	if [ ! -d "$(ELECTIONGUARD_BUILD_LIBS_DIR)/Ios" ]; then mkdir $(ELECTIONGUARD_BUILD_LIBS_DIR)/Ios; fi
	if [ ! -d "$(ELECTIONGUARD_BUILD_LIBS_DIR)/Darwin" ]; then mkdir $(ELECTIONGUARD_BUILD_LIBS_DIR)/Darwin; fi
	if [ ! -d "$(ELECTIONGUARD_BUILD_LIBS_DIR)/Linux" ]; then mkdir $(ELECTIONGUARD_BUILD_LIBS_DIR)/Linux; fi
	if [ ! -d "$(ELECTIONGUARD_BUILD_LIBS_DIR)/Windows" ]; then mkdir $(ELECTIONGUARD_BUILD_LIBS_DIR)/Windows; fi
	if [ ! -d "$(ELECTIONGUARD_BUILD_LIBS_DIR)/Wasm" ]; then mkdir $(ELECTIONGUARD_BUILD_LIBS_DIR)/Wasm; fi
endif	

clean-netstandard:
	@echo 🗑️ CLEAN NETSTANDARD
	cd $(ELECTIONGUARD_APP_CLI_DIR) && dotnet restore
	dotnet clean -c Debug $(ELECTIONGUARD_APP_CLI_DIR)/ElectionGuard.CLI.sln
	dotnet clean -c Release $(ELECTIONGUARD_APP_CLI_DIR)/ElectionGuard.CLI.sln
	dotnet clean -c Debug ./bindings/netstandard/ElectionGuard/ElectionGuard.sln
	dotnet clean -c Release ./bindings/netstandard/ElectionGuard/ElectionGuard.sln

clean-ui:
	@echo 🗑️ CLEAN UI
	cd ./$(ELECTIONGUARD_APP_ADMIN_DIR) && dotnet restore
	dotnet clean -c Debug ./$(ELECTIONGUARD_APP_ADMIN_DIR)/ElectionGuard.UI.sln
	dotnet clean -c Release ./$(ELECTIONGUARD_APP_ADMIN_DIR)/ElectionGuard.UI.sln

clean-wasm:
	@echo 🗑️ CLEAN WASM
ifeq ($(OPERATING_SYSTEM),Windows)
	echo "wasm builds are only supported on MacOS and Linux"
else
	cd $(ELECTIONGUARD_BINDING_TYPESCRIPT_DIR) && npm run clean:all
endif

clean: clean-build clean-netstandard clean-ui clean-wasm
	@echo 🗑️ CLEAN ALL

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
	@echo 🧱 PUBLISH UI
ifeq ($(OPERATING_SYSTEM),Windows)
	dotnet publish -f net7.0-windows10.0.19041.0 -c $(TARGET) /p:WindowsPackageType=None /p:ApplicationVersion=$(BUILD_NUMBER) /p:RuntimeIdentifierOverride=win10-x64 /p:APPCENTER_SECRET_UWP=$(APPCENTER_SECRET_UWP) ./$(ELECTIONGUARD_APP_ADMIN_DIR)/ElectionGuard.UI/ElectionGuard.UI.csproj -o ./publish/ElectionGuard.UI
	cd ./publish && pwsh -Command "Compress-Archive ElectionGuard.UI ElectionGuard.UI.zip -Force"
endif
ifeq ($(OPERATING_SYSTEM),Darwin)
	dotnet publish -f net7.0-maccatalyst -c $(TARGET) /p:CreatePackage=true /p:ApplicationVersion=$(BUILD_NUMBER) /p:APPCENTER_SECRET_MACOS=$(APPCENTER_SECRET_MACOS) ./$(ELECTIONGUARD_APP_ADMIN_DIR)/ElectionGuard.UI/ElectionGuard.UI.csproj -o ./publish
endif

publish-ui-appcenter: 
	@echo 🧱 PUBLISH UI APPCENTER
ifneq ($(APPCENTER_SECRET_UWP),)
ifeq ($(OPERATING_SYSTEM),Windows)
	@echo "Publishing UWP to AppCenter"
	appcenter distribute release -f $(ELECTIONGUARD_PUBLISH_DIR)/ElectionGuard.UI.zip -g QualityAssurance -a "Election-Technology-Initiative/ElectionGuard-Admin-1" -n $(BUILD_NUMBER) -b $(BUILD_VERSION) --disable-telemetry --token $(APPCENTER_API_TOKEN_UWP)
endif
else
	@echo "APPCENTER_SECRET_UWP not set. Skipping AppCenter publish"
	exit 1
endif
ifneq ($(APPCENTER_SECRET_MACOS),)
ifeq ($(OPERATING_SYSTEM),Darwin)
	@echo "Publishing MacCatalyst to AppCenter"
	appcenter distribute release -f $(ELECTIONGUARD_PUBLISH_DIR)/*.pkg -g QualityAssurance -a "Election-Technology-Initiative/ElectionGuard-Admin" -n $(BUILD_NUMBER) -b $(BUILD_VERSION) --disable-telemetry --token $(APPCENTER_API_TOKEN_MACOS)
endif
else
	@echo "APPCENTER_SECRET_MACOS not set. Skipping AppCenter publish"
	exit 1
endif

publish-wasm: build-npm
	@echo 🌐 PUBLISH WASM
ifeq ($(OPERATING_SYSTEM),Windows)
	@echo "wasm builds are only supported on MacOS and Linux"
else
	cd ./bindings/typescript && npm publish
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
# handle executing benchamrks on different processors
ifeq ($(HOST_PROCESSOR),$(PROCESSOR))
	@echo 🧪 BENCHMARK $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET) net7.0
	dotnet run --framework net7.0 -a $(PROCESSOR) -c $(TARGET) --project $(ELECTIONGUARD_BINDING_BENCH_DIR)/Electionguard.Encryption.Bench.csproj 	

	# @echo 🧪 BENCHMARK $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET) netstandard2.0
	# dotnet run --framework netstandard2.0 -a $(PROCESSOR) -c $(TARGET) --project $(ELECTIONGUARD_BINDING_BENCH_DIR)/Electionguard.Encryption.Bench.csproj 	
else
	@echo 🧪 BENCHMARK $(OPERATING_SYSTEM) $(PROCESSOR) on $(HOST_PROCESSOR) $(TARGET) net7.0
	$(DOTNET_PATH)/$(PROCESSOR)/dotnet exec --runtimeconfig $(ELECTIONGUARD_BINDING_BENCH_DIR)/bin/$(PROCESSOR)/$(TARGET)/net7.0/ElectionGuard.Encryption.Bench.runtimeconfig.json $(ELECTIONGUARD_BINDING_BENCH_DIR)/bin/$(PROCESSOR)/$(TARGET)/net7.0/ElectionGuard.Encryption.Bench.dll
	
	# @echo 🧪 BENCHMARK $(OPERATING_SYSTEM) $(PROCESSOR) on $(HOST_PROCESSOR) $(TARGET) netstandard2.0
	# $(DOTNET_PATH)/$(PROCESSOR)/dotnet $(ELECTIONGUARD_BINDING_BENCH_DIR)/bin/$(PROCESSOR)/$(TARGET)/netstandard2.0/ElectionGuard.Encryption.Bench.dll
endif

bench-netstandard-arm64:
ifeq ($(OPERATING_SYSTEM),Windows)
	set "PROCESSOR=arm64" && make bench-netstandard
else
	PROCESSOR=arm64 && make bench-netstandard
endif

bench-netstandard-x64:
ifeq ($(OPERATING_SYSTEM),Windows)
	set "PROCESSOR=x64" && make bench-netstandard
else
	PROCESSOR=x64 && make bench-netstandard
endif

# Test

test:
	@echo 🧪 TEST $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET) $(VSPLATFORM)
ifeq ($(OPERATING_SYSTEM),Windows)
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET) \
		-G "Visual Studio 17 2022" -A $(VSPLATFORM) \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DBUILD_SHARED_LIBS=ON \
		-DEXPORT_INTERNALS=ON \
		-DUSE_TEST_PRIMES=OFF \
		-DDISABLE_VALE=$(TEMP_DISABLE_VALE) \
		-DUSE_MSVC=ON \
		-DUSE_32BIT_MATH=$(USE_32BIT_MATH) \
		-DOPTION_ENABLE_TESTS=ON \
		-DLOG_LEVEL=debug \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/$(PROCESSOR)-$(OPERATING_SYSTEM).cmake
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/ --config $(TARGET)
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/test/$(TARGET)/ElectionGuardTests
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/test/$(TARGET)/ElectionGuardCTests
else
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET) \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DBUILD_SHARED_LIBS=ON \
		-DEXPORT_INTERNALS=ON \
		-DUSE_TEST_PRIMES=OFF \
		-DDISABLE_VALE=$(TEMP_DISABLE_VALE) \
		-DUSE_32BIT_MATH=$(USE_32BIT_MATH) \
		-DOPTION_ENABLE_TESTS=ON \
		-DLOG_LEVEL=debug \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/$(PROCESSOR)-$(OPERATING_SYSTEM).cmake
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)/test/ElectionGuardTests
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)/test/ElectionGuardCTests
endif

test-arm64:
ifeq ($(OPERATING_SYSTEM),Windows)
	PROCESSOR=arm64 && make test
else
	PROCESSOR=arm64 && make test
endif

test-x64:
ifeq ($(OPERATING_SYSTEM),Windows)
	PROCESSOR=x64 && make test
else
	PROCESSOR=x64 && make test
endif

test-x86:
	PROCESSOR=x86 USE_32BIT_MATH=ON VSPLATFORM=Win32 && make test

test-msys2:
	@echo 🧪 TEST MSYS2 $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
ifeq ($(OPERATING_SYSTEM),Windows)
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET) -G "MSYS Makefiles" \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DBUILD_SHARED_LIBS=ON \
		-DEXPORT_INTERNALS=ON \
		-DUSE_TEST_PRIMES=OFF \
		-DDISABLE_VALE=$(TEMP_DISABLE_VALE) \
		-DUSE_32BIT_MATH=$(USE_32BIT_MATH) \
		-DOPTION_ENABLE_TESTS=ON \
		-DLOG_LEVEL=debug \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/$(PROCESSOR)-$(OPERATING_SYSTEM)-msys2.cmake
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)/test/ElectionGuardTests
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)/test/ElectionGuardCTests
endif

test-msys2-x86:
ifeq ($(OPERATING_SYSTEM),Windows)
	PROCESSOR=x86 USE_32BIT_MATH=ON && make test-msys2
else
	echo "MSYS2 tests are only supported on Windows"
endif

test-netstandard: build-netstandard
	@echo 🧪 TEST NETSTANDARD $(PROCESSOR) $(TARGET)
	dotnet test -a $(PROCESSOR) -c $(TARGET) ./bindings/netstandard/ElectionGuard/ElectionGuard.ElectionSetup.Tests/ElectionGuard.ElectionSetup.Tests.csproj
	dotnet test -a $(PROCESSOR) -c $(TARGET) ./bindings/netstandard/ElectionGuard/ElectionGuard.Encryption.Tests/ElectionGuard.Encryption.Tests.csproj
	dotnet test -a $(PROCESSOR) -c $(TARGET) ./bindings/netstandard/ElectionGuard/ElectionGuard.Decryption.Tests/ElectionGuard.Decryption.Tests.csproj

test-netstandard-arm64:
ifeq ($(OPERATING_SYSTEM),Windows)
	PROCESSOR=arm64 && make test-netstandard
else
	PROCESSOR=arm64 && make test-netstandard
endif

test-netstandard-x64:
	PROCESSOR=x64 && make test-netstandard

test-netstandard-x86:
ifeq ($(OPERATING_SYSTEM),Darwin)
	echo "x86 builds are not supported on MacOS"
else
	PROCESSOR=x86 VSPLATFORM=Win32 USE_32BIT_MATH=ON && make test-netstandard
endif

# copy the build output from the processor builds to 
# the default build for the current platform 
# (which enables debugging in vscode using code lens)
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
	input_files="$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/src/*.{dylib,a} $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/libs/hacl/*.{dylib,a} $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(OPERATING_SYSTEM)/$(PROCESSOR)/$(TARGET)/_deps/hacl-build/*.{dylib,a}"
	output_dirs="$(ELECTIONGUARD_BINDING_NETSTANDARD_DIR)/ElectionGuard.Decryption.Tests/bin/$(TARGET)/net7.0/runtimes/osx/native $(ELECTIONGUARD_BINDING_NETSTANDARD_DIR)/ElectionGuard.ElectionSetup.Tests/bin/$(TARGET)/net7.0/runtimes/osx/native $(ELECTIONGUARD_BINDING_NETSTANDARD_DIR)/ElectionGuard.Encryption.Tests/bin/$(TARGET)/net7.0/runtimes/osx/native"
	echo "$$input_files" | xargs cp -n -R --preserve=mode,timestamps $$output_dirs
endif

test-cli: build-cli
	@echo 🧪 TEST CLI $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
	cd ./apps/electionguard-cli && dotnet run -- -c $(ELECTIONGUARD_DATA_DIR)/test/context.json -m $(ELECTIONGUARD_DATA_DIR)/test/manifest.json -b $(ELECTIONGUARD_DATA_DIR)/test/plaintext -d $(ELECTIONGUARD_DATA_DIR)/test/device.json -o $(ELECTIONGUARD_DATA_DIR)/test/output

test-ui: build-ui
	@echo 🧪 TEST UI $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
# dotnet test -a $(PROCESSOR) -c $(TARGET) ./src/electionguard-ui/ElectionGuard.UI.Test/ElectionGuard.UI.Test.csproj

test-wasm: build-wasm
	@echo 🧪 TEST WASM $(PROCESSOR) $(TARGET)
	cd ./bindings/typescript && npm run test

# Coverage

coverage:
	@echo ✅ CHECK COVERAGE $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
ifeq ($(OPERATING_SYSTEM),Windows)
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET) -G "MSYS Makefiles" \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DCODE_COVERAGE=ON \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/$(PROCESSOR)-$(OPERATING_SYSTEM).cmake
else
	cmake -S . -B $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET) \
		-DCMAKE_BUILD_TYPE=$(TARGET) \
		-DCODE_COVERAGE=ON \
		-DUSE_STATIC_ANALYSIS=ON \
		-DDISABLE_VALE=$(TEMP_DISABLE_VALE) \
		-DCPM_SOURCE_CACHE=$(CPM_SOURCE_CACHE) \
		-DCMAKE_TOOLCHAIN_FILE=cmake/toolchains/$(PROCESSOR)-$(OPERATING_SYSTEM).cmake
endif
	cmake --build $(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)/test/ElectionGuardTests
	$(ELECTIONGUARD_BUILD_LIBS_DIR)/$(PROCESSOR)/$(TARGET)/test/ElectionGuardCTests

# Sample Data

TEMPFILE=sample-data-1-0.zip
fetch-sample-data:
	@echo ⬇️ FETCH Sample Data
	wget -O $(TEMPFILE) https://github.com/microsoft/electionguard/releases/download/v1.0/sample-data.zip
	unzip -o $(TEMPFILE)
	rm -f $(TEMPFILE)

generate-sample-data: build-netstandard
	@echo Generate Sample Data
	dotnet test -a $(PROCESSOR) -c $(TARGET) --filter TestCategory=OutputTestData ./bindings/netstandard/ElectionGuard/ElectionGuard.Decryption.Tests/ElectionGuard.Decryption.Tests.csproj

generate-sample-election-record: build-netstandard
	@echo Generate Sample Data
	dotnet test -a $(PROCESSOR) -c $(TARGET) --filter TestCategory=GenerateElectionRecord ./bindings/netstandard/ElectionGuard/ElectionGuard.Decryption.Tests/ElectionGuard.Decryption.Tests.csproj


verify: build-cli
	@echo 🧪 Executing CLI Verifier $(OPERATING_SYSTEM) $(PROCESSOR) $(TARGET)
	cd ./apps/electionguard-cli && dotnet run -a $(PROCESSOR) -c $(TARGET) verify -- -f $(ELECTIONGUARD_DATA_DIR)/test/ElectionRecord.zip
	