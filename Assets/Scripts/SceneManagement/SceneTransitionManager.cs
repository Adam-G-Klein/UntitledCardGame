using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] public float fadeTime = .25f;
    [SerializeField] private Color fadeColor = Color.black;

    [Header("Reveal Transition (in-run location changes)")]
    [Tooltip("Ink wipe material (InkWipeTransition shader). Leave empty to use the plain fade everywhere.")]
    [SerializeField] private Material revealMaterial;
    [Tooltip("Seconds for the ink to cover the screen, and again to pull away.")]
    [SerializeField] private float revealTime = 0.45f;
    [Tooltip("Direction the ink pulls in to cover the screen.")]
    [SerializeField] private bool coverLeftToRight = true;
    [Tooltip("When on, the ink keeps travelling the same way to reveal (a pass-through wipe). When off, it pulls back the way it came.")]
    [SerializeField] private bool revealContinuesAcross = true;
    [Tooltip("Cutoff at which the screen is fully clear. Depends on the dissolve texture; re-measure if you swap it.")]
    [SerializeField] private float revealClearCutoff = 0.52f;

    [Header("Hold On Black")]
    [Tooltip("A short beat on black between covering and revealing, so the swap reads as one motion.")]
    [SerializeField] private float holdOnBlack = 0.1f;
    [Tooltip("Longest the screen waits on black for the new scene's first frames (which hitch while it starts up) before revealing.")]
    [SerializeField] private float maxSettleTime = 1.2f;

    private static SceneTransitionManager instance;
    public static SceneTransitionManager Instance => instance;
    private ScreenFader screenFader;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SetupScreenFader();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupScreenFader()
    {
        GameObject faderObj = new GameObject("ScreenFader");
        faderObj.transform.SetParent(transform);
        screenFader = faderObj.AddComponent<ScreenFader>();
        screenFader.fadeTime = fadeTime;
        screenFader.fadeColor = fadeColor;
        screenFader.Initialize();
        screenFader.InitializeWipe(revealMaterial);
    }

    // Used for moving between locations within a run (map, combat, shop, events). Menus and
    // exits keep the quick fade from LoadScene.
    public static void LoadSceneWithReveal(string sceneName, float delay = 0f)
    {
        if (instance == null) return;
        if (!instance.screenFader.HasWipe)
        {
            LoadScene(sceneName, delay);
            return;
        }
        Debug.Log($"SceneTransitionManager: Loading scene {sceneName} with reveal");
        if (instance.fadeCoroutine != null) instance.StopCoroutine(instance.fadeCoroutine);
        // Matches LoadScene, which starts its transition straight away (it doesn't forward its delay).
        instance.fadeCoroutine = instance.StartCoroutine(instance.WipeAndLoadScene(sceneName));
    }

    private IEnumerator WipeAndLoadScene(string sceneName)
    {
        // the old scene keeps moving under the ink; its tweens are only cut once it is hidden
        yield return StartCoroutine(screenFader.Wipe(true, coverLeftToRight, revealTime, revealClearCutoff));
        yield return StartCoroutine(PresentCovered());
        LeanTween.cancelAll();
        SceneManager.sceneLoaded += OnSceneLoadedReveal;
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoadedReveal(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedReveal;
        fadeCoroutine = StartCoroutine(RevealWhenSettled());
    }

    private IEnumerator RevealWhenSettled()
    {
        yield return StartCoroutine(WaitForSceneToSettle());
        bool revealLeftToRight = revealContinuesAcross ? !coverLeftToRight : coverLeftToRight;
        yield return StartCoroutine(screenFader.Wipe(false, revealLeftToRight, revealTime, revealClearCutoff));
    }

    // Makes sure a fully covered frame has been drawn before the load blocks the main thread, so the
    // screen holds on black rather than on a part-covered frame.
    private IEnumerator PresentCovered()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
    }

    // A new scene's first frames run its Awake/Start work and can take several hundred milliseconds each.
    // Revealing over them makes the transition stutter, so stay on black until two frames in a row run at a
    // normal rate (capped at maxSettleTime), plus the short holdOnBlack beat.
    private IEnumerator WaitForSceneToSettle()
    {
        const float smoothFrame = 1f / 20f;
        float start = Time.realtimeSinceStartup;
        int smoothFrames = 0, frames = 0;
        while (Time.realtimeSinceStartup - start < maxSettleTime)
        {
            yield return null;
            frames++;
            smoothFrames = Time.unscaledDeltaTime < smoothFrame ? smoothFrames + 1 : 0;
            if (frames >= 3 && smoothFrames >= 2 && Time.realtimeSinceStartup - start >= holdOnBlack) break;
        }
    }

    public static void LoadScene(string sceneName, float delay = 0f)
    {
        Debug.Log($"SceneTransitionManager: Loading scene {sceneName}");
        if (instance != null)
        {
            if (instance.fadeCoroutine != null) instance.StopCoroutine(instance.fadeCoroutine);
            instance.fadeCoroutine = instance.StartCoroutine(instance.WaitAndLoadScene(sceneName));
        }
    }

    private IEnumerator WaitAndLoadScene(string sceneName, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        LeanTween.cancelAll();
        yield return StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        yield return StartCoroutine(screenFader.Fade(1f));
        SceneManager.sceneLoaded += OnSceneLoaded;
        screenFader.SetAlpha(1f);
        yield return StartCoroutine(PresentCovered());
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        screenFader.SetAlpha(1f);
        fadeCoroutine = StartCoroutine(FadeInWhenSettled());
    }

    private IEnumerator FadeInWhenSettled()
    {
        yield return StartCoroutine(WaitForSceneToSettle());
        yield return StartCoroutine(screenFader.Fade(0f));
    }

    public void SetSortingOrder(int sortingOrder) => screenFader.SetSortingOrder(sortingOrder);

    public void ResetSortingOrder() => screenFader.ResetSortingOrder();

    public IEnumerator Fade(float targetAlpha) => screenFader.Fade(targetAlpha);
}
