using NUnit.Framework;
using Prichut.Resort.Game;

namespace Prichut.Resort.Tests
{
    public sealed class HiddenSatisfactionTests
    {
        [Test]
        public void PanelTextsDuringWaveNeverShowAPercentage()
        {
            var session = new ResortSession(ResortConfig.CreateDefault());
            session.StartWave();
            for (int i = 0; i < 900; i++)
            {
                session.Tick(0.1f);
                foreach (NeedRun run in session.Wave.needs)
                {
                    StringAssert.DoesNotContain("%", NeedsPanel.CountText(run));
                    StringAssert.DoesNotContain("%", NeedsPanel.WaitingText(run, session.CapacityOf(run.Building)));
                }
            }
        }

        [Test]
        public void SatisfactionIsMeasuredInternallyAndShownOnResult()
        {
            var session = new ResortSession(ResortConfig.CreateDefault());
            session.StartWave();
            while (session.Phase != WavePhase.WaveComplete) session.Tick(0.1f);
            Assert.AreEqual(session.CurrentSatisfaction, session.LastResult.satisfaction, 0.0001f);
            Assert.That(session.LastResult.satisfaction, Is.InRange(0.4f, 1f));
        }
    }
}
