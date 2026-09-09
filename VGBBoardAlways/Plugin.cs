using BepInEx;
using BepInEx.Configuration;
using VGModAPI;

namespace VGBBoardAlways;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInProcess("VanguardGalaxy.exe")]
[BepInDependency(ModApi.PluginId, "0.2.7")]
public class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "vg.boardalways";
    public const string PluginName = "Board Always";
    public const string PluginVersion = "0.4.1";
    private BoardingPolicy? _policy;

    private void Awake()
    {
        var enabled = Config.Bind("General", "Enabled", true,
            "Enable ship boarding eligibility, difficulty and integrity policies. False restores vanilla policy contributions.");
        var difficulty = Config.Bind("General", "DifficultyModifier", 1.0f,
            new ConfigDescription("Ship defender power and initial health multiplier, applied once at creation and in estimates. Existing saved encounters retain their tuning.", new AcceptableValueRange<float>(0, 10)));
        var integrity = Config.Bind("General", "IntegrityDamageMultiplier", 1.0f,
            new ConfigDescription("Ship boarding integrity damage multiplier, including scuttle damage exactly once. Cannot prevent authoritative host destruction.", new AcceptableValueRange<float>(0, 10)));
        var rules = ModApi.Services.BoardingRules;
        // Policy registration is session-independent; the API gates evaluation on current health.
        // Do not permanently skip registration because the service is unavailable during startup.
        if (!rules.Availability.IsAvailable)
            Logger.LogWarning($"Boarding rules currently unavailable ({rules.Availability.Reason}); registered policies remain inactive until the API permits evaluation. No native patch fallback is installed.");
        _policy = new BoardingPolicy(rules, PluginGuid, () => enabled.Value, () => difficulty.Value, () => integrity.Value);
        Logger.LogInfo($"{PluginName} v{PluginVersion} registered public boarding policies.");
    }
    private void OnDestroy() { _policy?.Dispose(); _policy = null; }
}
