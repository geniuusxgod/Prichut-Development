#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Zaloha: ak by sa scena Main.unity nenacitala spravne,
// v menu Resort Empire > Rebuild Main Scene sa vytvori znova z obrazkov.
public static class MainSceneBuilder
{
    private const string SpriteFolder = "Assets/Art/Sprites/";
    private const string ScenePath = "Assets/Scenes/Main.unity";

    private struct Item
    {
        public string name;
        public string sprite;
        public float x;
        public float y;
        public int order;
        public float scale;

        public Item(string name, string sprite, float x, float y, int order, float scale)
        {
            this.name = name;
            this.sprite = sprite;
            this.x = x;
            this.y = y;
            this.order = order;
            this.scale = scale;
        }
    }

    private static readonly Item[] Items = new Item[]
    {
        new Item("Ocean", "ocean_background", 0.0f, 0.0f, 0, 1.6f),
        new Item("Island", "island", -0.08f, -0.795f, 1, 1.0f),
        new Item("Reception", "building_hotel", 0.23f, 2.435f, 2, 1.0f),
        new Item("Restaurant", "building_restaurant", -2.735f, 0.96f, 3, 1.0f),
        new Item("BeachBar", "building_beach_bar", 3.555f, -0.02f, 3, 1.0f),
        new Item("Clouds", "clouds", 0.18f, -0.245f, 10, 1.0f)
    };

    [MenuItem("Resort Empire/Rebuild Main Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camObject = new GameObject("Main Camera");
        camObject.tag = "MainCamera";
        camObject.transform.position = new Vector3(0f, 0f, -10f);
        var cam = camObject.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.184f, 0.576f, 0.706f, 1f);
        camObject.AddComponent<AudioListener>();
        camObject.AddComponent<CameraFitter>();

        var buildings = new GameObject("Buildings");
        foreach (var item in Items)
        {
            var go = new GameObject(item.name);
            go.transform.position = new Vector3(item.x, item.y, 0f);
            go.transform.localScale = new Vector3(item.scale, item.scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + item.sprite + ".png");
            sr.sortingOrder = item.order;
            if (item.name == "Reception" || item.name == "Restaurant" || item.name == "BeachBar")
            {
                go.transform.SetParent(buildings.transform, true);
            }
        }

        var slots = new GameObject("BuildingSlots");
        CreateSlot(slots.transform, "Slot_Pool", new Vector3(-1.6f, -1.2f, 0f));
        CreateSlot(slots.transform, "Slot_Bungalows", new Vector3(2.3f, -1.2f, 0f));

        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    private static void CreateSlot(Transform parent, string name, Vector3 position)
    {
        var slot = new GameObject(name);
        slot.transform.SetParent(parent, false);
        slot.transform.position = position;
    }
}
#endif
