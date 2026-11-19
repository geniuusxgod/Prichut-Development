using NUnit.Framework;

namespace Prichut.Resort.Tests
{
    public sealed class RatingTests
    {
        private static readonly ResortConfig Config = ResortConfig.CreateDefault();

        [TestCase(0.00f, RatingTier.OneStar)]
        [TestCase(0.40f, RatingTier.OneStar)]
        [TestCase(0.5499f, RatingTier.OneStar)]
        [TestCase(0.55f, RatingTier.TwoStars)]
        [TestCase(0.70f, RatingTier.ThreeStars)]
        [TestCase(0.80f, RatingTier.FourStars)]
        [TestCase(0.90f, RatingTier.FiveStars)]
        [TestCase(0.999f, RatingTier.FiveStars)]
        [TestCase(1.00f, RatingTier.Perfect)]
        public void ThresholdsPickTheRightTier(float satisfaction, RatingTier expected)
        {
            Assert.AreEqual(expected, RatingRules.FromSatisfaction(satisfaction, Config));
        }

        [Test]
        public void WorstValidWaveGivesOneStarAndCoins()
        {
            var run = new NeedRun(NeedId.Rooms, 10);
            run.contributionSum = 10 * Config.unitFloor;
            run.contributionCount = 10;
            float satisfaction = SatisfactionRules.WaveSatisfaction(new[] { run }, Config);
            Assert.AreEqual(Config.waveFloor, satisfaction, 0.0001f);
            RatingTier tier = RatingRules.FromSatisfaction(satisfaction, Config);
            Assert.AreEqual(RatingTier.OneStar, tier);
            Assert.Greater(RewardRules.Coins(10, tier, Config), 0);
        }

        [Test]
        public void FloorsKeepWr1()
        {
            Assert.IsTrue(SatisfactionRules.FloorsAreValid(Config, Config.ratingThresholds[2]));
        }

        [Test]
        public void ContributionCurve()
        {
            Assert.AreEqual(1f, SatisfactionRules.Contribution(0f, Config));
            Assert.AreEqual(1f, SatisfactionRules.Contribution(10f, Config));
            Assert.AreEqual(0.625f, SatisfactionRules.Contribution(20f, Config), 0.0001f);
            Assert.AreEqual(0.25f, SatisfactionRules.Contribution(30f, Config));
            Assert.AreEqual(0.25f, SatisfactionRules.Contribution(300f, Config));
        }

        [Test]
        public void ZeroDemandDoesNotDivideByZero()
        {
            Assert.AreEqual(1f, SatisfactionRules.WaveSatisfaction(new NeedRun[0], Config));
        }
    }
}
