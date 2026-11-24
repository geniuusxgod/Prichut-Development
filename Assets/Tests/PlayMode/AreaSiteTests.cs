using System.Collections;
using NUnit.Framework;
using Prichut.Resort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Prichut.Resort.Tests
{
    public sealed class AreaSiteTests
    {
        [UnityTest]
        public IEnumerator SitesShowGatesAndOnlyAvailableSitesRequestPurchase()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;
            var game = Object.FindAnyObjectByType<ResortGameController>();
            Assert.AreEqual(2, Object.FindObjectsByType<AreaSiteView>(FindObjectsSortMode.None).Length);
            Assert.IsNotNull(Resources.Load<Sprite>("Game/site"));
            Assert.IsNotNull(Resources.Load<Sprite>("Game/icon_lock"));
            int requests = 0;
            game.AreaUnlockRequested += (area, fromFlow) => requests++;
            game.RequestAreaUnlock(BuildingId.Pool);
            Assert.AreEqual(0, requests);
            game.Session.State.completedWaves = 3;
            yield return null;
            yield return null;
            game.RequestAreaUnlock(BuildingId.Pool);
            Assert.AreEqual(1, requests);
            Assert.AreEqual(AreaState.Available, game.Session.AreaStateOf(BuildingId.Pool));
            Assert.IsFalse(game.Session.State.Owns(BuildingId.Pool));
            string labels = string.Join("\n", System.Array.ConvertAll(
                Object.FindObjectsByType<Text>(FindObjectsSortMode.None), text => text.text));
            StringAssert.Contains("Pool - tap to unlock", labels);
            StringAssert.Contains("Bungalows after Wave 6", labels);
            game.Session.StartWave();
            game.RequestAreaUnlock(BuildingId.Pool);
            Assert.AreEqual(1, requests);
        }
    }
}
