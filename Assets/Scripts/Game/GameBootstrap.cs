using UnityEngine;

namespace Prichut.Resort.Game
{
    // Hra sa do sceny Main.unity prida pri spusteni, scenu netreba rucne upravovat.
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Object.FindAnyObjectByType<ResortGameController>() != null) return;
            if (GameObject.Find("Buildings") == null) return;
            new GameObject("ResortGame").AddComponent<ResortGameController>();
        }
    }
}
