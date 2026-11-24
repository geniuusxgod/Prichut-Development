using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // Kozmeticka animacia ziskania Coins (PRIH-153): spusti sa az po potvrdenej transakcii
    // a mince doletia k pocitadlu Coins v HUD. Zostatok nemeni, ten uz zmenil ClaimRewards.
    public sealed class CoinBurst : GameView
    {
        private const int CoinCount = 10;
        private const float Duration = 0.9f;

        private sealed class Coin
        {
            public RectTransform rect;
            public Vector2 from;
            public Vector2 control;
            public Vector2 to;
            public float delay;
            public float t;
        }

        private readonly List<Coin> coins = new List<Coin>();
        private RectTransform root;

        protected override void Build()
        {
            root = (RectTransform)transform;
            UiKit.Stretch(root);
        }

        protected override void OnResortEvent(ResortEvent e)
        {
            if (e.type == ResortEventType.RewardsClaimed && e.value > 0) Play();
        }

        private void Play()
        {
            Vector2 target = HudCoinsPosition();
            for (int i = 0; i < CoinCount; i++)
            {
                Image image = UiKit.Icon(root, "coin", 40f);
                RectTransform rect = image.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                Vector2 from = new Vector2(Random.Range(-140f, 140f), Random.Range(-60f, 40f));
                coins.Add(new Coin
                {
                    rect = rect,
                    from = from,
                    control = from + new Vector2(Random.Range(-160f, 160f), Random.Range(120f, 220f)),
                    to = target,
                    delay = i * 0.05f
                });
                rect.anchoredPosition = from;
            }
        }

        // Pozicia karty Coins z HUD (PRIH-12) v suradniciach tejto vrstvy.
        private Vector2 HudCoinsPosition()
        {
            GameObject card = GameObject.Find("CoinsCard");
            Vector2 fallback = new Vector2(root.rect.width * 0.3f, root.rect.height * 0.45f);
            if (card == null) return fallback;
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, card.transform.position);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, null, out Vector2 local) ? local : fallback;
        }

        private void Update()
        {
            for (int i = coins.Count - 1; i >= 0; i--)
            {
                Coin coin = coins[i];
                if (coin.delay > 0f)
                {
                    coin.delay -= Time.unscaledDeltaTime;
                    continue;
                }

                coin.t += Time.unscaledDeltaTime / Duration;
                float t = Mathf.Clamp01(coin.t);
                float e = t * t;
                Vector2 a = Vector2.Lerp(coin.from, coin.control, e);
                Vector2 b = Vector2.Lerp(coin.control, coin.to, e);
                coin.rect.anchoredPosition = Vector2.Lerp(a, b, e);
                coin.rect.localScale = Vector3.one * Mathf.Lerp(1.2f, 0.6f, t);
                if (t >= 1f)
                {
                    Destroy(coin.rect.gameObject);
                    coins.RemoveAt(i);
                }
            }
        }
    }
}
