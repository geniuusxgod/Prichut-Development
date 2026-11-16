using NUnit.Framework;

namespace Prichut.Resort.Tests
{
    public sealed class WorkerTests
    {
        [Test]
        public void StartHasThreeBuildingsTwoWorkersAndOneEmpty()
        {
            ResortSession session = TestResort.NewSession();
            Assert.AreEqual(3, WorkerRoster.OwnedBuildings(session.State));
            Assert.AreEqual(2, WorkerRoster.WorkerCount(session.State));
            Assert.AreEqual(1, WorkerRoster.EmptyBuildings(session.State).Count);
        }

        [Test]
        public void MoveIsFreeAndInstant()
        {
            ResortSession session = TestResort.NewSession();
            int coins = session.State.coins;
            int energy = session.State.energy;
            Assert.IsTrue(session.MoveWorkerFrom(BuildingId.BeachBar));
            Assert.IsFalse(WorkerRoster.IsStaffed(session.State, BuildingId.BeachBar));
            Assert.IsTrue(WorkerRoster.IsStaffed(session.State, BuildingId.Restaurant));
            Assert.AreEqual(2, WorkerRoster.WorkerCount(session.State));
            Assert.AreEqual(coins, session.State.coins);
            Assert.AreEqual(energy, session.State.energy);
        }

        [Test]
        public void EmptySourceOrSwapIsNotOffered()
        {
            ResortSession session = TestResort.NewSession();
            Assert.IsFalse(session.MoveWorkerFrom(BuildingId.Restaurant), "Restaurant has no worker to give.");
            Assert.IsFalse(session.MoveWorkerFrom(BuildingId.Pool), "Locked plot has no worker.");
        }

        [Test]
        public void MoveDuringWaveUpdatesBothServicesWithoutReset()
        {
            ResortSession session = TestResort.NewSession();
            session.StartWave();
            TestResort.RunUntil(session, WavePhase.WaveActive);
            TestResort.Run(session, 30f);
            NeedRun rooms = session.Wave.Find(NeedId.Rooms);
            NeedRun food = session.Wave.Find(NeedId.Food);
            int roomsFilled = rooms.filled;
            int foodFilled = food.filled;
            int seatedBefore = rooms.Seated + food.Seated;
            Assert.IsTrue(session.MoveWorkerFrom(BuildingId.Reception));
            Assert.AreEqual(seatedBefore, rooms.Seated + food.Seated, "Nobody is thrown out of a seat.");
            TestResort.Run(session, 5f);
            Assert.GreaterOrEqual(rooms.filled, roomsFilled, "Source keeps serving, only slower.");
            Assert.GreaterOrEqual(food.filled, foodFilled);
            Assert.AreEqual(2, WorkerRoster.WorkerCount(session.State), "One slot can never hold two workers.");
        }

        [Test]
        public void AssignmentIsKeptBetweenWaves()
        {
            ResortSession session = TestResort.NewSession();
            Assert.IsTrue(session.MoveWorkerFrom(BuildingId.BeachBar), "Move before Start Wave is allowed.");
            session.StartWave();
            Assert.IsTrue(WorkerRoster.IsStaffed(session.State, BuildingId.Restaurant), "Prepared placement is kept on start.");
            TestResort.RunUntil(session, WavePhase.WaveComplete);
            session.ClaimRewards();
            session.FinishUpgradeStep();
            Assert.AreEqual(WavePhase.AwaitingWave, session.Phase);
            Assert.IsTrue(WorkerRoster.IsStaffed(session.State, BuildingId.Restaurant));
            Assert.IsFalse(WorkerRoster.IsStaffed(session.State, BuildingId.BeachBar), "Next wave starts with the same placement.");
        }
    }
}
