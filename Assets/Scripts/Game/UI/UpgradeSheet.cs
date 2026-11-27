using UnityEngine;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // S5 Upgrade Suggestion (PRIH-38): spolocne okno upgradu. Otvara sa po S4 s odporucanou budovou
    // a z kontextoveho okna budovy v AWAITING_WAVE. Upgrade meni len pocet miest.
    public sealed class UpgradeSheet : ModalView
    {
        private Text heading;
        private Text building;
        private Text reason;
        private Text effect;
        private Text price;
        private Text missing;
        private Button upgrade;
        private Button later;
        private BuildingId? target;
        private bool fromFlow;

        protected override void Build()
        {
            BuildModal(new Vector2(680f, 380f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), false);
            Transform card = Card.transform;
            heading = UiKit.Label(card, "Recommended upgrade", 20, UiKit.Muted, TextAnchor.UpperCenter);
            UiKit.Place(heading.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(620f, 28f));
            building = UiKit.Label(card, "", 34, UiKit.Ink, TextAnchor.UpperCenter);
            UiKit.Place(building.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(620f, 44f));
            reason = UiKit.Label(card, "", 18, UiKit.Muted, TextAnchor.UpperCenter, FontStyle.Normal);
            UiKit.Place(reason.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -92f), new Vector2(600f, 50f));

            effect = UiKit.Label(card, "", 30, UiKit.Ink, TextAnchor.MiddleCenter);
            effect.supportRichText = true;
            UiKit.Place(effect.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(620f, 44f));
            price = UiKit.Label(card, "", 22, UiKit.Ink, TextAnchor.MiddleCenter);
            price.supportRichText = true;
            UiKit.Place(price.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -200f), new Vector2(620f, 32f));

            missing = UiKit.Label(card, "", 19, UiKit.Warning, TextAnchor.MiddleCenter);
            UiKit.Place(missing.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -232f), new Vector2(620f, 28f));

            upgrade = UiKit.Button(card, "UpgradeNow", "Upgrade Now", "ui_green", null, OnUpgrade, 26);
            UiKit.Place((RectTransform)upgrade.transform, new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(-10f, 20f), new Vector2(260f, 66f));
            later = UiKit.Button(card, "Later", "Later", "ui_grey", null, OnLater, 24);
            UiKit.Place((RectTransform)later.transform, new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(10f, 20f), new Vector2(200f, 66f));

            Game.UpgradeRequested += OpenFor;
        }

        protected override void OnDestroy()
        {
            if (Game != null) Game.UpgradeRequested -= OpenFor;
            base.OnDestroy();
        }

        // Z kontextoveho okna budovy (S1, AWAITING_WAVE).
        public void OpenFor(BuildingId id)
        {
            target = id;
            fromFlow = false;
            SetOpen(true);
            Refresh();
        }

        protected override void OnSessionChanged()
        {
            if (Card == null) return;
            if (Session.Phase == WavePhase.Upgrade && !IsOpen)
            {
                fromFlow = true;
                target = Session.LastResult != null ? Session.LastResult.RecommendedBuilding : null;
                SetOpen(true);
            }
            else if (IsOpen && !WavePhaseRules.CanUpgrade(Session.Phase))
            {
                SetOpen(false);
            }

            if (IsOpen) Refresh();
        }

        private void Refresh()
        {
            heading.text = fromFlow ? "Recommended upgrade" : "Upgrade";
            WaveResult result = Session.LastResult;
            if (!target.HasValue)
            {
                building.text = "Your resort is in good shape";
                reason.text = "No upgrade needed now. Place Workers by the forecast.";
                upgrade.gameObject.SetActive(false);
                effect.text = "";
                price.text = "";
                missing.text = "";
                return;
            }

            BuildingId id = target.Value;
            building.text = ResortCatalog.BuildingName(id);
            reason.text = fromFlow && result != null && result.needsImprovement.Count > 0 ? result.needsImprovement[0] : "";
            upgrade.gameObject.SetActive(true);

            // Before/after ukazuje len jednu vec, pocet miest, ten isty, ktory hrac vidi na budove.
            BuildingState state = Session.State.Building(id);
            int now = Session.CapacityOf(id);
            if (Session.IsMaxLevel(id))
            {
                effect.text = "Seats " + now + "  <size=20>(maximum level)</size>";
                price.text = "";
                missing.text = "";
                upgrade.gameObject.SetActive(false);
                return;
            }

            int next = Session.Config.CapacityForLevel(state.level + 1);
            effect.text = "Seats: " + now + " now, <color=#348C50>" + next + " after upgrade</color>";
            price.text = "Cost: <b>" + UiKit.Number(Session.UpgradeCost(id)) + " Coins</b>    Resort Value <b>+" +
                         UiKit.Number(Session.UpgradeResortValue(id)) + "</b>";
            // Nedostatok sa hovori textom, nie len farbou.
            int shortBy = Session.UpgradeCost(id) - Session.State.coins;
            missing.text = shortBy > 0 ? "Missing: " + UiKit.Number(shortBy) + " Coins" : "";
            upgrade.interactable = Session.CanUpgrade(id);
            later.GetComponentInChildren<Text>().text = shortBy > 0 ? "Later" : "Not now";
        }

        private void OnUpgrade()
        {
            if (!target.HasValue || !Session.Upgrade(target.Value)) return;
            if (Game.Toasts != null) Game.Toasts.Show(ResortCatalog.BuildingName(target.Value) + " upgraded!", ToastKind.Positive);
            Continue();
        }

        // Later nemeni progres, len zavrie okno a pokracuje na S1 (resp. S9 ak je unlock).
        private void OnLater()
        {
            Continue();
        }

        private void Continue()
        {
            SetOpen(false);
            if (fromFlow) Game.ContinueAfterUpgradeStep();
        }
    }
}
