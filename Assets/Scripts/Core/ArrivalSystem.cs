using System;

namespace Prichut.Resort
{
    // Rovnomerny prichod demandu: arrival_rate = demand_total / wave_timer (Build §4.4).
    // Akumulator drzi zlomok jednotky, takze po pauze pokracuje rovnaky zvysok.
    public static class ArrivalSystem
    {
        public static float Rate(NeedRun run, ResortConfig config)
        {
            return config.waveTimer > 0f ? run.baseTotal / config.waveTimer : run.baseTotal;
        }

        // Vrati pocet jednotiek, ktore v tomto kroku prisli.
        public static int Tick(NeedRun run, ResortConfig config, float dt)
        {
            int remaining = run.baseTotal - run.regularArrived;
            if (remaining <= 0 || dt <= 0f) return 0;
            run.arrivalAccumulator += Rate(run, config) * dt;
            int count = Math.Min(remaining, (int)Math.Floor(run.arrivalAccumulator + 1e-4f));
            if (count <= 0) return 0;
            run.arrivalAccumulator -= count;
            run.regularArrived += count;
            return count;
        }

        // Pri uzavreti prichodu sa uvolni presny koncovy pocet (zaokruhlenie float nesmie stratit jednotku).
        public static int Flush(NeedRun run)
        {
            int remaining = run.baseTotal - run.regularArrived;
            if (remaining <= 0) return 0;
            run.regularArrived += remaining;
            run.arrivalAccumulator = 0f;
            return remaining;
        }
    }
}
