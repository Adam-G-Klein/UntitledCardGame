using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;

public class testRenderTExt : MonoBehaviour
{
    public PanelSettings panelSettings1;
    public Texture2D texture;
    void Update() {
        VisualElement visualElement = new VisualElement();
        visualElement.style.backgroundColor = new StyleColor(Color.blue);
        StartCoroutine(GetVETextureCoroutine(
            panelSettings1, 1920, 1080,
            visualElement, // Or any other VisualElement.
            (tex) => {
                // Do something with the texture here.
                // For example, assign it to a RawImage component.
                RawImage rawImage = gameObject.GetComponent<RawImage>();
                rawImage.texture = tex;
            }
        ));

    }
    public IEnumerator GetVETextureCoroutine(
            PanelSettings panelSettings,
            int width, int height,
            VisualElement element, Action<Texture2D> completionAction)
        {
            // Store the existing parent (if any).
            VisualElement _existingParent = element.parent;
         
            // Create a new UIDocumment, RenderTexture, and Panel to draw to.
            GameObject obj = new GameObject();
            UIDocument doc = obj.AddComponent<UIDocument>();
            doc.panelSettings = panelSettings;
            RenderTexture rt = new RenderTexture(width, height, 32);
            doc.panelSettings.targetTexture = rt;
            doc.panelSettings.referenceResolution = new Vector2Int(width, height);
            rt.Create();
            doc.rootVisualElement.Add(element);
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
         
            // Restore the existing parent of the element, and invoke
            // any completion action specified.
            _existingParent?.Add(element);
        }
}

