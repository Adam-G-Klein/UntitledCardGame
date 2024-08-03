using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

[InitializeOnLoad]
[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(UIDocument))]
public class UIDocumentScreenspace : MonoBehaviour {

    private UIDocument doc;
    private RawImage image;
    [SerializeField]
    private Texture2D texture {get;set;}
    void Start() {
        
        doc = GetComponent<UIDocument>();

        StartCoroutine(GetVETextureCoroutine(
            doc.panelSettings, Screen.width, Screen.height,
            (tex) => {
                print("completion action invoked!");
                // Do something with the texture here.
                image = gameObject.GetComponent<RawImage>();
                image.texture = tex;
                image.material.mainTexture = tex;
            }
        ));

        SetAllPickingModeIgnore(doc.rootVisualElement);
    }

    private void SetAllPickingModeIgnore(VisualElement ve){
        ve.pickingMode = PickingMode.Ignore;
        foreach (VisualElement child in ve.Children()){
            SetAllPickingModeIgnore(child);
        }
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
        //doc.panelSettings.targetTexture = defaultTexture;
    }
}