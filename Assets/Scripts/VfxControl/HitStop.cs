using System.Collections;
using UnityEngine;

/*
    Brief time freezes and slow-mo for impacts. Works relative to whatever Time.timeScale was when it
    started (the options menu sets a game-speed multiplier there) and puts that value back afterwards.
    Overlapping calls extend the current one rather than stacking, so the restored speed is always the
    original.
*/
public class HitStop : MonoBehaviour
{
    private static HitStop instance;
    private float baseScale = 1f;
    private float endTime;
    private Coroutine running;

    private static HitStop Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("HitStop");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<HitStop>();
            }
            return instance;
        }
    }

    // Near-freeze: the game runs at `scale` of its normal speed for `seconds` of real time.
    public static void Freeze(float seconds, float scale = 0.05f)
    {
        if (seconds <= 0f) return;
        Instance.Play(seconds, scale, 0f);
    }

    // Slow-mo that eases back to full speed over the last `easeOutPortion` of its time.
    public static void SlowMo(float seconds, float scale, float easeOutPortion = 0.35f)
    {
        if (seconds <= 0f) return;
        Instance.Play(seconds, scale, easeOutPortion);
    }

    private void Play(float seconds, float scale, float easeOutPortion)
    {
        if (running == null) baseScale = Time.timeScale;
        endTime = Mathf.Max(endTime, Time.realtimeSinceStartup + seconds);
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Run(seconds, scale, easeOutPortion));
    }

    private IEnumerator Run(float seconds, float scale, float easeOutPortion)
    {
        float easeStart = endTime - seconds * easeOutPortion;
        while (Time.realtimeSinceStartup < endTime)
        {
            float now = Time.realtimeSinceStartup;
            float t = easeOutPortion > 0f && now > easeStart ? Mathf.InverseLerp(easeStart, endTime, now) : 0f;
            Time.timeScale = baseScale * Mathf.Lerp(scale, 1f, t * t * (3f - 2f * t));
            yield return null;
        }
        Time.timeScale = baseScale;
        running = null;
    }

    private void OnDestroy()
    {
        if (running != null) Time.timeScale = baseScale;
    }
}
