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
