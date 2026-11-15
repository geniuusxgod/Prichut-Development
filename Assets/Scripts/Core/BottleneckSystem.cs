namespace Prichut.Resort
{
    // Stavy needu (Build §4.4): LOCKED need sa vobec nevytvara, preto tu nie je.
    public enum NeedStatus
    {
        Ok = 0,
        Bottleneck = 1,
        Recovering = 2,
        Satisfied = 3
    }

    // OK -> BOTTLENECK: queue > threshold a rastie aspon hysteresis_s.
    // BOTTLENECK -> RECOVERING: queue sa zmensuje aspon hysteresis_s (alebo klesne pod prah).
    // RECOVERING -> OK: queue <= threshold. RECOVERING -> BOTTLENECK: queue znova rastie hysteresis_s.
    // OK/RECOVERING -> SATISFIED: prichod uzavrety, nikto necaka ani nesedi.
    // Hysterezia brani blikaniu, ked rad kolise okolo prahu.
    public static class BottleneckSystem
    {
        public static NeedStatus Update(NeedRun run, ResortConfig config, float dt, bool staffed, bool arrivalClosed)
        {
            int queue = run.Waiting;
            if (run.status == NeedStatus.Bottleneck)
            {
                run.bottleneckSeconds += dt;
                if (!staffed) run.bottleneckUnstaffedSeconds += dt;
            }

            if (run.status == NeedStatus.Satisfied) return run.status;
            if (arrivalClosed && run.Backlog == 0 && run.Waiting == 0 && run.Seated == 0)
                return Enter(run, NeedStatus.Satisfied, queue);

            switch (run.status)
            {
                case NeedStatus.Ok:
                    TrackRise(run, queue, dt);
                    if (queue > config.bottleneckThreshold && run.riseTimer >= config.bottleneckHysteresis)
                        return Enter(run, NeedStatus.Bottleneck, queue);
                    break;

                case NeedStatus.Bottleneck:
                    if (queue >= run.highWatermark)
                    {
                        run.highWatermark = queue;
                        run.fallTimer = 0f;
                    }
                    else
                    {
                        run.fallTimer += dt;
                    }

                    if (queue <= config.bottleneckThreshold || run.fallTimer >= config.bottleneckHysteresis)
                        return Enter(run, NeedStatus.Recovering, queue);
                    break;

                case NeedStatus.Recovering:
                    if (queue <= config.bottleneckThreshold) return Enter(run, NeedStatus.Ok, queue);
                    TrackRise(run, queue, dt);
                    if (run.riseTimer >= config.bottleneckHysteresis)
                        return Enter(run, NeedStatus.Bottleneck, queue);
                    break;
            }

            return run.status;
        }

        public static NeedStatus Enter(NeedRun run, NeedStatus status, int queue)
        {
            run.status = status;
            run.riseTimer = 0f;
            run.fallTimer = 0f;
            run.lowWatermark = queue;
            run.highWatermark = queue;
            return status;
        }

        private static void TrackRise(NeedRun run, int queue, float dt)
        {
            if (queue < run.lowWatermark)
            {
                run.lowWatermark = queue;
                run.riseTimer = 0f;
            }
            else if (queue > run.lowWatermark)
            {
                run.riseTimer += dt;
            }
            else
            {
                run.riseTimer = 0f;
            }
        }
    }
}
