using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace VGBBoardAlways;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInProcess("VanguardGalaxy.exe")]
public class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "vg.boardalways";
    public const string PluginName = "Board Always";
    public const string PluginVersion = "0.2.0";

    internal static Plugin Instance { get; private set; } = null!;
    internal static ManualLogSource Log { get; private set; } = null!;

    internal ConfigEntry<bool> CfgEnabled = null!;
    internal ConfigEntry<float> CfgDifficultyModifier = null!;
    internal ConfigEntry<float> CfgIntegrityDamageMultiplier = null!;

    private Harmony _harmony = null!;

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        CfgEnabled = Config.Bind("General", "Enabled", true,
            "When true, enemy ships below 40% HP will become boardable " +
            "without RNG. The 15% damage accumulation gate is bypassed; " +
            "every damage tick in the sub-40% zone triggers instant 100% boarding chance.");

        CfgDifficultyModifier = Config.Bind("General", "DifficultyModifier", 1.0f,
            "Global multiplier for dungeon enemy difficulty. Scales defender combat power and " +
            "defender HP. Set below 1.0 to make high-level boardings easier. " +
            "1.0 is vanilla behavior.");

        CfgIntegrityDamageMultiplier = Config.Bind("General", "IntegrityDamageMultiplier", 1.0f,
            "Multiplier for ship integrity damage during boarding. Set to 0.0 to prevent ship integrity from dropping at all, or a small value (e.g. 0.05) to make it drop very slowly. 1.0 is vanilla behavior.");

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Patches.BoardingPatches));
        Log.LogInfo($"{PluginName} v{PluginVersion} loaded ({_harmony.GetPatchedMethods().Count()} patches)");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }
}
