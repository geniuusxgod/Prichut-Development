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
    }
}
