using UnityEngine;
using UnityEngine.UIElements;

/*
    Keeps a world-space effect laid over a UI Toolkit element: centred on it and scaled to its size on
    screen, every frame. worldBound includes USS transforms, so the effect grows and shrinks with the
    element's hover/focus scale instead of staying at the size it was spawned at.
*/
public class FollowVisualElementBounds : MonoBehaviour
{
    private VisualElement target;

    public void Follow(VisualElement element)
    {
        target = element;
        LateUpdate();
    }

    private void LateUpdate()
    {
        if (target == null || target.panel == null) return;
        Rect bounds = target.worldBound;
        if (float.IsNaN(bounds.width) || bounds.width <= 0f || bounds.height <= 0f) return;
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 bottomLeft = UIToWorld(cam, bounds.xMin, bounds.yMax);
        Vector3 topRight = UIToWorld(cam, bounds.xMax, bounds.yMin);
        transform.position = (bottomLeft + topRight) / 2f;
        transform.localScale = new Vector3(Mathf.Abs(topRight.x - bottomLeft.x), Mathf.Abs(topRight.y - bottomLeft.y), 1f);
    }

    // Same mapping as UIDocumentGameObjectPlacer.GetWorldPositionFromUIDocumentPosition, without its
    // per-call Debug.Logs (this runs every frame, which flooded the console and Editor.log).
    private static Vector3 UIToWorld(Camera cam, float x, float y)
    {
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(x, Screen.height - y, 0f));
        return new Vector3(world.x, world.y, UIDocumentGameObjectPlacer.zPlane);
    }
}
