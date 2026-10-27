using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Edit-mode preview of the scene transition ink wipe (Tools > VFX > Ink Wipe Preview).
// Renders the wipe through its own hidden camera into a texture shown in this window, so overlay UI in the
// open scene can't cover it and nothing in the scene is touched. Material values and shader graph edits
// show up live; the animated cutoff only ever touches a hidden copy of the material.
public class InkWipePreviewWindow : EditorWindow
{
    const string DefaultMaterialPath = "Assets/VFX V2/New VFX/Screen Fade/CM_SceneTransition.mat";
    const int UILayer = 5;

    static readonly int CutoffId = Shader.PropertyToID("_Cutoff");
    static readonly int SoftnessId = Shader.PropertyToID("_Softness");
    static readonly int LeftRightId = Shader.PropertyToID("_Left_Right");
    static readonly int FadeWidthId = Shader.PropertyToID("_Fade_Width");

    Material wipeMaterial;
    float revealTime = 0.45f;
    float holdTime = 0.3f;
    bool coverLeftToRight = true;
    bool revealContinuesAcross = true;
    float revealClearCutoff = 0.52f;
    bool animate = true;
    float scrub = 0.25f;
    bool showSceneBehind = true;
    Color backgroundColour = new Color(0.35f, 0.4f, 0.45f);

    Camera previewCam;
    Canvas previewCanvas;
    RawImage previewImage;
    Material previewMaterial;
    RenderTexture rt;
    double animStart;

    [MenuItem("Tools/VFX/Ink Wipe Preview")]
    static void Open() => GetWindow<InkWipePreviewWindow>("Ink Wipe Preview");

    void OnEnable()
    {
        if (wipeMaterial == null) wipeMaterial = AssetDatabase.LoadAssetAtPath<Material>(DefaultMaterialPath);
        animStart = EditorApplication.timeSinceStartup;
        EditorApplication.update += Tick;
    }

    void OnDisable()
    {
        EditorApplication.update -= Tick;
        DestroyRig();
    }

    void Tick()
    {
        if (animate) Repaint();
    }

    void OnGUI()
    {
        wipeMaterial = (Material)EditorGUILayout.ObjectField("Wipe Material", wipeMaterial, typeof(Material), false);
        revealTime = EditorGUILayout.FloatField("Reveal Time", revealTime);
        holdTime = EditorGUILayout.FloatField("Hold Time", holdTime);
        coverLeftToRight = EditorGUILayout.Toggle("Cover Left To Right", coverLeftToRight);
        revealContinuesAcross = EditorGUILayout.Toggle("Reveal Continues Across", revealContinuesAcross);
        revealClearCutoff = EditorGUILayout.FloatField("Reveal Clear Cutoff", revealClearCutoff);
        animate = EditorGUILayout.Toggle("Animate", animate);
        using (new EditorGUI.DisabledScope(animate))
            scrub = EditorGUILayout.Slider(new GUIContent("Scrub", "0 clear, 0.5 fully covered, 1 clear again"), scrub, 0f, 1f);
        showSceneBehind = EditorGUILayout.Toggle(new GUIContent("Show Scene Behind", "Render the main camera's view behind the wipe (world only, no UI)"), showSceneBehind);
        if (!showSceneBehind || Camera.main == null)
            backgroundColour = EditorGUILayout.ColorField("Background", backgroundColour);

        if (wipeMaterial == null)
        {
            EditorGUILayout.HelpBox("Assign the wipe material (CM_SceneTransition).", MessageType.Info);
            return;
        }
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            DestroyRig();
            EditorGUILayout.HelpBox("Paused in Play mode. Use the Wipe Transition Preview component's Play Wipe button there.", MessageType.Info);
            return;
        }

        float cutoff;
        bool leftToRight;
        float phase = Evaluate(out cutoff, out leftToRight);
        EditorGUILayout.LabelField(" ", $"cutoff {cutoff:F3}   {(phase < 0.5f ? "covering" : "revealing")}   {(leftToRight ? "L->R" : "R->L")}");

        Rect area = GUILayoutUtility.GetRect(position.width - 8f, position.width - 8f, 50f, 4000f, GUILayout.ExpandHeight(true));
        float w = area.width, h = w * 9f / 16f;
        if (h > area.height) { h = area.height; w = h * 16f / 9f; }
        Rect view = new Rect(area.x + (area.width - w) * 0.5f, area.y, w, h);
        if (Event.current.type != EventType.Repaint || w < 8f) return;

