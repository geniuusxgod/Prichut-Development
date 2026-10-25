using UnityEngine;

// Prisposobi kameru tak, aby bol ostrov na sirku vzdy cely vidno.
// Scena ma 20 x 10 jednotiek (obrazok 2000 x 1000 px pri 100 px na jednotku).
[RequireComponent(typeof(Camera))]
public class CameraFitter : MonoBehaviour
{
    [SerializeField] private float worldWidth = 20f;
    [SerializeField] private float worldHeight = 10f;

    private Camera cam;
    private int lastWidth;
    private int lastHeight;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        Fit();
    }

    private void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            Fit();
        }
    }

    private void Fit()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        float aspect = (float)Screen.width / Screen.height;
        float sizeByHeight = worldHeight / 2f;
        float sizeByWidth = worldWidth / (2f * aspect);
        cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
    }
}
