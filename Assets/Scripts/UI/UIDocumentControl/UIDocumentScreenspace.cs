using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(UIDocument))]
public class UIDocumentScreenspace : MonoBehaviour {

    [SerializeField]
    private UIDocument doc;
    private RawImage image;
    [SerializeField]
    private Texture2D texture {get;set;}
    [SerializeField]
    public bool stateDirty = true;

    public PickingMode pickingMode = PickingMode.Ignore;

    void Awake() {
        if(!doc) {
            Debug.LogError("UIDocumentScreenspace: No UIDocument component set on this script. Please set it from the component attached to the gameobject so we load the scene 1 frame faster :)");
        }
        // Do this in awake so individual controllers can enable clicking on the elements they care about
        UIDocumentUtils.SetAllPickingMode(doc.rootVisualElement, pickingMode);

        // Do this in Awake so we can query for element positions sooner
        UpdateRenderTexture();

        // Set all of the onclick / onhover / on interact for all elements to call state dirty as a callback
        // TODO
    }
    void Start() {
        
    }

    void Update() {
        if (stateDirty){
            UpdateRenderTexture();
        }
    }

    public void SetStateDirty(){
        stateDirty = true;
    }

    private void UpdateRenderTexture(){
        StartCoroutine(GetVETextureCoroutine(
            doc.panelSettings, Screen.width, Screen.height,
            (tex) => {
                // Do something with the texture here.
                image = gameObject.GetComponent<RawImage>();
                image.texture = tex;
                image.material.mainTexture = tex;
                stateDirty = false;
            }
        ));
    }

    public VisualElement GetVisualElement(string name){
        return doc.rootVisualElement.Q<VisualElement>(name);
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

    public void ActiveAllClickInteractions(){
        Debug.Log("Setting all picking mode to position");
        pickingMode = PickingMode.Position;
        UIDocumentUtils.RecursivelySetAllPointerEventsToCallback(doc.rootVisualElement, () => {
            SetStateDirty();
        });
    }

}