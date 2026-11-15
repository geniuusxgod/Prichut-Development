using System.Collections.Generic;
using UnityEngine;

namespace Prichut.Resort.Game
{
    // Vizual hosti (PRIH-16/109): lod pri mole, postavicky idu k budovam a cakajuci stoja v rade.
    // Postavicky su len obraz, ekonomika ani satisfaction sa z nich nepocitaju.
    public sealed class GuestCrowdView : GameView
    {
        private const int MaxWalkers = 32;
        private const int MaxQueueFigures = 8;
        private static readonly Vector3 Pier = new Vector3(-5.6f, -3.0f, 0f);
        private static readonly Vector3 BoatDock = new Vector3(-6.9f, -3.75f, 0f);
        private static readonly Vector3 BoatAway = new Vector3(-12.5f, -4.2f, 0f);

        private sealed class Walker
        {
            public Transform transform;
            public Vector3 from;
            public Vector3 to;
            public float t;
            public float duration;
        }

        private readonly List<Walker> walkers = new List<Walker>();
        private readonly Stack<SpriteRenderer> pool = new Stack<SpriteRenderer>();
        private readonly Dictionary<BuildingId, List<SpriteRenderer>> queues = new Dictionary<BuildingId, List<SpriteRenderer>>();
        private Transform boat;
        private float spawnBudget;
        private int spriteIndex;

        protected override void Build()
        {
            var boatGo = new GameObject("Boat");
            boatGo.transform.SetParent(transform, false);
            var boatRenderer = boatGo.AddComponent<SpriteRenderer>();
            boatRenderer.sprite = UiKit.Sprite("boat");
            boatRenderer.sortingOrder = 4;
            boat = boatGo.transform;
            boat.position = BoatAway;
        }

        protected override void OnResortEvent(ResortEvent e)
        {
            if (e.type == ResortEventType.UnitsArrived) SpawnWalkers(e.building, e.value);
        }

        // Pomer postaviciek k jednotkam needov = guests / jednotky vlny (hostia nesu ~2 needy).
        private void SpawnWalkers(BuildingId building, int units)
        {
            WaveRun wave = Session.Wave;
            if (wave == null || Game.Island == null || !Game.Island.Buildings.TryGetValue(building, out BuildingView view)) return;
            int totalUnits = 0;
            foreach (NeedRun run in wave.needs) totalUnits += run.DemandTotal;
            float ratio = totalUnits > 0 ? Mathf.Clamp01((float)wave.guests / totalUnits) : 0.5f;
            spawnBudget += units * ratio;
            while (spawnBudget >= 1f)
            {
                spawnBudget -= 1f;
                if (walkers.Count >= MaxWalkers) continue;
                SpriteRenderer guest = Take();
                Vector3 target = QueueSpot(view, Random.Range(0, 3)) + new Vector3(Random.Range(-0.2f, 0.2f), 0f, 0f);
                var walker = new Walker
                {
                    transform = guest.transform,
                    from = Pier + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.1f, 0.1f), 0f),
                    to = target
                };
                walker.duration = Vector3.Distance(walker.from, walker.to) / Random.Range(1.8f, 2.6f);
                walker.transform.position = walker.from;
                walkers.Add(walker);
            }
        }

        private void Update()
        {
            if (Session == null) return;
            UpdateBoat();
            UpdateWalkers();
            UpdateQueues();
        }

        private void UpdateBoat()
        {
            bool docked = Session.Phase == WavePhase.WaveIncoming ||
                          (Session.Phase == WavePhase.WaveActive && !Session.AllGuestsArrived);
            Vector3 target = docked ? BoatDock : BoatAway;
            boat.position = Vector3.MoveTowards(boat.position, target, Time.deltaTime * 4f);
            boat.position += new Vector3(0f, Mathf.Sin(Time.time * 2f) * 0.002f, 0f);
        }

        private void UpdateWalkers()
        {
            bool running = Session.Phase != WavePhase.WavePaused;
            for (int i = walkers.Count - 1; i >= 0; i--)
            {
                Walker w = walkers[i];
                if (running) w.t += Time.deltaTime / Mathf.Max(0.1f, w.duration);
                Vector3 p = Vector3.Lerp(w.from, w.to, Mathf.Clamp01(w.t));
                p.y += Mathf.Abs(Mathf.Sin(w.t * 18f)) * 0.06f;
                w.transform.position = p;
                if (w.t >= 1f)
                {
                    Give(w.transform.GetComponent<SpriteRenderer>());
                    walkers.RemoveAt(i);
                }
            }
        }

        // Rad cakajucich je viditelny pri budove: max 8 postaviciek, zvysok ukaze stitok budovy.
        private void UpdateQueues()
        {
            if (Game.Island == null) return;
            foreach (KeyValuePair<BuildingId, BuildingView> pair in Game.Island.Buildings)
            {
                NeedRun run = Session.Wave != null && WavePhaseRules.IsWaveRunning(Session.Phase) || Session.Phase == WavePhase.WavePaused
                    ? Session.Wave?.Find(ResortCatalog.NeedOf(pair.Key)) : null;
                int count = run != null ? Mathf.Min(MaxQueueFigures, run.Waiting) : 0;
                if (!queues.TryGetValue(pair.Key, out List<SpriteRenderer> list))
                {
                    list = new List<SpriteRenderer>();
                    queues[pair.Key] = list;
                }

                while (list.Count < count) list.Add(Take());
                while (list.Count > count)
                {
                    Give(list[list.Count - 1]);
                    list.RemoveAt(list.Count - 1);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    Vector3 spot = QueueSpot(pair.Value, i);
                    spot.y += Mathf.Abs(Mathf.Sin(Time.time * 2f + i)) * 0.02f;
                    list[i].transform.position = spot;
                }
            }
        }

        private static Vector3 QueueSpot(BuildingView view, int index)
        {
            Vector3 start = view.BasePoint + new Vector3(-0.9f, -0.35f, 0f);
            return start + new Vector3(-index * 0.32f, -index * 0.12f, 0f);
        }

        private SpriteRenderer Take()
        {
            SpriteRenderer r = pool.Count > 0 ? pool.Pop() : CreateGuest();
            r.gameObject.SetActive(true);
            return r;
        }

        private void Give(SpriteRenderer r)
        {
            r.gameObject.SetActive(false);
            pool.Push(r);
        }

        private SpriteRenderer CreateGuest()
        {
            var go = new GameObject("Guest");
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * 0.85f;
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = UiKit.Sprite("guest_" + (spriteIndex++ % 4));
            r.sortingOrder = 6;
            return r;
        }
    }
}
