"""Packaging gate tests: an archive must never advertise a version its DLL does not carry."""
import unittest
from pathlib import Path

from package import ROOT, check_compiled_version, resource_version


def built_assembly(configuration="Release"):
    return ROOT / "VGBBoardAlways/bin" / configuration / "netstandard2.1/VGBBoardAlways.dll"


class CompiledVersionGate(unittest.TestCase):
    def setUp(self):
        dll = built_assembly()
        if not dll.is_file():
            self.skipTest("Build the plugin before running packaging tests")
        self.data = dll.read_bytes()
        self.version = resource_version(self.data, "Assembly Version").rsplit(".", 1)[0]

    def test_matching_assembly_is_accepted(self):
        check_compiled_version(self.data, self.version)

    def test_assembly_built_from_another_version_is_rejected(self):
        major, minor, patch = self.version.split(".")
        for other in (f"{major}.{minor}.{int(patch) + 1}", f"{major}.{int(minor) + 1}.{patch}"):
            with self.subTest(version=other), self.assertRaises(AssertionError):
                check_compiled_version(self.data, other)

    def test_stale_assembly_cannot_be_packaged_under_a_new_version(self):
        stale = self.data.replace(self.version.encode("utf-16-le"), "0.0.1".encode("utf-16-le"))
        self.assertNotEqual(self.data, stale)
        with self.assertRaises(AssertionError):
            check_compiled_version(stale, self.version)

    def test_reference_versions_cannot_satisfy_the_gate(self):
        """Only the assembly's own keyed resource counts, never a referenced version string."""
        with self.assertRaises(AssertionError):
            check_compiled_version(b"Some.Reference, Version=0.0.0.0, Culture=neutral", "0.0.0")

    def test_assembly_without_a_version_resource_is_rejected(self):
        with self.assertRaises(AssertionError):
            check_compiled_version(b"no version resource here", self.version)


if __name__ == "__main__":
    unittest.main()
