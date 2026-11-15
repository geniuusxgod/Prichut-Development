using NUnit.Framework;

namespace Prichut.Resort.Tests
{
    public sealed class FifoQueueTests
    {
        private static readonly ResortConfig Config = ResortConfig.CreateDefault();

        [Test]
        public void WaitingUnitIsNeverRemovedForAge()
        {
            var run = new NeedRun(NeedId.Food, 10);
            for (int i = 0; i < 6; i++) ServiceSystem.Arrive(run, 0f, 3, Config);
            Assert.AreEqual(3, run.Waiting);
            // ziadna obsluha 10 minut (rychlost 0), nikto neodide
            for (int i = 0; i < 6000; i++) ServiceSystem.Tick(run, i * 0.1f, 0.1f, 0f, 3, Config);
            Assert.AreEqual(3, run.Waiting);
            Assert.AreEqual(3, run.Seated);
        }

        [Test]
        public void LongerWaitingUnitIsSeatedFirst()
        {
            var run = new NeedRun(NeedId.Drinks, 5);
            ServiceSystem.Arrive(run, 0f, 1, Config);
            ServiceSystem.Arrive(run, 1f, 1, Config);
            ServiceSystem.Arrive(run, 2f, 1, Config);
            Assert.AreEqual(new[] { 1f, 2f }, run.queue.ToArray());
            ServiceSystem.Tick(run, 12f, 12f, 1f / 12f, 1, Config);
            Assert.AreEqual(1, run.filled);
            Assert.AreEqual(new[] { 2f }, run.queue.ToArray(), "The unit from t=1 must sit down before t=2.");
        }

        [Test]
        public void LongQueueDrainsWithoutLosingUnits()
        {
            var config = ResortConfig.CreateDefault();
            config.waves = new[] { new WaveDefinition(10, 40, 2, 2, 0, 0) };
            ResortSession session = TestResort.NewSession(config);
            // Reception bez Workera: Rooms 40 caka dlhsie nez floor_after
            Assert.IsTrue(session.MoveWorkerFrom(BuildingId.Reception));
            session.StartWave();
            TestResort.RunUntil(session, WavePhase.WaveComplete, 3600f);
            Assert.AreEqual(WavePhase.WaveComplete, session.Phase);
            NeedResult rooms = session.LastResult.needs.Find(n => n.need == NeedId.Rooms);
            Assert.AreEqual(40, rooms.served);
            Assert.Greater(rooms.longestWait, config.patienceFloorAfter);
            Assert.AreEqual(session.LastResult.unitsTotal, session.LastResult.unitsServed);
        }
    }
}
