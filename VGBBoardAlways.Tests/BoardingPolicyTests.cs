using System;
using VGModAPI;
using Xunit;
namespace VGBBoardAlways;
public sealed class BoardingPolicyTests
{
    private sealed class Rules : IBoardingRules, IBoardingRuleProvider
    {
        public bool IsEvaluating => false;
        internal Func<BoardingDisableContext, BoardingDisableDecision> Disable = null!;
        internal Func<BoardingEncounterContext, BoardingEncounterTuning> Tuning = null!;
        internal Func<BoardingIntegrityContext, float> Integrity = null!;
        internal bool Disposed, Fail;
        public IBoardingRuleProvider AcquireProvider(string id) => this;
        public IDisposable RegisterDisable(string id, Func<BoardingDisableContext, BoardingDisableDecision> evaluate, int priority = 0) { Disable = evaluate; return this; }
        public IDisposable RegisterEncounter(string id, BoardingRuleScope scope, Func<BoardingEncounterContext, BoardingEncounterTuning> evaluate, int priority = 0)
        { Assert.Equal(BoardingRuleScope.Ships, scope); if (Fail) throw new InvalidOperationException(); Tuning = evaluate; return this; }
        public IDisposable RegisterIntegrity(string id, BoardingRuleScope scope, Func<BoardingIntegrityContext, float> multiplier, int priority = 0)
        { Assert.Equal(BoardingRuleScope.Ships, scope); Integrity = multiplier; return this; }
        public IDisposable RegisterDisableChance(string id, Func<BoardingDisableContext, float?> probability, int priority = 0) => throw new NotSupportedException();
        public IDisposable RegisterExplosion(string id, BoardingRuleScope scope, Func<BoardingEncounterContext, bool> allow, int priority = 0) => throw new NotSupportedException();
        public IDisposable RegisterScuttle(string id, BoardingRuleScope scope, Func<BoardingEncounterContext, bool> allow, int priority = 0) => throw new NotSupportedException();
        public void Dispose() => Disposed = true;
    }
    [Fact]
    public void EnabledGatesAllPoliciesAndDifficultyCanIncrease()
    {
        var rules = new Rules(); var enabled = true;
        using (var policy = new BoardingPolicy(rules, "test", () => enabled, () => 2, () => .5f))
        {
            var session = Guid.NewGuid(); var encounter = new BoardingEncounterContext(session, BoardingEncounterKind.Ship, 1);
            Assert.Equal(BoardingDisableDecision.Allow, rules.Disable(new(session, 39, 100, 0)));
            Assert.Equal(BoardingDisableDecision.Vanilla, rules.Disable(new(session, 40, 100, 0)));
            Assert.Equal(2, rules.Tuning(encounter).DefenderPowerMultiplier);
            Assert.Equal(2, rules.Tuning(encounter).DefenderHealthMultiplier);
            Assert.Equal(.5f, rules.Integrity(new(encounter, BoardingDamageCause.Scuttle, 10, 100)));
            enabled = false;
            Assert.Equal(BoardingDisableDecision.Vanilla, rules.Disable(new(session, 39, 100, 0)));
            Assert.Equal(1, rules.Tuning(encounter).DefenderPowerMultiplier);
            Assert.Equal(1, rules.Integrity(new(encounter, BoardingDamageCause.Scuttle, 10, 100)));
        }
        Assert.True(rules.Disposed);
    }
    [Theory]
    [InlineData(-1, 0)] [InlineData(20, 10)] [InlineData(0, 0)] [InlineData(float.NaN, 1)] [InlineData(float.PositiveInfinity, 1)]
    public void InvalidConfigurationIsBounded(float input, float expected) => Assert.Equal(expected, BoardingPolicy.Normalize(input));
    [Fact]
    public void PartialRegistrationFailureDisposesProvider()
    { var rules = new Rules { Fail = true }; Assert.Throws<InvalidOperationException>(() => new BoardingPolicy(rules, "test", () => true, () => 1, () => 1)); Assert.True(rules.Disposed); }
}
