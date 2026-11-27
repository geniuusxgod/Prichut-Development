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

        [Test]
        public void PoolOfferAppearsAfterWaveThreeAndCanBeBought()
        {
            ResortSession session = TestResort.NewSession();
            session.State.coins = 2000;
            for (int wave = 1; wave <= 3; wave++)
            {
                Assert.IsFalse(session.CanUnlock(BuildingId.Pool), "Locked before Wave 3 is completed.");
                session.StartWave();
                TestResort.RunUntil(session, WavePhase.WaveComplete);
                session.ClaimRewards();
                if (wave < 3) session.FinishUpgradeStep();
            }

            Assert.AreEqual(BuildingId.Pool, session.PendingAreaOffer, "S9 follows S5 after Wave 3.");
            Assert.IsTrue(session.Unlock(BuildingId.Pool));
            session.FinishUpgradeStep();
            session.StartWave();
            Assert.IsNotNull(session.Wave.Find(NeedId.Relax), "Wave 4 brings Relax.");
        }

        [Test]
        public void BungalowsUseTheSameFlowASecondTime()
        {
            ResortSession session = TestResort.NewSession();
            session.State.completedWaves = 6;
            session.State.coins = 10000;
            session.MarkAreaOffered(BuildingId.Pool);
            Assert.AreEqual(BuildingId.Bungalows, session.PendingAreaOffer);
            Assert.IsTrue(session.Unlock(BuildingId.Bungalows));
            Assert.AreEqual(AreaState.Unlocked, session.AreaStateOf(BuildingId.Bungalows));
            Assert.AreEqual(AreaState.Available, session.AreaStateOf(BuildingId.Pool), "Pool can still be bought later.");
            Assert.IsNotNull(session.Forecast().Find(n => n.need == NeedId.OceanView));
        }

        [Test]
        public void SkippedAreaCanBeBoughtLaterAndWavesStillWork()
        {
            ResortSession session = TestResort.NewSession();
            session.State.completedWaves = 3;
            session.MarkAreaOffered(BuildingId.Pool);
            session.StartWave();
            Assert.IsNull(session.Wave.Find(NeedId.Relax), "WR9: unbought Pool spawns no Relax.");
            TestResort.RunUntil(session, WavePhase.WaveComplete);
            session.ClaimRewards();
            session.FinishUpgradeStep();
            session.State.coins = 1200;
            Assert.IsTrue(session.CanUnlock(BuildingId.Pool), "Later purchase from the site in AWAITING_WAVE.");
            Assert.IsTrue(session.Unlock(BuildingId.Pool));
        }
    }
}
