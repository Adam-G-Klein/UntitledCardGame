using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] public float fadeTime = .25f;
    [SerializeField] private Color fadeColor = Color.black;

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
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        screenFader.SetAlpha(1f);
        StartCoroutine(screenFader.Fade(0f));
    }

    public void SetSortingOrder(int sortingOrder) => screenFader.SetSortingOrder(sortingOrder);

    public void ResetSortingOrder() => screenFader.ResetSortingOrder();

    public IEnumerator Fade(float targetAlpha) => screenFader.Fade(targetAlpha);
}
