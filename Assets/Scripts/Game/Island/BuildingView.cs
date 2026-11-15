using UnityEngine;
using UnityEngine.EventSystems;

namespace Prichut.Resort.Game
{
    // Budova na ostrove: tap ju vyberie. Hracove akcie idu vzdy cez ResortGameController,
    // budova nema vlastnu logiku ani kopiu stavu.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class BuildingView : MonoBehaviour, IPointerClickHandler
    {
        public BuildingId Id { get; private set; }
        public SpriteRenderer Renderer { get; private set; }
        public ResortGameController Game { get; private set; }

        public void Init(ResortGameController game, BuildingId id)
        {
            Game = game;
            Id = id;
            Renderer = GetComponent<SpriteRenderer>();
            var box = GetComponent<BoxCollider2D>();
            if (box == null) box = gameObject.AddComponent<BoxCollider2D>();
            Vector3 size = Renderer.sprite != null ? Renderer.sprite.bounds.size : Vector3.one;
            box.size = new Vector2(size.x * 0.8f, size.y * 0.75f);
            box.offset = Renderer.sprite != null ? (Vector2)Renderer.sprite.bounds.center : Vector2.zero;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Game == null) return;
            Game.Select(Id);
        }

        // Svetovy bod pod budovou, sem sa ukotvuje informacny stitok a rad hosti.
        public Vector3 BasePoint => new Vector3(Renderer.bounds.center.x, Renderer.bounds.min.y + 0.35f, 0f);
    }
}
