using Prichut.Resort;

namespace Prichut.Resort.Tests
{
    // Pomocne funkcie pre testy: rychle prehratie vlny bez Unity sceny.
    public static class TestResort
    {
        public static ResortSession NewSession(ResortConfig config = null)
        {
            return new ResortSession(config ?? ResortConfig.CreateDefault());
        }

        public static void RunUntil(ResortSession session, WavePhase phase, float maxSeconds = 1200f, float step = 0.1f)
        {
            float elapsed = 0f;
            while (session.Phase != phase && elapsed < maxSeconds)
            {
                session.Tick(step);
                elapsed += step;
            }
        }

        public static void Run(ResortSession session, float seconds, float step = 0.1f)
        {
            for (float t = 0f; t < seconds; t += step) session.Tick(step);
        }
    }
}
