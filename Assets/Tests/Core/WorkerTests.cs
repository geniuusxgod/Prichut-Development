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
    }
}
