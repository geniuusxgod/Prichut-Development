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
    }
}
