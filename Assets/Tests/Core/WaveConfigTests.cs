using NUnit.Framework;

namespace Prichut.Resort.Tests
{
    public sealed class WaveConfigTests
    {
        [Test]
        public void DefaultConfigKeepsHardRules()
        {
            CollectionAssert.IsEmpty(ResortConfig.CreateDefault().Validate());
        }

        [Test]
        public void DemandPerNeedAndWaveMatchesBuildTable()
        {
            var config = ResortConfig.CreateDefault();
            Assert.AreEqual(8, config.WaveCount);
            Assert.AreEqual(8, config.WaveAt(1).Demand(NeedId.Rooms));
            Assert.AreEqual(16, config.WaveAt(2).Demand(NeedId.Drinks));
            Assert.AreEqual(8, config.WaveAt(4).Demand(NeedId.Relax), "Relax debuts softly in W4.");
            Assert.AreEqual(10, config.WaveAt(7).Demand(NeedId.OceanView), "Ocean View debuts softly in W7.");
            Assert.AreEqual(0, config.WaveAt(6).Demand(NeedId.OceanView));
            Assert.AreEqual(35, config.WaveAt(8).Demand(NeedId.Drinks));
        }

        [Test]
        public void DebutWaveWithGrowingNeedIsReported()
        {
            var config = ResortConfig.CreateDefault();
            config.waves[3].rooms = 30;
            CollectionAssert.IsNotEmpty(config.Validate());
        }

        [Test]
        public void EightWavesThenFinaleThenFreePlay()
        {
            var config = ResortConfig.CreateDefault();
            ResortSession session = TestResort.NewSession(config);
            session.State.coins = 100000;
            for (int wave = 1; wave <= 9; wave++)
            {
                Assert.AreEqual(wave, session.NextWaveNumber);
                Assert.IsTrue(session.StartWave());
                TestResort.RunUntil(session, WavePhase.WaveComplete, 3600f);
                Assert.AreEqual(WavePhase.WaveComplete, session.Phase, "Wave " + wave + " must finish.");
                Assert.AreEqual(wave == 8, session.LastResult.isFinale);
                session.ClaimRewards();
                session.FinishUpgradeStep();
            }
            Assert.AreSame(config.WaveAt(8), config.WaveAt(9), "Free play repeats the last wave.");
        }
    }
}
