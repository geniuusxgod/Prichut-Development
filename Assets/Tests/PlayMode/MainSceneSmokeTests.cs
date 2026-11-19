using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Prichut.Resort.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Prichut.Resort.Tests
{
    // Dymovy test hlavnej sceny: hra sa nainstaluje, Start Wave je klikatelne cez EventSystem.
    public sealed class MainSceneSmokeTests
    {
        [UnityTest]
        public IEnumerator StartWaveButtonIsClickableThroughEventSystem()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;
            var game = Object.FindAnyObjectByType<ResortGameController>();
            Assert.IsNotNull(game, "Bootstrap must install the game.");
            Assert.IsNotNull(EventSystem.current, "EventSystem must exist.");

            GameObject button = GameObject.Find("StartWave");
            Assert.IsNotNull(button);
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, button.transform.position);
            var data = new PointerEventData(EventSystem.current) { position = screen };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, hits);
            string names = string.Join(", ", hits.ConvertAll(h => h.gameObject.name + "@" + h.module.GetType().Name));
            Debug.Log("Raycast at " + screen + ": " + names);
            Assert.IsTrue(hits.Count > 0, "Nothing hit at the button position.");
            Assert.IsTrue(hits[0].gameObject == button || hits[0].gameObject.transform.IsChildOf(button.transform),
                "Top hit is " + names);
        }
    }
}
