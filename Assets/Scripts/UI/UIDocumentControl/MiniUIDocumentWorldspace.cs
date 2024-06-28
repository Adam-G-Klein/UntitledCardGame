using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;

[InitializeOnLoad]
[RequireComponent(typeof(UIDocument))]
public class MiniUIDocumentWorldspace : MonoBehaviour {

    public UIDocument doc;
    private MeshRenderer meshRenderer;
    [SerializeField]
    private Texture2D texture {get;set;}
    [SerializeField]
    [Header("Needed so that the panelSettings can be restored after we exit\nplaymode in editor. Otherwise the Game view will be useless.")]
    private RenderTexture defaultTexture;

    void Start() {
        
        doc = GetComponent<UIDocument>();
        doc.panelSettings = doc.panelSettings;
        defaultTexture = doc.panelSettings.targetTexture;
        StartCoroutine(GetVETextureCoroutine(
            doc.panelSettings, 1920, 1080,
            (tex) => {
                print("completion action invoked!");
                // Do something with the texture here.
                meshRenderer = gameObject.GetComponent<MeshRenderer>();
                meshRenderer.material.mainTexture = tex;
            }
        ));

        EditorApplication.playModeStateChanged += (PlayModeStateChange state) => {
            if (state == PlayModeStateChange.ExitingPlayMode) {
                OnExitPlaymode();
            }
        };
    }
    /*
    stolen from: https://forum.unity.com/threads/render-visualelement-to-texture.1169015/
    */
    public IEnumerator GetVETextureCoroutine(
            PanelSettings panelSettings,
            int width, int height,
            Action<Texture2D> completionAction)
    {
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
    }

    void OnExitPlaymode(){
        doc.panelSettings.targetTexture = defaultTexture;
    }
    
}