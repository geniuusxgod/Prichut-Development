namespace Prichut.Resort
{
    // Stavy core loopu (Build §4.3). Pokojovy stav je AWAITING_WAVE, nikdy "idle".
    public enum WavePhase
    {
        AwaitingWave = 0,
        WaveIncoming = 1,
        WaveActive = 2,
        WaveResolving = 3,
        WaveComplete = 4,
        Upgrade = 5,
        WavePaused = 6
    }

    // Povolene akcie podla stavu. Vstupy (UI) sa pytaju tu, nemaju vlastne booleany.
    public static class WavePhaseRules
    {
        public static bool CanStartWave(WavePhase phase) => phase == WavePhase.AwaitingWave;

        public static bool IsWaveRunning(WavePhase phase) =>
            phase == WavePhase.WaveIncoming || phase == WavePhase.WaveActive;

        public static bool CanPause(WavePhase phase) => IsWaveRunning(phase);

        public static bool CanMoveWorker(WavePhase phase) =>
            phase == WavePhase.AwaitingWave || IsWaveRunning(phase);

        public static bool CanUpgrade(WavePhase phase) =>
            phase == WavePhase.AwaitingWave || phase == WavePhase.Upgrade;
    }
}
