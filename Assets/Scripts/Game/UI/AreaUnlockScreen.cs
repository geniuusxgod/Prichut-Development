using UnityEngine;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // S9 Area Unlock (PRIH-39/40): jeden template, dva triggery (Pool po Wave 3, Bungalows po Wave 6).
    // Je to oslava rastu, full screen moment. Worker je sucast balika (Includes: 1 Worker).
    public sealed class AreaUnlockScreen : ModalView
    {
        private BuildingId area;
        private bool fromFlow;
        private Text heading;
        private Text title;
        private Image picture;
        private Text adds;
        private Text cost;
        private Text missing;
        private Button unlock;
        private Image glow;

        protected override void Build()
        {
            BuildModal(new Vector2(820f, 500f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), false);
            Transform card = Card.transform;
            glow = UiKit.Fill(card, "Accent", new Color32(255, 214, 120, 255));
            glow.raycastTarget = false;
            UiKit.Place(glow.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(780f, 8f));

            heading = UiKit.Label(card, "New Area Available", 22, UiKit.Muted, TextAnchor.MiddleCenter);
            UiKit.Place(heading.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(700f, 30f));
            title = UiKit.Label(card, "", 40, UiKit.Ink, TextAnchor.MiddleCenter);
            UiKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(700f, 50f));

            picture = UiKit.Icon(card, "pool", 200f);
            UiKit.Place(picture.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -120f), new Vector2(300f, 210f));

            adds = UiKit.Label(card, "", 21, UiKit.Ink, TextAnchor.UpperLeft, FontStyle.Normal);
            adds.supportRichText = true;
            UiKit.Place(adds.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(370f, -126f), new Vector2(420f, 170f));
            cost = UiKit.Label(card, "", 24, UiKit.Ink, TextAnchor.MiddleLeft);
            UiKit.Place(cost.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(370f, -300f), new Vector2(420f, 34f));
            missing = UiKit.Label(card, "", 19, UiKit.Warning, TextAnchor.MiddleLeft);
            UiKit.Place(missing.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(370f, -334f), new Vector2(420f, 28f));

            unlock = UiKit.Button(card, "Unlock", "Unlock", "ui_green", "icon_play", OnUnlock, 28);
            UiKit.Place((RectTransform)unlock.transform, new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(-10f, 22f), new Vector2(260f, 70f));
            Button later = UiKit.Button(card, "Later", "Later", "ui_grey", null, OnLater, 24);
            UiKit.Place((RectTransform)later.transform, new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(10f, 22f), new Vector2(200f, 70f));

            Game.AreaUnlockRequested += OpenFor;
        }

        protected override void OnDestroy()
        {
            if (Game != null) Game.AreaUnlockRequested -= OpenFor;
            base.OnDestroy();
        }

        // fromFlow = otvorene z toku po S5 (Later vedie na S1); inak z tuknutia na stavenisko.
        public void OpenFor(BuildingId building, bool flow)
        {
            area = building;
            fromFlow = flow;
            Session.MarkAreaOffered(building);
            SetOpen(true);
            Refresh();
        }

        protected override void OnSessionChanged()
        {
            if (IsOpen) Refresh();
        }

        // Stavy: locked / available / not affordable (Missing: X Coins) / unlocked.
        private void Refresh()
        {
            bool second = area == BuildingId.Bungalows;
            title.text = ResortCatalog.BuildingName(area);
            heading.text = second ? "Your resort keeps growing" : "New Area Available";
            glow.color = second ? new Color32(120, 200, 235, 255) : new Color32(255, 214, 120, 255);
            picture.sprite = UiKit.Sprite(second ? "bungalows" : "pool");

            string need = ResortCatalog.NeedName(ResortCatalog.NeedOf(area));
            adds.text = "Adds: <b>" + need + "</b> demand\n" +
                        (second ? "Premium guests who love the view\n" : "Higher satisfaction potential\n") +
                        "Includes: <b>1 Worker</b>\n" +
                        "Resort Value <b>+" + UiKit.Number(Session.UnlockResortValue(area)) + "</b>";

            AreaState state = Session.AreaStateOf(area);
            int price = Session.UnlockCost(area);
            cost.text = "";
            int short_ = price - Session.State.coins;
            if (state == AreaState.Locked)
                missing.text = "Unlocks after Wave " + Session.GateWave(area);
            else
                missing.text = short_ > 0 ? "Missing: " + UiKit.Number(short_) + " Coins" : "";
            unlock.interactable = Session.CanUnlock(area);
        }

        private void OnUnlock()
        {
            if (!Session.Unlock(area)) return;
            Close();
            if (fromFlow) Session.FinishUpgradeStep();
            if (Game.Toasts != null) Game.Toasts.Show(ResortCatalog.BuildingName(area) + " is open - a new Worker joined!", ToastKind.Positive);
        }

        private void OnLater()
        {
            Close();
            if (fromFlow) Session.FinishUpgradeStep();
        }
    }
}
