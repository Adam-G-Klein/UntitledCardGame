using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Playables;
using Unity.VisualScripting;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(UIDocument))]
[RequireComponent(typeof(PlayableCard))]
public class UIDocumentCard : MonoBehaviour {

    private Card card;
    private UIDocument doc;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    [SerializeField]
    private Texture2D texture {get;set;}
    public static Vector2Int CARD_REFERENCE_RESOLUTION = new (400, 700);
    public static float CARD_REFERENCE_SCALE = 0.5f;
    public static Vector2 CARD_SIZE = new Vector2(1f, 1.5f);

    [Header("Only for dev, one of the worst possible things you could do for GPU performance")]
    public bool renderTextureConstantly = false;
    private bool renderTextureCoroutineIsRunning = false;
    void Start() {
        card = GetComponent<PlayableCard>().card;
        Invoke("LateStart", 0.1f);
    }
    void LateStart() {
        
        boxCollider = GetComponent<BoxCollider2D>();
        doc = GetComponent<UIDocument>();
        doc.panelSettings = CardPanelSettingsPooler.Instance.GetPanelSettings();
        // TODO: take in card rather than cardtype
        doc.rootVisualElement.Add(UIDocumentUtils.makeWorldspaceCardView(card.cardType));
        UIDocumentUtils.SetAllPickingModeIgnore(doc.rootVisualElement);
        spriteRenderer = GetComponent<SpriteRenderer>();
        runCoroutine();
    }
    
    void Update(){
        if(renderTextureConstantly && !renderTextureCoroutineIsRunning){
            runCoroutine();
        }
    }

    private void runCoroutine(){
        Action<Texture2D> completionAction = (tex) => {
            print("UIDocumentCard: completion action invoked!");
            // Do something with the texture here.
            spriteRenderer.material.SetTexture("_SecondTex", tex);
            spriteRenderer.size = CARD_SIZE;
            boxCollider.size = CARD_SIZE;
        };

        IEnumerable coroutine = GetVETextureCoroutine(
            doc.panelSettings, CARD_REFERENCE_RESOLUTION.x, CARD_REFERENCE_RESOLUTION.y, completionAction);

        StartCoroutine(coroutine.GetEnumerator());
    }

    private void OnDestroy() {
        Debug.Log("UIDocumentCard: OnDestroy");
        CardPanelSettingsPooler.Instance.ReturnPanelSettings(doc.panelSettings);
        UIDocumentGameObjectPlacer.Instance.removeMapping(gameObject);
    }
    /*
    stolen from: https://forum.unity.com/threads/render-visualelement-to-texture.1169015/
    */
    public IEnumerable GetVETextureCoroutine(
            PanelSettings panelSettings,
            int width, int height,
            Action<Texture2D> completionAction)
    {
        renderTextureCoroutineIsRunning = true;
        
        // Store the existing parent (if any).
        
        // Create a new UIDocumment, RenderTexture, and Panel to draw to.
        doc.panelSettings = panelSettings;
        RenderTexture rt = new RenderTexture(width, height, 32);
        doc.panelSettings.targetTexture = rt;
        doc.panelSettings.referenceResolution = new Vector2Int(width, height);
        rt.Create();
        yield return null;
        
        // A frame later, we should have the RenderTexture fully rendered.
        // Create a texture and fill it in from the RenderTexture now that it's drawn.
        RenderTexture.active = rt;
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        yield return null;
        
        // Clean up the object we created and release the RenderTexture
        // (RenderTextures are not garbage collected objects).
        rt.Release();
        completionAction?.Invoke(texture);
        renderTextureCoroutineIsRunning = false;
    }

    void OnExitPlaymode(){
        //doc.panelSettings.targetTexture = defaultTexture;
    }
}