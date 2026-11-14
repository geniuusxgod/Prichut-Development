using System;

namespace Prichut.Resort
{
    // Obsluha jednej budovy: rad jednotiek needu + capacity subeznych miest (Build §4.4).
    public static class ServiceSystem
    {
        // Nova jednotka sa postavi na koniec radu; ak je volne miesto, hned si sadne (cakanie 0 s).
        public static void Arrive(NeedRun run, float time, int capacity)
        {
            run.queue.Add(time);
            SeatWaiting(run, time, capacity);
        }

        // Posunie vsetky rozbehnute obsluhy o speed * dt. Dokoncena obsluha zvysi filled,
        // uvolni miesto a prijme dalsiu jednotku z radu. Vrati pocet dokoncenych jednotiek.
        public static int Tick(NeedRun run, float time, float dt, float speed, int capacity)
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

            if (completed > 0) SeatWaiting(run, time, capacity);
            return completed;
        }

        // Volne miesto obsadi vzdy prva cakajuca jednotka (FIFO).
        public static int SeatWaiting(NeedRun run, float time, int capacity)
        {
            int seated = 0;
            while (run.queue.Count > 0 && run.seats.Count < capacity)
            {
                run.queue.RemoveAt(0);
                run.seats.Add(0f);
                seated++;
            }
            return seated;
        }
    }
}
