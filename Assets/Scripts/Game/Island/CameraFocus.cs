using UnityEngine;

namespace Prichut.Resort.Game
{
    // Kamera plynulo prileti k vybranej budove a ukaze ju zblizka aj s Workerom.
    // Po zruseni vyberu sa vrati na cely ostrov. Velkost celeho zaberu pocita rovnako ako CameraFitter.
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFocus : MonoBehaviour
    {
        private const float WorldWidth = 20f;
        private const float WorldHeight = 10f;
        private const float Zoom = 0.6f;
        private const float SmoothTime = 0.35f;

        private Camera cam;
        private ResortGameController game;
        private Vector3 velocity;
        private float sizeVelocity;

        public void Init(ResortGameController controller)
        {
            cam = GetComponent<Camera>();
            game = controller;
        }

        public static float FullSize(float aspect)
        {
            return Mathf.Max(WorldHeight / 2f, WorldWidth / (2f * aspect));
        }

        // Ciel je posunuty nahor, aby budovu nezakrylo kontextove okno v dolnej casti obrazovky.
        public static Vector3 Clamp(Vector3 target, float size, float aspect)
        {
            float halfW = Mathf.Max(0f, WorldWidth / 2f - size * aspect);
            float halfH = Mathf.Max(0f, WorldHeight / 2f - size);
            return new Vector3(Mathf.Clamp(target.x, -halfW, halfW), Mathf.Clamp(target.y, -halfH, halfH), -10f);
        }

        private void LateUpdate()
        {
            if (game == null) return;
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            float full = FullSize(aspect);
            float size = full;
            Vector3 target = new Vector3(0f, 0f, -10f);

            BuildingId? selected = game.SelectedBuilding;
            if (selected.HasValue && game.Island != null &&
                game.Island.Buildings.TryGetValue(selected.Value, out BuildingView view))
            {
                size = full * Zoom;
                Vector3 focus = view.Renderer.bounds.center;
                target = new Vector3(focus.x + 0.4f, focus.y - size * 0.25f, -10f);
            }

            target = Clamp(target, size, aspect);
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, size, ref sizeVelocity, SmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, SmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        }
    }
}
