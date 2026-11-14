using System.Collections.Generic;
using UnityEngine;

/*
    VFX: for an effect that plays every hit of a multi-hit card itself (Double Strike's two crossing cuts). The card
    spawns its effect once per hit, so a spawn of the same effect within `window` seconds of the last one removes
    itself, and the first spawn's strikes are timed to land with each hit's feedback (HitTiming). Purely visual.
*/
public class OnePerCast : MonoBehaviour
{
    [Tooltip("Spawns of this effect closer together than this (seconds) count as the same card.")]
    [SerializeField] private float window = 0.9f;

    private static readonly Dictionary<string, float> lastSpawn = new Dictionary<string, float>();

    private void Awake()
    {
        string key = gameObject.name.Replace("(Clone)", "").Trim();
        if (lastSpawn.TryGetValue(key, out float at) && Time.time - at < window)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        lastSpawn[key] = Time.time;
    }
}
