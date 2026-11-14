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
