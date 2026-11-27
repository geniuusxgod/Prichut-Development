using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // Stavenisko pre Pool / Bungalows (PRIH-51): locked -> available -> unlocked.
    // Prazdna plocha je asset, nie diera, od prvej sekundy ukazuje, co sa da ziskat.
    public sealed class AreaSiteView : MonoBehaviour, IPointerClickHandler
    {
        private ResortGameController game;
        private BuildingId id;
        private SpriteRenderer site;
        private SpriteRenderer building;
        private RectTransform marker;
        private RectTransform overlay;
        private Text markerLabel;
        private Image markerIcon;
        private bool registered;

        public void Init(ResortGameController controller, BuildingId area, Vector3 position, RectTransform overlayRoot)
        {
            game = controller;
            id = area;
            overlay = overlayRoot;
            transform.position = position;

            site = gameObject.AddComponent<SpriteRenderer>();
            site.sprite = UiKit.Sprite("site");
            site.sortingOrder = 2;
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = site.sprite != null ? (Vector2)site.sprite.bounds.size * 0.8f : Vector2.one * 2f;

            var buildingGo = new GameObject(ResortCatalog.BuildingName(area));
            buildingGo.transform.SetParent(transform, false);
            building = buildingGo.AddComponent<SpriteRenderer>();
            building.sortingOrder = 3;

            Image pill = UiKit.Panel(overlay, "Site_" + area, "ui_dark", 3f);
            pill.raycastTarget = false;
            marker = pill.rectTransform;
            marker.anchorMin = marker.anchorMax = Vector2.zero;
            marker.pivot = new Vector2(0.5f, 0.5f);
            marker.sizeDelta = new Vector2(190f, 40f);
            markerIcon = UiKit.Icon(marker, "icon_lock", 26f);
            UiKit.Place(markerIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(26f, 26f));
            markerLabel = UiKit.Label(marker, "", 16, UiKit.White, TextAnchor.MiddleLeft);
            UiKit.Place(markerLabel.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(42f, 0f), new Vector2(144f, 36f));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (game == null || game.Session.State.Owns(id)) return;
            game.RequestAreaUnlock(id);
        }

        private void LateUpdate()
        {
            if (game == null) return;
            ResortSession session = game.Session;
            AreaState state = session.AreaStateOf(id);
            bool owned = state == AreaState.Unlocked;
            building.enabled = owned;
            site.enabled = !owned;
            site.color = state == AreaState.Locked ? new Color(0.85f, 0.85f, 0.85f, 0.9f) : Color.white;
            marker.gameObject.SetActive(!owned);

            if (owned && !registered)
            {
                registered = true;
                building.sprite = UiKit.Sprite(id == BuildingId.Pool ? "pool" : "bungalows");
                GetComponent<BoxCollider2D>().enabled = false;
                game.Island.Register(id, building.gameObject);
            }

            if (owned) return;
            float pulse = state == AreaState.Available ? 1f + 0.05f * Mathf.Sin(Time.unscaledTime * 4f) : 1f;
            marker.localScale = Vector3.one * pulse;
            markerIcon.sprite = UiKit.Sprite(state == AreaState.Available ? "icon_play" : "icon_lock");
            markerLabel.text = state == AreaState.Available
                ? ResortCatalog.BuildingName(id) + ": tap to unlock"
                : ResortCatalog.BuildingName(id) + " after Wave " + session.GateWave(id);

            Camera cam = Camera.main;
            if (cam == null) return;
            Vector2 screen = cam.WorldToScreenPoint(transform.position + new Vector3(0f, 0.2f, 0f));
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, screen, null, out Vector2 local))
                marker.anchoredPosition = local + overlay.rect.size * 0.5f;
        }
    }
}
