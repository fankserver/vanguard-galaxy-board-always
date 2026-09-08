TFM      := netstandard2.1
CONFIG   := Debug
DLL      := VGBBoardAlways.dll

BUILDDIR := VGBBoardAlways/bin/$(CONFIG)/$(TFM)
BUILDDLL := $(BUILDDIR)/$(DLL)

GAME_DIR := /mnt/c/Program Files (x86)/Steam/steamapps/common/Vanguard Galaxy
PLUGIN_DIR := $(GAME_DIR)/BepInEx/plugins

DOTNET   ?= $(shell command -v dotnet 2>/dev/null || echo /tmp/dnsdk/dotnet/dotnet)

.PHONY: all build link-asm clean deploy check-bepinex

all: build

check-bepinex:
	@test -d "$(GAME_DIR)/BepInEx/plugins" || { \
		echo "BepInEx plugins dir not found at $(GAME_DIR)/BepInEx/plugins." ; \
		echo "Install BepInEx 5.x into the game folder and launch the game once." ; \
		exit 1 ; \
	}

API_DIR ?= ../vanguard-galaxy-api
link-asm:
	@mkdir -p VGBBoardAlways/lib
	@test -f "$(API_DIR)/VGModAPI.Abstractions/bin/Release/$(TFM)/VGModAPI.Abstractions.dll" || { echo 'Build Mod API Release first.'; exit 1; }
	ln -sfn "$(GAME_DIR)/VanguardGalaxy_Data/Managed/UnityEngine.dll" VGBBoardAlways/lib/UnityEngine.dll
	ln -sfn "$(GAME_DIR)/VanguardGalaxy_Data/Managed/UnityEngine.CoreModule.dll" VGBBoardAlways/lib/UnityEngine.CoreModule.dll
	ln -sfn "$(abspath $(API_DIR))/VGModAPI.Abstractions/bin/Release/$(TFM)/VGModAPI.Abstractions.dll" VGBBoardAlways/lib/VGModAPI.Abstractions.dll

build: link-asm
	DOTNET_ROOT=$(dir $(DOTNET)) $(DOTNET) build VGBBoardAlways/VGBBoardAlways.csproj -c $(CONFIG)

.PHONY: test
test: build
	$(DOTNET) test VGBBoardAlways.Tests/VGBBoardAlways.Tests.csproj -c $(CONFIG)

deploy: build check-bepinex
	@mkdir -p "$(PLUGIN_DIR)"
	cp "$(BUILDDLL)" "$(PLUGIN_DIR)/"
	@if [ -f "$(BUILDDIR)/VGBBoardAlways.pdb" ]; then cp "$(BUILDDIR)/VGBBoardAlways.pdb" "$(PLUGIN_DIR)/"; fi
	@echo "Deployed $(DLL) to $(PLUGIN_DIR)"

clean:
	$(DOTNET) clean VGBBoardAlways/VGBBoardAlways.csproj
	rm -rf VGBBoardAlways/bin VGBBoardAlways/obj
