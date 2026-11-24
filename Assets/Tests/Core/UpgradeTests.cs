using NUnit.Framework;

namespace Prichut.Resort.Tests
{
    public sealed class UpgradeTests
    {
        [TestCase(1, 3)]
        [TestCase(2, 5)]
        [TestCase(3, 7)]
        public void LevelChangesConcurrentSeatsWithoutChangingServiceSpeed(int level, int capacity)
        {
            var config = ResortConfig.CreateDefault();
            var session = new ResortSession(config);
            int workersBefore = WorkerRoster.WorkerCount(session.State);
            float serviceTimeBefore = config.serviceTime;
            var run = new NeedRun(NeedId.Rooms, 8);
            float speedBefore = session.SpeedOf(run);

            session.State.Building(BuildingId.Reception).level = level;
            for (int i = 0; i < 8; i++)
                ServiceSystem.Arrive(run, 0f, session.CapacityOf(BuildingId.Reception), config);

            Assert.AreEqual(capacity, run.Seated);
            Assert.AreEqual(8 - capacity, run.Waiting);
            Assert.AreEqual(serviceTimeBefore, config.serviceTime);
            Assert.AreEqual(speedBefore, session.SpeedOf(run));
            Assert.AreEqual(workersBefore, WorkerRoster.WorkerCount(session.State));
        }

        [Test]
        public void CapacityLevelsAreThreeFiveSeven()
        {
            var config = ResortConfig.CreateDefault();
            Assert.AreEqual(3, config.MaxLevel);
            Assert.AreEqual(3, config.CapacityForLevel(1));
            Assert.AreEqual(5, config.CapacityForLevel(2));
            Assert.AreEqual(7, config.CapacityForLevel(3));
            Assert.AreEqual(7, config.CapacityForLevel(9), "Levels above max clamp to the top capacity.");
        }

        [Test]
        public void UpgradeIsOnlyAvailableBetweenWaves()
        {
            ResortSession session = TestResort.NewSession();
            session.State.coins = 5000;
            session.StartWave();
            TestResort.RunUntil(session, WavePhase.WaveActive);
            Assert.IsFalse(session.CanUpgrade(BuildingId.Restaurant), "WR6: no upgrade in WAVE_ACTIVE.");
            Assert.IsFalse(session.Upgrade(BuildingId.Restaurant));
            TestResort.RunUntil(session, WavePhase.WaveComplete);
            Assert.IsFalse(session.CanUpgrade(BuildingId.Restaurant), "Result first, then the upgrade step.");
            session.ClaimRewards();
            Assert.IsTrue(session.Upgrade(BuildingId.Restaurant));
            Assert.AreEqual(5, session.CapacityOf(BuildingId.Restaurant));
        }

        [Test]
        public void UpgradeCostsCoinsAndRaisesResortValue()
        {
            ResortSession session = TestResort.NewSession();
            session.State.coins = 600;
            int value = session.State.resortValue;
            Assert.IsTrue(session.Upgrade(BuildingId.BeachBar));
            Assert.AreEqual(100, session.State.coins);
            Assert.AreEqual(value + 60, session.State.resortValue);
            Assert.IsFalse(session.Upgrade(BuildingId.BeachBar), "Lv3 costs 1,200 - not affordable.");
            session.State.coins = 2000;
            Assert.IsTrue(session.Upgrade(BuildingId.BeachBar));
            Assert.IsFalse(session.CanUpgrade(BuildingId.BeachBar), "Maximum level reached.");
            Assert.AreEqual(-1, session.UpgradeCost(BuildingId.BeachBar));
        }
    }
}
