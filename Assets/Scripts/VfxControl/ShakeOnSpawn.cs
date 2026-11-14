using System.Collections;
using UnityEngine;

// Shakes the camera once, `delay` seconds after the effect spawns (its impact frame).
// Goes through ScreenShakeManager, which caps and damps stacked shakes.
public class ShakeOnSpawn : MonoBehaviour
{
    public float delay = 0.3f;
    public float force = 0.4f;

    IEnumerator Start()
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        // don't create the singleton from an effect (previews, menus)
        ScreenShakeManager shaker = FindObjectOfType<ScreenShakeManager>();
        if (shaker != null) shaker.ShakeWithForce(force);
    }
}
