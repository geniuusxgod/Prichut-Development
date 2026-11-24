using System;
using System.Collections.Generic;

namespace Prichut.Resort
{
    // Data jednej beziacej vlny. Vsetko je serializovatelne, aby sa dala vlna ulozit a obnovit.
    [Serializable]
    public sealed class WaveRun
    {
        public int waveNumber;
        public int guests;
        public float time;
        public bool arrivalClosed;
        public List<NeedRun> needs = new List<NeedRun>();

        public NeedRun Find(NeedId need)
        {
            for (int i = 0; i < needs.Count; i++)
                if (needs[i].need == need) return needs[i];
            return null;
        }
    }
}
