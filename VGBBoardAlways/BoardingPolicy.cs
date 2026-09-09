using System;
using VGModAPI;

namespace VGBBoardAlways;

/// <summary>Consumer balance policy; native eligibility, persistence and damage accounting belong to the API.</summary>
internal sealed class BoardingPolicy : IDisposable
{
    private readonly IBoardingRuleProvider _provider;
    internal BoardingPolicy(IBoardingRules rules, string pluginId, Func<bool> enabled,
        Func<float> difficulty, Func<float> integrity)
    {
        _provider = rules.AcquireProvider(pluginId);
        try
        {
            _provider.RegisterDisable("low-hull", context => enabled() && context.HullFraction < .4f
                ? BoardingDisableDecision.Allow : BoardingDisableDecision.Vanilla);
            _provider.RegisterEncounter("difficulty", BoardingRuleScope.Ships, _ =>
            {
                var multiplier = enabled() ? Normalize(difficulty()) : 1;
                return new BoardingEncounterTuning(multiplier, multiplier);
            });
            _provider.RegisterIntegrity("integrity", BoardingRuleScope.Ships,
                _ => enabled() ? Normalize(integrity()) : 1);
        }
        catch { _provider.Dispose(); throw; }
    }
    // Unsupported values cannot poison a native evaluation. Keep persisted config keys unchanged.
    internal static float Normalize(float value) => float.IsNaN(value) || float.IsInfinity(value) ? 1 : Math.Max(0, Math.Min(10, value));
    public void Dispose() => _provider.Dispose();
}
