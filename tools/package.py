"""Build an immutable release bundle and matching Mod API update feed."""
import argparse
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET
import zipfile

ROOT = Path(__file__).resolve().parent.parent
REPOSITORY = "https://github.com/fankserver/vanguard-galaxy-board-always"
PLUGIN_ID = "vg.boardalways"


def unique_fields(pairs):
    result = {}
    for key, value in pairs:
        assert key not in result, "Duplicate metadata field"
        result[key] = value
    return result


def package(configuration="Release", output=ROOT / "dist", tag=None):
    version = ET.parse(ROOT / "VGBBoardAlways/VGBBoardAlways.csproj").findtext("PropertyGroup/Version")
    assert re.fullmatch(r"\d+\.\d+\.\d+", version), "Invalid release version"
    plugin = (ROOT / "VGBBoardAlways/Plugin.cs").read_text()
    assert f'PluginVersion = "{version}"' in plugin, "Plugin and project versions differ"
    assert tag is None or tag == "v" + version, "Release tag differs from package version"
    metadata = ROOT / (PLUGIN_ID + ".vgmod.json")
    data = json.loads(metadata.read_text(encoding="utf-8"), object_pairs_hook=unique_fields)
    assert set(data) <= {"schemaVersion", "pluginId", "author", "description", "projectUrl", "updateUrl", "channel"}
    for key, limit in {"pluginId": 128, "author": 256, "description": 4096, "projectUrl": 2048, "updateUrl": 2048, "channel": 32}.items():
        if key in data:
            assert isinstance(data[key], str) and 0 < len(data[key].encode("utf-16-le")) // 2 <= limit
    assert data["schemaVersion"] == 1 and data["pluginId"] == PLUGIN_ID
    assert data["channel"] == "stable"
    assert data["updateUrl"] == REPOSITORY + "/releases/latest/download/update.json"
    assert all(data.get(key) for key in ("author", "description", "projectUrl"))
    dll = ROOT / "VGBBoardAlways/bin" / configuration / "netstandard2.1/VGBBoardAlways.dll"
    assert dll.is_file(), "Build the plugin before packaging"
    assert dll.stat().st_mtime_ns >= max((ROOT / "VGBBoardAlways/Plugin.cs").stat().st_mtime_ns,
                                       (ROOT / "VGBBoardAlways/VGBBoardAlways.csproj").stat().st_mtime_ns), "Stale assembly: rebuild before packaging"
    output.mkdir(parents=True, exist_ok=True)
    archive = output / f"VGBBoardAlways-v{version}.zip"
    with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as bundle:
        for file in (dll, metadata, ROOT / "README.md", ROOT / "LICENSE"):
            if file.is_file():
                bundle.write(file, "VGBBoardAlways/" + file.name)
    feed = {"schemaVersion": 1, "pluginId": PLUGIN_ID, "channel": "stable",
            "version": version, "releaseUrl": REPOSITORY + "/releases/tag/v" + version}
    (output / "update.json").write_text(json.dumps(feed, indent=2) + "\n", encoding="utf-8")
    with zipfile.ZipFile(archive) as bundle:
        assert "VGBBoardAlways/" + metadata.name in bundle.namelist()
        assert [n for n in bundle.namelist() if n.endswith(".dll")] == ["VGBBoardAlways/VGBBoardAlways.dll"]
    print(archive)
    return archive


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--configuration", default="Release")
    parser.add_argument("--tag")
    options = parser.parse_args()
    package(options.configuration, tag=options.tag)
