"""Packaging gate tests: an archive must never advertise a version its DLL does not carry."""
import unittest
from pathlib import Path

from package import ROOT, assembly_versions, check_compiled_version


def built_assembly(configuration="Release"):
    return ROOT / "VGBBoardAlways/bin" / configuration / "netstandard2.1/VGBBoardAlways.dll"


class CompiledVersionGate(unittest.TestCase):
    def setUp(self):
        dll = built_assembly()
        if not dll.is_file():
            self.skipTest("Build the plugin before running packaging tests")
        self.data = dll.read_bytes()
        versions = assembly_versions(self.data)
        self.assertEqual(1, len(versions), "The assembly must carry exactly one four-part version")
        self.version = versions.pop().rsplit(".", 1)[0]

    def test_matching_assembly_is_accepted(self):
        check_compiled_version(self.data, self.version)

    def test_assembly_built_from_another_version_is_rejected(self):
        major, minor, patch = self.version.split(".")
        older = f"{major}.{minor}.{int(patch) + 1}"
        with self.assertRaises(AssertionError):
            check_compiled_version(self.data, older)

    def test_stale_assembly_cannot_be_packaged_under_a_new_version(self):
        stale = self.data.replace(self.version.encode("ascii") + b".0", b"0.0.1.0")
        self.assertNotEqual(self.data, stale)
        with self.assertRaises(AssertionError):
            check_compiled_version(stale, self.version)


if __name__ == "__main__":
    unittest.main()
