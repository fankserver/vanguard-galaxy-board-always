using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Mono.Cecil;
using Xunit;
namespace VGBBoardAlways;

/// <summary>The sidecar is author metadata only: identity comes from the loader, never from this file.</summary>
public sealed class MetadataTests
{
    private static readonly string Root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
    private static JsonElement Metadata()
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(Root, "vg.boardalways.vgmod.json")));
        return document.RootElement.Clone();
    }

    [Fact]
    public void SidecarDeclaresOnlySupportedFieldsWithinDocumentedLimits()
    {
        var metadata = Metadata();
        var fields = metadata.EnumerateObject().Select(property => property.Name).ToArray();
        Assert.Equal(fields.Length, fields.Distinct(StringComparer.Ordinal).Count());
        Assert.Empty(fields.Except(new[] { "schemaVersion", "pluginId", "author", "description", "projectUrl", "updateUrl", "channel" }, StringComparer.Ordinal));
        Assert.Equal(1, metadata.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("stable", metadata.GetProperty("channel").GetString());
        foreach (var (field, limit) in new Dictionary<string, int> { ["author"] = 256, ["description"] = 4096, ["projectUrl"] = 2048, ["updateUrl"] = 2048 })
        {
            var value = metadata.GetProperty(field).GetString();
            Assert.False(string.IsNullOrWhiteSpace(value));
            Assert.InRange(value!.Length, 1, limit);
        }
        foreach (var field in new[] { "projectUrl", "updateUrl" })
            Assert.StartsWith("https://github.com/fankserver/vanguard-galaxy-board-always", metadata.GetProperty(field).GetString(), StringComparison.Ordinal);
        // Installed version is authoritative loader data; author metadata must never restate it.
        Assert.False(metadata.TryGetProperty("version", out _));
    }

    /// <summary>The published version must come from compiled metadata, not from a source file the archive never contains.</summary>
    [Fact]
    public void CompiledAssemblyAndLoaderVersionsMatchTheProjectVersion()
    {
        var declared = System.Xml.Linq.XDocument.Load(Path.Combine(Root, "VGBBoardAlways", "VGBBoardAlways.csproj"))
            .Descendants("Version").Single().Value;
        using var assembly = Compiled();
        Assert.Equal(new Version(declared + ".0"), assembly.Name.Version);
        var identity = Assert.Single(assembly.MainModule.GetType("VGBBoardAlways.Plugin").CustomAttributes
            .Where(attribute => attribute.AttributeType.FullName == "BepInEx.BepInPlugin"));
        Assert.Equal(declared, (string)identity.ConstructorArguments[2].Value);
    }

    private static AssemblyDefinition Compiled()
    {
        var configuration = Directory.GetParent(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar))!.Name;
        return AssemblyDefinition.ReadAssembly(Path.Combine(Root, "VGBBoardAlways", "bin", configuration, "netstandard2.1", "VGBBoardAlways.dll"));
    }

    [Fact]
    public void SidecarFileNameAndIdentityMatchTheCompiledLoaderGuid()
    {
        using var assembly = Compiled();
        var plugin = assembly.MainModule.GetType("VGBBoardAlways.Plugin");
        var identity = Assert.Single(plugin.CustomAttributes.Where(attribute => attribute.AttributeType.FullName == "BepInEx.BepInPlugin"));
        var guid = (string)identity.ConstructorArguments[0].Value;
        Assert.Equal(guid, Metadata().GetProperty("pluginId").GetString());
        Assert.True(File.Exists(Path.Combine(Root, guid + ".vgmod.json")));
    }
}
