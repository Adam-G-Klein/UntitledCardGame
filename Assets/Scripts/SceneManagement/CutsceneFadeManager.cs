using UnityEngine;
using System.Collections;

public class CutsceneFadeManager : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    private static CutsceneFadeManager instance;
    public static CutsceneFadeManager Instance => instance;

    private ScreenFader screenFader;

    public float FadeTime => screenFader.fadeTime;

    private void Awake()
    {
        instance = this;
        SetupScreenFader();
    }

    private void SetupScreenFader()
    {
        GameObject faderObj = new GameObject("ScreenFader");
        faderObj.transform.SetParent(transform);
        screenFader = faderObj.AddComponent<ScreenFader>();
        screenFader.fadeTime = fadeTime;
        screenFader.fadeColor = fadeColor;
        screenFader.Initialize();
    }

    public void SetSortingOrder(int sortingOrder) => screenFader.SetSortingOrder(sortingOrder);

    public void ResetSortingOrder() => screenFader.ResetSortingOrder();

    public void SetAlpha(float alpha) => screenFader.SetAlpha(alpha);

    public IEnumerator Fade(float targetAlpha) => screenFader.Fade(targetAlpha);
}
