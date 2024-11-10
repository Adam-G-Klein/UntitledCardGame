using UnityEngine;
using System;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System.Collections;
using UnityEditor;


[RequireComponent(typeof(UIDocument))]
public class MiniUIDocumentWorldspace : MonoBehaviour
{

    public UIDocument doc;
    private MeshRenderer meshRenderer;
    private UnityEngine.UI.Image image;
    [SerializeField]
    private Texture2D texture { get; set; }

    [SerializeField]
    private int width = 1920;
    [SerializeField]
    private int height = 1080;

    void Start()
    {

        doc = GetComponent<UIDocument>();
        doc.panelSettings = doc.panelSettings;
        //defaultTexture = doc.panelSettings.targetTexture;
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        image = gameObject.GetComponent<UnityEngine.UI.Image>();
        if (meshRenderer)
        {
            StartCoroutine(GetVETextureCoroutine(
                        doc.panelSettings, this.width, this.height,
                        (tex) =>
                        {
                            print("completion action invoked!");
                            // Do something with the texture here.
                            meshRenderer = gameObject.GetComponent<MeshRenderer>();
                            meshRenderer.material.mainTexture = tex;
                        }
                    ));
        } else if (image){
            StartCoroutine(GetVETextureCoroutine(
                        doc.panelSettings, this.width, this.height,
                        (tex) =>
                        {
                            print("completion action invoked!");
                            // Do something with the texture here.
                            image.material.SetTexture("_SecondTex", tex);
                        }
                    ));
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

    void OnExitPlaymode()
    {
        //        doc.panelSettings.targetTexture = defaultTexture;
    }

}