using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prichut.Resort.Game
{
    // Hra sa do sceny Main.unity prida pri spusteni, scenu netreba rucne upravovat.
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Object.FindAnyObjectByType<ResortGameController>() != null) return;
            if (GameObject.Find("Buildings") == null) return;
            new GameObject("ResortGame").AddComponent<ResortGameController>();
        }
    }
}
