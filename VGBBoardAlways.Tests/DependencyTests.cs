using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Xunit;
namespace VGBBoardAlways;
public sealed class DependencyTests
{
    [Fact]
    public void CompiledConsumerHasNoNativeBoardingOrHarmonyDependency()
    {
        var configuration = Directory.GetParent(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar))!.Name;
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(root, "VGBBoardAlways", "bin", configuration, "netstandard2.1", "VGBBoardAlways.dll"));
        Assert.Contains(assembly.MainModule.AssemblyReferences, reference => reference.Name == "VGModAPI.Abstractions");
        Assert.DoesNotContain(assembly.MainModule.AssemblyReferences, reference => reference.Name is "Assembly-CSharp" or "0Harmony" or "HarmonyX" or "VGModAPI.Core");
        var plugin = assembly.MainModule.GetType("VGBBoardAlways.Plugin");
        var dependency = Assert.Single(plugin.CustomAttributes.Where(attribute => attribute.AttributeType.FullName == "BepInEx.BepInDependency"));
        Assert.Equal(2, dependency.ConstructorArguments.Count);
        Assert.Equal(VGModAPI.ModApi.PluginId, dependency.ConstructorArguments[0].Value);
        Assert.Equal("0.2.7", dependency.ConstructorArguments[1].Value);
        Assert.Contains(assembly.MainModule.GetMemberReferences().OfType<MethodReference>(),
            reference => reference.DeclaringType.FullName == "VGModAPI.ModApi" && reference.Name == "get_Services");
        var policy = assembly.MainModule.GetType("VGBBoardAlways.BoardingPolicy");
        foreach (var method in policy.Methods.Where(method => method.HasBody))
            Assert.DoesNotContain(method.Body.Instructions.Select(instruction => instruction.Operand).OfType<MethodReference>(),
                reference => reference.DeclaringType.Namespace.StartsWith("UnityEngine", StringComparison.Ordinal) || reference.DeclaringType.Namespace.StartsWith("System.Reflection", StringComparison.Ordinal));
    }
}
