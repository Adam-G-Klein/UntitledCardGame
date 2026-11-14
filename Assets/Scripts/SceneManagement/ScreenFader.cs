using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public float fadeTime = 0.25f;
    public Color fadeColor = Color.black;

    private Image fadeImage;
    private Canvas canvas;
    private int defaultSortingOrder = 999;
    private RenderMode defaultRenderMode = RenderMode.ScreenSpaceOverlay;
    private string defaultSortingLayer;
    private IEnumerator fadeCoroutine;

    public void Initialize()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = defaultRenderMode;
        canvas.sortingOrder = defaultSortingOrder;
        defaultSortingLayer = canvas.sortingLayerName;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(transform, false);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }

    public void SetSortingOrder(int sortingOrder)
    {
        if (canvas == null) return;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.sortingLayerName = "Overlay UI";
        canvas.sortingOrder = sortingOrder;
    }

    public void ResetSortingOrder()
    {
        if (canvas == null) return;
        canvas.renderMode = defaultRenderMode;
        canvas.sortingLayerName = defaultSortingLayer;
        canvas.sortingOrder = defaultSortingOrder;
    }

    public void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;
        LeanTween.cancel(fadeImage.rectTransform);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
    }

    // ---- Ink wipe (optional; uses the InkWipeTransition shader) ----
    // Alpha is smoothstep(_Cutoff, _Cutoff + _Softness, ink), and colour runs from the ink texture at the
    // leading edge to _Dissolve_Colour (black) over _Fade_Width behind it. clearCutoff is where the first
    // ink appears (it depends on the dissolve texture); covering runs past full coverage by the fade width
    // so the screen ends solid black before the scene swaps. _Left_Right picks the side.
    private RawImage wipeImage;
    private Material wipeMaterial;
    private static readonly int CutoffId = Shader.PropertyToID("_Cutoff");
    private static readonly int SoftnessId = Shader.PropertyToID("_Softness");
    private static readonly int LeftRightId = Shader.PropertyToID("_Left_Right");
    private static readonly int FadeWidthId = Shader.PropertyToID("_Fade_Width");
    // VFX: each wipe erodes a little differently (its erosion texture's vertical tiling varies), and gold and pink flakes
    // fly across the screen to the right as it plays
    private static readonly int DissolveTilingId = Shader.PropertyToID("_Dissolve_Tiling");
    private Vector4 baseDissolveTiling;
    private GameObject wipeFlakes;
    private const float MaxWipeStep = 1f / 30f;

    public bool HasWipe => wipeMaterial != null;

    public void InitializeWipe(Material material, GameObject flakes = null)
    {
        if (material == null) return;
        wipeFlakes = flakes;
        GameObject wipeObj = new GameObject("WipeImage");
        wipeObj.transform.SetParent(transform, false);
        wipeImage = wipeObj.AddComponent<RawImage>();
        // run on a copy so animating the cutoff never dirties the shared material asset
        wipeMaterial = new Material(material);
        wipeImage.material = wipeMaterial;
        if (wipeMaterial.HasProperty(DissolveTilingId)) baseDissolveTiling = wipeMaterial.GetVector(DissolveTilingId);
        wipeImage.texture = material.mainTexture;
        wipeImage.raycastTarget = true;

        RectTransform rect = wipeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        wipeImage.enabled = false;
    }

    public IEnumerator Wipe(bool cover, bool leftToRight, float duration, float clearCutoff)
    {
        if (wipeMaterial == null) yield break;
        float fadeWidth = wipeMaterial.HasProperty(FadeWidthId) ? wipeMaterial.GetFloat(FadeWidthId) : 0f;
        float hiddenCutoff = -(wipeMaterial.GetFloat(SoftnessId) + fadeWidth);
        float shownCutoff = clearCutoff;
        float from = cover ? shownCutoff : hiddenCutoff;
        float to = cover ? hiddenCutoff : shownCutoff;

        wipeMaterial.SetFloat(LeftRightId, leftToRight ? 1f : 0f);
        wipeMaterial.SetFloat(CutoffId, from);
        if (baseDissolveTiling != Vector4.zero)
            wipeMaterial.SetVector(DissolveTilingId, new Vector4(baseDissolveTiling.x, baseDissolveTiling.y * UnityEngine.Random.Range(0.8f, 1.25f), baseDissolveTiling.z, baseDissolveTiling.w));
        UseCameraSpaceForWipe();
        if (cover) SpawnWipeFlakes();   // once per transition: a second burst over the new scene looked like a repeat
        wipeImage.enabled = true;
        if (!cover) SetAlpha(0f);   // the plain black that held the screen if the new scene had no camera yet

        float t = 0f;
        while (t < duration)
        {
            // Clamp the step: the first frame after a scene load can take 300-500ms, which would
            // otherwise finish the whole reveal in a single frame.
            t += Mathf.Min(Time.unscaledDeltaTime, MaxWipeStep);
            float p = Mathf.Clamp01(t / duration);
            // covering eases in and out; revealing eases out, so the ink pulls away briskly and settles
            float eased = cover ? p * p * (3f - 2f * p) : 1f - (1f - p) * (1f - p) * (1f - p);
            wipeMaterial.SetFloat(CutoffId, Mathf.Lerp(from, to, eased));
            yield return null;
        }
        wipeMaterial.SetFloat(CutoffId, to);
        if (!cover)
        {
            wipeImage.enabled = false;
            ResetSortingOrder();
        }
    }

    // Called as soon as a new scene has loaded, before it draws: the camera the covered wipe was drawn through
    // went with the old scene, and a Screen Space Camera canvas with no camera falls back to overlay, where the
    // wipe's Shader Graph draws its screen position as colour (a red and green gradient) in a build. So switch
    // it to the new camera now, or hold plain black until the reveal finds one.
    public void HoldWipeCovered()
    {
        if (wipeImage == null || !wipeImage.enabled) return;
        if (Camera.main != null) { UseCameraSpaceForWipe(); return; }
        wipeImage.enabled = false;
        SetAlpha(1f);
    }

    // VFX: the flakes ride the camera (so they're in view and go with its scene), just past the wipe's canvas and drawn
    // in front of it
    private void SpawnWipeFlakes()
    {
        Camera cam = Camera.main;
        if (wipeFlakes == null || cam == null) return;
        GameObject flakes = Instantiate(wipeFlakes, cam.transform);
        flakes.transform.localPosition = new Vector3(0f, 0f, cam.nearClipPlane + 1f);
        flakes.transform.localRotation = Quaternion.identity;
        if (cam.orthographic) flakes.transform.localScale = Vector3.one * (cam.orthographicSize / 5f);   // built for the combat camera
        foreach (ParticleSystemRenderer r in flakes.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            r.sortingLayerName = "Tooltips";
            r.sortingOrder = defaultSortingOrder + 1;
        }
    }

    // The wipe material is a Shader Graph, which a Screen Space Overlay canvas draws as solid black.
    // While wiping, render the canvas through the current main camera instead, just in front of it and
    // on the top sorting layer. (After a scene load the old camera is gone, so this is called again with
    // the new one before the reveal.)
    private void UseCameraSpaceForWipe()
    {
        Camera cam = Camera.main;
        if (canvas == null || cam == null) return;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = cam.nearClipPlane + 0.1f;
        canvas.sortingLayerName = "Tooltips";
        canvas.sortingOrder = defaultSortingOrder;
    }

    public IEnumerator Fade(float targetAlpha)
    {
        if (fadeImage == null) yield break;
        LeanTween.cancel(fadeImage.rectTransform);
        float startAlpha = fadeImage.color.a;
        Color targetColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, targetAlpha);
        LeanTween.value(gameObject, startAlpha, targetAlpha, fadeTime)
            .setOnUpdate((float a) =>
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, a);
            })
            .setOnComplete(() =>
            {
                fadeImage.color = targetColor;
            });
        yield return new WaitForSeconds(fadeTime);
    }
}
