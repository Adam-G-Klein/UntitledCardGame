using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private Color fadeColor = Color.black;
    
    private static SceneTransitionManager instance;
    private Image fadeImage;
    private bool isFading = false;

    private void Awake()
    {
        // Singleton pattern to persist across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SetupFadeCanvas();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupFadeCanvas()
    {
        // Create canvas for the fade effect
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Ensure it renders on top

        // Add a Canvas Scaler for proper UI scaling
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create the fade image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(transform, false);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        // Set the image to cover the entire screen
        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }

    public static void LoadScene(string sceneName)
    {
        if (instance != null && !instance.isFading)
        {
            instance.StartCoroutine(instance.FadeAndLoadScene(sceneName));
        }
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        isFading = true;

        // Fade to black
        yield return StartCoroutine(Fade(1f));

        // Load the new scene
        SceneManager.LoadScene(sceneName);

        yield return new WaitForSeconds(0.25f); // waits for 0.1 seconds

        // Fade back in
        yield return StartCoroutine(Fade(0f));

        isFading = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float alpha = fadeImage.color.a;
        
        while (!Mathf.Approximately(alpha, targetAlpha))
        {
            alpha = Mathf.MoveTowards(alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
    }
}