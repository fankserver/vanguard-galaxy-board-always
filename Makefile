TFM      := netstandard2.1
CONFIG   := Debug
DLL      := VGBBoardAlways.dll

BUILDDIR := VGBBoardAlways/bin/$(CONFIG)/$(TFM)
BUILDDLL := $(BUILDDIR)/$(DLL)

GAME_DIR := /mnt/c/Program Files (x86)/Steam/steamapps/common/Vanguard Galaxy
PLUGIN_DIR := $(GAME_DIR)/BepInEx/plugins

DOTNET   ?= $(shell command -v dotnet 2>/dev/null || echo /tmp/dnsdk/dotnet/dotnet)

METADATA := vg.boardalways.vgmod.json
PLUGIN_FOLDER := $(PLUGIN_DIR)/VGBBoardAlways

.PHONY: all build link-api clean deploy package check-bepinex

all: build

check-bepinex:
	@test -d "$(GAME_DIR)/BepInEx/plugins" || { \
		echo "BepInEx plugins dir not found at $(GAME_DIR)/BepInEx/plugins." ; \
		echo "Install BepInEx 5.x into the game folder and launch the game once." ; \
		exit 1 ; \
	}

API_DIR ?= ../vanguard-galaxy-api
API_ABSTRACTIONS ?= $(API_DIR)/VGModAPI.Abstractions/bin/Release/$(TFM)/VGModAPI.Abstractions.dll
link-api:
	@mkdir -p VGBBoardAlways/lib
	@test -f "$(API_ABSTRACTIONS)" || { echo 'Build Mod API 0.2.7+ abstractions or set API_ABSTRACTIONS to its DLL.'; exit 1; }
	ln -sfn "$(abspath $(API_ABSTRACTIONS))" VGBBoardAlways/lib/VGModAPI.Abstractions.dll

build: link-api
	DOTNET_ROOT=$(dir $(DOTNET)) $(DOTNET) build VGBBoardAlways/VGBBoardAlways.csproj -c $(CONFIG)

.PHONY: test
test: build
	$(DOTNET) test VGBBoardAlways.Tests/VGBBoardAlways.Tests.csproj -c $(CONFIG)

package: build
	python3 tools/package.py --configuration $(CONFIG)

deploy: build check-bepinex
	@test ! -f "$(PLUGIN_DIR)/$(DLL)" || { echo 'Remove the old standalone $(DLL) from $(PLUGIN_DIR) before folder deployment.'; exit 1; }
	@mkdir -p "$(PLUGIN_FOLDER)"
	cp "$(BUILDDLL)" "$(METADATA)" README.md LICENSE "$(PLUGIN_FOLDER)/"
	@echo "Deployed $(DLL) and $(METADATA) to $(PLUGIN_FOLDER)"

clean:
	$(DOTNET) clean VGBBoardAlways.sln
	rm -rf VGBBoardAlways/bin VGBBoardAlways/obj VGBBoardAlways.Tests/bin VGBBoardAlways.Tests/obj
