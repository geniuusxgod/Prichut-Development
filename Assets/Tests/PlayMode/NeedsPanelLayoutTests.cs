using System.Collections;
using System.IO;
using NUnit.Framework;
using Prichut.Resort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Prichut.Resort.Tests
{
    public sealed class NeedsPanelLayoutTests
    {
        [UnityTest]
        public IEnumerator FiveNeedsAndFontAreAvailable()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;
            Assert.IsNotNull(Resources.Load<Font>("Fonts/Fredoka-Medium"));
            var game = Object.FindAnyObjectByType<ResortGameController>();
            game.Session.State.Building(BuildingId.Pool).owned = true;
            game.Session.State.Building(BuildingId.Bungalows).owned = true;
            game.Session.State.completedWaves = 7;
            game.Session.StartWave();
            game.Session.Tick(4f);
            yield return null;
            yield return null;
            Assert.AreEqual(5, game.Session.Wave.needs.Count);
            var panel = Object.FindAnyObjectByType<NeedsPanel>();
            var rows = panel.transform.Find("NeedsPanel/Rows");
            Assert.AreEqual(5, rows.childCount);
            Canvas.ForceUpdateCanvases();
            var camera = Camera.main;
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var modes = new RenderMode[canvases.Length];
            var cameras = new Camera[canvases.Length];
            var distances = new float[canvases.Length];
            var target = RenderTexture.GetTemporary(1280, 720, 24);
            var oldTarget = camera.targetTexture;
            var oldActive = RenderTexture.active;
            var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                for (int i = 0; i < canvases.Length; i++)
                {
                    modes[i] = canvases[i].renderMode;
                    cameras[i] = canvases[i].worldCamera;
                    distances[i] = canvases[i].planeDistance;
                    canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                    canvases[i].worldCamera = camera;
                    canvases[i].planeDistance = 1f;
                }
                camera.targetTexture = target;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                image.Apply();
                File.WriteAllBytes(Path.GetFullPath(Application.dataPath + "/../../PRIH-79-panel.png"), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = oldTarget;
                RenderTexture.active = oldActive;
                for (int i = 0; i < canvases.Length; i++)
                {
                    canvases[i].renderMode = modes[i];
                    canvases[i].worldCamera = cameras[i];
                    canvases[i].planeDistance = distances[i];
                }
                RenderTexture.ReleaseTemporary(target);
                Object.Destroy(image);
            }
        }
    }
}
