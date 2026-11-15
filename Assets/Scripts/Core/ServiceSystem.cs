using System;

namespace Prichut.Resort
{
    // Obsluha jednej budovy: rad jednotiek needu + capacity subeznych miest (Build §4.4).
    public static class ServiceSystem
    {
        // Nova jednotka sa postavi na koniec radu; ak je volne miesto, hned si sadne (cakanie 0 s).
        public static void Arrive(NeedRun run, float time, int capacity, ResortConfig config)
        {
            run.queue.Add(time);
            SeatWaiting(run, time, capacity, config);
        }

        // Posunie vsetky rozbehnute obsluhy o speed * dt. Dokoncena obsluha zvysi filled,
        // uvolni miesto a prijme dalsiu jednotku z radu. Vrati pocet dokoncenych jednotiek.
        public static int Tick(NeedRun run, float time, float dt, float speed, int capacity, ResortConfig config)
        {
            if (dt <= 0f || speed <= 0f) return 0;
            int completed = 0;
            for (int i = run.seats.Count - 1; i >= 0; i--)
            {
                float progress = run.seats[i] + speed * dt;
                if (progress >= 1f)
                {
                    run.seats.RemoveAt(i);
                    run.filled++;
                    completed++;
                }
                else
                {
                    run.seats[i] = progress;
                }
            }

            if (completed > 0) SeatWaiting(run, time, capacity, config);
            return completed;
        }

        // Volne miesto obsadi vzdy prva cakajuca jednotka (FIFO). V momente usadenia sa zapise
        // jej cakanie (prichod -> usadenie); samotna obsluha sa do cakania nepocita.
        public static int SeatWaiting(NeedRun run, float time, int capacity, ResortConfig config)
        {
            int seated = 0;
            while (run.queue.Count > 0 && run.seats.Count < capacity)
            {
                float wait = Math.Max(0f, time - run.queue[0]);
                run.queue.RemoveAt(0);
                run.seats.Add(0f);
                run.contributionSum += SatisfactionRules.Contribution(wait, config);
                run.contributionCount++;
                if (wait > run.longestWait) run.longestWait = wait;
                seated++;
            }
            return seated;
        }
    }
}
