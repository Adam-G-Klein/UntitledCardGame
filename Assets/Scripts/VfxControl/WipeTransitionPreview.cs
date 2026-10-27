using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Play-mode test for the scene transition ink wipe. Drop it on any GameObject in a scene with a camera
// tagged MainCamera, enter Play mode and press "Play Wipe" in the inspector. It runs the same
// cover -> hold -> reveal as SceneTransitionManager, through its own ScreenFader, without loading a scene.
// (Screen Space Overlay UI in the scene, e.g. the main menu, draws over it, as it would in game.)
// For tweaking the shader in edit mode use Tools > VFX > Ink Wipe Preview instead.
public class WipeTransitionPreview : MonoBehaviour
{
    [Tooltip("Ink wipe material (InkWipeTransition shader), e.g. CM_SceneTransition.")]
    public Material wipeMaterial;
    [Tooltip("Seconds for the ink to cover the screen, and again to pull away. The game uses 0.6.")]
    public float revealTime = 0.6f;
    [Tooltip("Seconds the screen stays covered between the two halves (stands in for the scene load).")]
    public float holdTime = 0.3f;
    public bool coverLeftToRight = true;
    [Tooltip("When on, the ink keeps travelling the same way to reveal. When off, it pulls back the way it came.")]
    public bool revealContinuesAcross = true;
    [Tooltip("Cutoff at which the screen is fully clear. Keep in step with SceneTransitionManager.")]
    public float revealClearCutoff = 0.52f;
    public bool playOnStart = false;
    public bool loop = false;

    private ScreenFader screenFader;
    private Material faderMaterial;
    private Coroutine running;

    private void Start()
    {
        if (playOnStart) PlayWipe();
    }

    [ContextMenu("Play Wipe")]
    public void PlayWipe()
    {
        if (!Application.isPlaying || wipeMaterial == null)
        {
            Debug.LogWarning("WipeTransitionPreview: enter Play mode and assign a wipe material first.");
            return;
        }
        // (Re)build the fader here rather than in Start, so a material assigned or swapped in Play mode is used.
        if (screenFader == null || faderMaterial != wipeMaterial)
        {
            if (running != null) StopCoroutine(running);
            if (screenFader != null) Destroy(screenFader.gameObject);
            GameObject faderObj = new GameObject("WipePreviewFader");
            faderObj.transform.SetParent(transform);
            screenFader = faderObj.AddComponent<ScreenFader>();
            screenFader.Initialize();
            screenFader.InitializeWipe(wipeMaterial);
            faderMaterial = wipeMaterial;
        }
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        do
        {
            yield return screenFader.Wipe(true, coverLeftToRight, revealTime, revealClearCutoff);
            yield return new WaitForSecondsRealtime(holdTime);
            bool revealLeftToRight = revealContinuesAcross ? !coverLeftToRight : coverLeftToRight;
            yield return screenFader.Wipe(false, revealLeftToRight, revealTime, revealClearCutoff);
            if (loop) yield return new WaitForSecondsRealtime(0.5f);
        } while (loop);
        running = null;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WipeTransitionPreview))]
    private class WipeTransitionPreviewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Play Wipe", GUILayout.Height(28)))
                    ((WipeTransitionPreview)target).PlayWipe();
            }
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play mode to run the wipe. To tweak the shader in edit mode, use Tools > VFX > Ink Wipe Preview.", MessageType.Info);
                if (GUILayout.Button("Open Ink Wipe Preview"))
                    EditorApplication.ExecuteMenuItem("Tools/VFX/Ink Wipe Preview");
            }
        }
    }
#endif
}
