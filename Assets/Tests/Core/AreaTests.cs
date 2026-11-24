using NUnit.Framework;

namespace Prichut.Resort.Tests
{
    public sealed class AreaTests
    {
        [Test]
        public void AvailabilityFollowsTheGateNotTheWallet()
        {
            ResortSession session = TestResort.NewSession();
            session.State.coins = 100000;
            Assert.AreEqual(AreaState.Locked, session.AreaStateOf(BuildingId.Pool), "Coins alone do not unlock.");
            session.State.completedWaves = 3;
            Assert.AreEqual(AreaState.Available, session.AreaStateOf(BuildingId.Pool));
            Assert.AreEqual(AreaState.Locked, session.AreaStateOf(BuildingId.Bungalows));
            Assert.AreEqual(BuildingId.Pool, session.PendingAreaOffer);
            session.MarkAreaOffered(BuildingId.Pool);
            Assert.IsNull(session.PendingAreaOffer, "S9 is offered automatically only once.");
            Assert.AreEqual(AreaState.Available, session.AreaStateOf(BuildingId.Pool), "Later keeps it available.");
        }

        [Test]
        public void BuyingPoolActivatesRelaxFromTheNextWave()
        {
            ResortSession session = TestResort.NewSession();
            session.State.completedWaves = 3;
            Assert.IsNull(session.Forecast().Find(n => n.need == NeedId.Relax), "WR9: no Relax before Pool is bought.");
            session.State.coins = 1000;
            Assert.IsFalse(session.Unlock(BuildingId.Pool), "Available but not affordable.");
            session.State.coins = 1500;
            int value = session.State.resortValue;
            Assert.IsTrue(session.Unlock(BuildingId.Pool));
            Assert.AreEqual(300, session.State.coins);
            Assert.AreEqual(value + 150, session.State.resortValue);
            Assert.AreEqual(8, session.Forecast().Find(n => n.need == NeedId.Relax).DemandTotal);
        }
    }
}