        EnsureRig((int)w, (int)h);
        previewMaterial.shader = wipeMaterial.shader;
        previewMaterial.CopyPropertiesFromMaterial(wipeMaterial);
        previewMaterial.SetFloat(CutoffId, cutoff);
        previewMaterial.SetFloat(LeftRightId, leftToRight ? 1f : 0f);
        previewImage.texture = wipeMaterial.mainTexture;
        previewImage.SetMaterialDirty();

        Camera main = Camera.main;
        if (showSceneBehind && main != null)
        {
            previewCam.CopyFrom(main);
            previewCam.cullingMask |= 1 << UILayer;
        }
        else
        {
            previewCam.transform.SetPositionAndRotation(new Vector3(0f, 0f, -10f), Quaternion.identity);
            previewCam.orthographic = true;
            previewCam.cullingMask = 1 << UILayer;
            previewCam.clearFlags = CameraClearFlags.SolidColor;
            previewCam.backgroundColor = backgroundColour;
        }
        previewCam.targetTexture = rt;
        previewCam.enabled = true;
        previewCanvas.worldCamera = previewCam;
        previewCanvas.planeDistance = previewCam.nearClipPlane + 0.1f;

        Canvas.ForceUpdateCanvases();
        previewCam.Render();
        GUI.DrawTexture(view, rt, ScaleMode.StretchToFill, false);
    }

    // Position in the cover -> hold -> reveal sequence, matching ScreenFader.Wipe's smoothstep easing.
    float Evaluate(out float cutoff, out bool leftToRight)
    {
        float total = revealTime * 2f + holdTime;
        float t = animate
            ? (float)((EditorApplication.timeSinceStartup - animStart) % (total + 0.5f))
            : (scrub < 0.5f ? scrub * 2f * revealTime : revealTime + holdTime + (scrub - 0.5f) * 2f * revealTime);

        float fadeWidth = wipeMaterial.HasProperty(FadeWidthId) ? wipeMaterial.GetFloat(FadeWidthId) : 0f;
        float hidden = -(wipeMaterial.GetFloat(SoftnessId) + fadeWidth);
        if (t < revealTime)
        {
            cutoff = Mathf.Lerp(revealClearCutoff, hidden, Smooth(t / revealTime));
            leftToRight = coverLeftToRight;
            return 0f;
        }
        if (t < revealTime + holdTime)
        {
            cutoff = hidden;
            leftToRight = coverLeftToRight;
            return 0.49f;
        }
        cutoff = Mathf.Lerp(hidden, revealClearCutoff, Smooth((t - revealTime - holdTime) / revealTime));
        leftToRight = revealContinuesAcross ? !coverLeftToRight : coverLeftToRight;
        return 1f;
    }

    static float Smooth(float p)
    {
        p = Mathf.Clamp01(p);
        return p * p * (3f - 2f * p);
    }

    void EnsureRig(int width, int height)
    {
        if (rt == null || rt.width != width || rt.height != height)
        {
            if (rt != null) { rt.Release(); DestroyImmediate(rt); }
            rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { hideFlags = HideFlags.HideAndDontSave };
        }
        if (previewCam == null)
        {
            DestroyRig(keepTexture: true);
            previewCam = new GameObject("InkWipePreviewCamera") { hideFlags = HideFlags.HideAndDontSave }.AddComponent<Camera>();
            previewCam.targetTexture = rt;

            GameObject canvasObj = new GameObject("InkWipePreviewCanvas") { hideFlags = HideFlags.HideAndDontSave, layer = UILayer };
            previewCanvas = canvasObj.AddComponent<Canvas>();
            previewCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            previewCanvas.sortingLayerName = "Tooltips";
            previewCanvas.sortingOrder = 999;

            GameObject imageObj = new GameObject("InkWipePreviewImage") { hideFlags = HideFlags.HideAndDontSave, layer = UILayer };
            imageObj.transform.SetParent(canvasObj.transform, false);
            previewImage = imageObj.AddComponent<RawImage>();
            previewImage.raycastTarget = false;
            RectTransform rect = previewImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            previewMaterial = new Material(wipeMaterial) { hideFlags = HideFlags.HideAndDontSave };
            previewImage.material = previewMaterial;
        }
    }

    void DestroyRig(bool keepTexture = false)
    {
        if (previewCanvas != null) DestroyImmediate(previewCanvas.gameObject);
        if (previewCam != null) DestroyImmediate(previewCam.gameObject);
        if (previewMaterial != null) DestroyImmediate(previewMaterial);
        previewCanvas = null;
        previewImage = null;
        previewCam = null;
        previewMaterial = null;
        if (!keepTexture && rt != null)
        {
            rt.Release();
            DestroyImmediate(rt);
            rt = null;
        }
    }
}
