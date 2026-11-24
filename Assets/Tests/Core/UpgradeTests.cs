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
    }
}
