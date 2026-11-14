using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/*
    An impact frame (after the ink-wash reference video): the whole screen drops to stark grey and white, so everything
    reads as a silhouette, while an ink splash bursts where the blow landed. Played on the last death of a fight, ally
    or enemy (CombatEntityManager.PlayLastKill), alongside the last-kill slow-mo.

    It eases in and out over `seconds` of real time rather than cutting, and doesn't blow the lights out to white: a
    hard flash to a bright frame and back can trigger flash warnings and is hard on the eyes.

    A temporary global post-processing volume (colour adjustments) above the scene's own, removed when it's done, on
    real time so the slow-mo doesn't stretch it. Settings live on the encounter constants under "Last Kill".
*/
public class ImpactFrame : MonoBehaviour
{
    private static ImpactFrame playing;

    public static void Play(Vector3 at, GameObject splashPrefab, float seconds)
    {
        if (splashPrefab != null) Instantiate(splashPrefab, at, Quaternion.identity);
        if (seconds <= 0f || playing != null) return;
        playing = new GameObject("Impact Frame").AddComponent<ImpactFrame>();
        playing.StartCoroutine(playing.Run(seconds));
    }

    private VolumeProfile profile;

    private IEnumerator Run(float seconds)
    {
        Volume volume = gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 100f;
        profile = ScriptableObject.CreateInstance<VolumeProfile>();
        ColorAdjustments grade = profile.Add<ColorAdjustments>(true);
        grade.saturation.Override(-100f);   // grey
        grade.contrast.Override(65f);       // pushed toward black and white: silhouettes
        grade.postExposure.Override(0.25f); // a little lifted, not blown out
        volume.profile = profile;
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            float u = t / seconds;
            // in over the first fifth, held, then out over the last half
            volume.weight = u < 0.2f ? Mathf.SmoothStep(0f, 1f, u / 0.2f) : u < 0.5f ? 1f : 1f - Mathf.SmoothStep(0f, 1f, (u - 0.5f) / 0.5f);
            yield return null;
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (profile != null) Destroy(profile);
        if (playing == this) playing = null;
    }
}
