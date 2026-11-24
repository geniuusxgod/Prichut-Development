using NUnit.Framework;

namespace Prichut.Resort.Tests
{
    public sealed class ServiceSpeedTests
    {
        private static readonly ResortConfig Config = ResortConfig.CreateDefault();

        [Test]
        public void EmptyBuildingIsSlowerButNeverStops()
        {
            float staffed = ServiceRules.Speed(Config, true);
            float empty = ServiceRules.Speed(Config, false);
            Assert.Greater(empty, 0f, "WR2: utilization_empty > 0");
            Assert.AreEqual(staffed * Config.utilizationEmpty, empty, 0.0001f);
        }

        [Test]
        public void MoveMidServiceKeepsProgress()
        {
            ResortSession session = TestResort.NewSession();
            session.StartWave();
            TestResort.RunUntil(session, WavePhase.WaveActive);
            TestResort.Run(session, 20f);
            NeedRun food = session.Wave.Find(NeedId.Food);
            Assert.Greater(food.Seated, 0);
            float before = food.seats[0];
            float slow = session.SpeedOf(food);
            Assert.IsTrue(session.MoveWorkerFrom(BuildingId.Reception));
            Assert.AreEqual(before, food.seats[0], 0.0001f, "Moving a worker must not reset a running service.");
            Assert.Greater(session.SpeedOf(food), slow, "Target speeds up immediately.");
            Assert.Less(session.SpeedOf(session.Wave.Find(NeedId.Rooms)), session.SpeedOf(food), "Source slows down immediately.");
            Assert.AreEqual(Config.CapacityForLevel(1), session.CapacityOf(BuildingId.Restaurant), "Capacity does not change.");
        }
    }
}
