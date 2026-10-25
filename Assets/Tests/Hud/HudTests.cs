using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class HudTests
{
    private GameObject root;

    [TearDown]
    public void TearDown()
    {
        if (root != null) Object.DestroyImmediate(root);
    }

    [UnityTest]
    public IEnumerator ValuesUpdateImmediatelyAndAfterReopeningHud()
    {
        root = new GameObject("HUD test");
        var state = root.AddComponent<GameState>();
        var panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform);
        panel.SetActive(false);
        Text coins = CreateLabel(panel, "Coins");
        Text energy = CreateLabel(panel, "Energy");
        Text resort = CreateLabel(panel, "Resort Value");
        var presenter = panel.AddComponent<HudPresenter>();
        Wire(presenter, "gameState", state);
        Wire(presenter, "coinsText", coins);
        Wire(presenter, "energyText", energy);
        Wire(presenter, "resortValueText", resort);
        panel.SetActive(true);
        yield return null;

        Assert.AreEqual("12,450", coins.text);
        Assert.AreEqual("45/50", energy.text);
        Assert.AreEqual("Resort Value: 700", resort.text);
        state.SetCoins(12345);
        Assert.AreEqual("12,345", coins.text);
        state.SetEnergy(27);
        Assert.AreEqual("27/50", energy.text);
        state.SetResortValue(9876);
        Assert.AreEqual("Resort Value: 9,876", resort.text);
        Assert.AreEqual(12345, state.Coins, "Presentation must not mutate the wallet.");

        for (int i = 0; i < 3; i++)
        {
            panel.SetActive(false);
            state.SetCoins(100 + i);
            Assert.AreNotEqual((100 + i).ToString(), coins.text, "Disabled HUD should be unsubscribed.");
            state.SetEnergy(30 + i);
            state.SetResortValue(900 + i);
            panel.SetActive(true);
            Assert.AreEqual((100 + i).ToString(), coins.text);
            Assert.AreEqual((30 + i).ToString() + "/50", energy.text);
            Assert.AreEqual("Resort Value: " + (900 + i), resort.text);
        }

        state.SetCoins(int.MaxValue);
        Assert.AreEqual("2,147,483,647", coins.text);
        state.SetEnergyCapacity(75);
        Assert.AreEqual("32/75", energy.text);
        Object.Destroy(panel);
        yield return null;
        Assert.DoesNotThrow(() => state.SetCoins(1), "Destroyed HUD must be unsubscribed.");
    }

    [TestCase(1920, 1080, 0, 0, 1920, 1080)]
    [TestCase(2340, 1080, 100, 30, 2140, 1050)]
    [TestCase(2556, 1179, 177, 63, 2202, 1116)]
    [TestCase(1280, 720, 60, 20, 1220, 700)]
    public void SafeAreaAnchorsStayInsideLandscapeScreen(int width, int height,
        float x, float y, float safeWidth, float safeHeight)
    {
        Rect anchors = HudSafeArea.Normalize(new Rect(x, y, safeWidth, safeHeight), width, height);
        Assert.That(anchors.xMin * width, Is.EqualTo(x).Within(0.001f));
        Assert.That(anchors.yMin * height, Is.EqualTo(y).Within(0.001f));
        Assert.That(anchors.xMax * width, Is.EqualTo(x + safeWidth).Within(0.001f));
        Assert.That(anchors.yMax * height, Is.EqualTo(y + safeHeight).Within(0.001f));
    }

    [Test]
    public void ZeroSizedScreenDoesNotProduceInvalidAnchors()
    {
        Assert.AreEqual(new Rect(0, 0, 1, 1), HudSafeArea.Normalize(Rect.zero, 0, 0));
    }

    private static Text CreateLabel(GameObject parent, string name)
    {
        var label = new GameObject(name, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        label.transform.SetParent(parent.transform, false);
        return label;
    }

    private static void Wire(HudPresenter presenter, string name, Object value)
    {
        typeof(HudPresenter).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(presenter, value);
    }
}
