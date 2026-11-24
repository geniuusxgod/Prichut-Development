using System.Collections;
using System.IO;
using NUnit.Framework;
using Prichut.Resort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Prichut.Resort.Tests
{
    public sealed class BuildingLevelVisualTests
    {
        [UnityTest]
        public IEnumerator LevelsStayScaledAfterSelectionAndDeselection()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;
            var game = Object.FindAnyObjectByType<ResortGameController>();
            var views = Object.FindObjectsByType<BuildingView>(FindObjectsSortMode.None);
            var view = System.Array.Find(views, item => item.Id == BuildingId.Reception);
            Assert.IsNotNull(view);
            Vector3 original = view.transform.localScale;
            foreach (int level in new[] { 2, 3, 1 })
            {
                game.Session.State.Building(view.Id).level = level;
                yield return null;
                yield return null;
                Vector3 expected = original * BuildingLevelVisual.ScaleFor(level);
                Assert.Less(Vector3.Distance(expected, view.transform.localScale), 0.001f);
                var glow = view.transform.Find("LevelGlow").GetComponent<SpriteRenderer>();
                Assert.AreEqual(level > 1, glow.enabled);
                game.Select(view.Id);
                yield return new WaitForSecondsRealtime(0.6f);
                Assert.Less(Vector3.Distance(expected, view.transform.localScale), 0.001f,
                    "Selection bounce must settle at the upgraded scale.");
                float focusedSize = Camera.main.orthographicSize;
                game.Select(null);
                yield return new WaitForSecondsRealtime(1f);
                Assert.Less(Vector3.Distance(expected, view.transform.localScale), 0.001f);
                Assert.Greater(Camera.main.orthographicSize, focusedSize,
                    "Deselecting must return the camera towards the full island view.");
            }

            game.Session.State.Building(BuildingId.Reception).level = 3;
            game.Session.State.Building(BuildingId.Restaurant).level = 2;
            yield return null;
            yield return null;
            var camera = Camera.main;
            var target = RenderTexture.GetTemporary(1280, 720, 24);
            var previous = camera.targetTexture;
            var active = RenderTexture.active;
            var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                image.Apply();
                File.WriteAllBytes(Path.GetFullPath(Application.dataPath + "/../../PRIH-49-levels.png"), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previous;
                RenderTexture.active = active;
                RenderTexture.ReleaseTemporary(target);
                Object.Destroy(image);
            }
        }
    }
}
