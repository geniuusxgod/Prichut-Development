using NUnit.Framework;

namespace Prichut.Resort.Tests
{
    public sealed class StartWaveTests
    {
        [Test]
        public void StartIsOnlyPossibleInAwaitingWave()
        {
            ResortSession session = TestResort.NewSession();
            Assert.IsTrue(session.CanStartWave);
            Assert.IsTrue(session.StartWave());
            Assert.AreEqual(WavePhase.WaveIncoming, session.Phase);
            Assert.IsFalse(session.StartWave(), "Double tap must not start a second wave.");
        }

        [Test]
        public void ActiveWaveBlocksStartUntilPreparationReturns()
        {
            ResortSession session = TestResort.NewSession();
            session.StartWave();
            TestResort.RunUntil(session, WavePhase.WaveActive);
            Assert.IsFalse(session.StartWave());
            TestResort.RunUntil(session, WavePhase.WaveComplete);
            Assert.IsFalse(session.StartWave());
            Assert.IsTrue(session.ClaimRewards());
            Assert.IsFalse(session.StartWave(), "Upgrade step comes before the next wave.");
            Assert.IsTrue(session.FinishUpgradeStep());
            Assert.IsTrue(session.StartWave());
            Assert.AreEqual(2, session.Wave.waveNumber);
        }

        [Test]
        public void PauseKeepsThePreviousPhase()
        {
            ResortSession session = TestResort.NewSession();
            session.StartWave();
            TestResort.RunUntil(session, WavePhase.WaveActive);
            TestResort.Run(session, 10f);
            float time = session.Wave.time;
            Assert.IsTrue(session.Pause());
            TestResort.Run(session, 30f);
            Assert.AreEqual(time, session.Wave.time, 0.0001f, "Nothing advances while paused.");
            Assert.IsTrue(session.Resume());
            Assert.AreEqual(WavePhase.WaveActive, session.Phase);
        }
    }
}
