using System.Collections;
using NUnit.Framework;
using Prichut.Resort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Prichut.Resort.Tests
{
    public sealed class UpgradeWindowValidation
    {
        [UnityTest]
        public IEnumerator OpensPurchasesAndClosesThroughButtons()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;
            var game = Object.FindAnyObjectByType<ResortGameController>();
            var sheet = Object.FindAnyObjectByType<UpgradeSheet>();
            Assert.IsNotNull(sheet);
            game.Session.State.coins = 2000;
            game.RequestUpgrade(BuildingId.Restaurant);
            Assert.IsTrue(sheet.IsOpen);
            var buy = sheet.transform.Find("Card/UpgradeNow").GetComponent<Button>();
            buy.onClick.Invoke();
            Assert.AreEqual(2, game.Session.State.Building(BuildingId.Restaurant).level);
            Assert.AreEqual(1500, game.Session.State.coins);
            Assert.IsFalse(sheet.IsOpen);
            game.RequestUpgrade(BuildingId.Restaurant);
            sheet.transform.Find("Card/Later").GetComponent<Button>().onClick.Invoke();
            Assert.IsFalse(sheet.IsOpen);
            Assert.AreEqual(1500, game.Session.State.coins);
            game.Session.StartWave();
            game.RequestUpgrade(BuildingId.Restaurant);
            Assert.IsFalse(sheet.IsOpen);
        }
    }
}
