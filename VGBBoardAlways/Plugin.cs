using BepInEx;
using BepInEx.Configuration;
using VGModAPI;

namespace VGBBoardAlways;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInProcess("VanguardGalaxy.exe")]
[BepInDependency(ModApi.PluginId, "0.1.42")]
public class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "vg.boardalways";
    public const string PluginName = "Board Always";
    public const string PluginVersion = "0.3.0";
    private BoardingPolicy? _policy;

    private void Awake()
    {
        var enabled = Config.Bind("General", "Enabled", true,
            "Enable ship boarding eligibility, difficulty and integrity policies. False restores vanilla policy contributions.");
        var difficulty = Config.Bind("General", "DifficultyModifier", 1.0f,
            new ConfigDescription("Ship defender power and initial health multiplier, applied once at creation and in estimates. Existing saved encounters retain their tuning.", new AcceptableValueRange<float>(0, 10)));
        var integrity = Config.Bind("General", "IntegrityDamageMultiplier", 1.0f,
            new ConfigDescription("Ship boarding integrity damage multiplier, including scuttle damage exactly once. Cannot prevent authoritative host destruction.", new AcceptableValueRange<float>(0, 10)));
        var rules = ModApi.BoardingRules;
        if (rules == null)
        {
            Logger.LogWarning("Boarding rules unavailable. Enable Mod API's experimental Boarding integration on a supported game build; no native patch fallback is installed.");
            return;
        }
        _policy = new BoardingPolicy(rules, PluginGuid, () => enabled.Value, () => difficulty.Value, () => integrity.Value);
        Logger.LogInfo($"{PluginName} v{PluginVersion} registered public boarding policies.");
    }
    private void OnDestroy() { _policy?.Dispose(); _policy = null; }
}
