using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class GenerateIconDescriptionsWindow : EditorWindow
{
    private string selectedFolderPath = "";
    private List<string> assetPaths = new List<string>();
    private int currentAssetIndex = 0;
    private string currentAssetContent = "";
    private string llmOutput = "";
    private string userFeedback = "";
    private string systemPrompt = "You are an assistant that modifies asset files. Respond with the modified content.";
    private Vector2 scrollPositionOriginal;
    private Vector2 scrollPositionOutput;
    private Vector2 scrollPositionFeedback;
    private bool isProcessing = false;

    [MenuItem("Tools/Generate Icon Descriptions")]
    public static void ShowWindow()
    {
        GetWindow<GenerateIconDescriptionsWindow>("Generate Icon Descriptions");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Icon Descriptions", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField("Folder Path:", selectedFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    selectedFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                    LoadAssetsFromFolder();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please select a folder within the Assets directory.", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUILayout.Label("System Prompt:");
        systemPrompt = EditorGUILayout.TextArea(systemPrompt, GUILayout.Height(60));

        if (assetPaths.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Asset {currentAssetIndex + 1} of {assetPaths.Count}: {Path.GetFileName(assetPaths[currentAssetIndex])}");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous") && currentAssetIndex > 0)
            {
                currentAssetIndex--;
                LoadCurrentAsset();
            }
            if (GUILayout.Button("Next") && currentAssetIndex < assetPaths.Count - 1)
            {
                currentAssetIndex++;
                LoadCurrentAsset();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILayout.Label("Original Content:");
            scrollPositionOriginal = EditorGUILayout.BeginScrollView(scrollPositionOriginal, GUILayout.Height(150));
            EditorGUILayout.TextArea(currentAssetContent, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            GUILayout.Label("LLM Output:");
            scrollPositionOutput = EditorGUILayout.BeginScrollView(scrollPositionOutput, GUILayout.Height(150));
            llmOutput = EditorGUILayout.TextArea(llmOutput, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            GUILayout.Label("Your Feedback (for regeneration):");
            scrollPositionFeedback = EditorGUILayout.BeginScrollView(scrollPositionFeedback, GUILayout.Height(60));
            userFeedback = EditorGUILayout.TextArea(userFeedback, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !isProcessing;
            if (GUILayout.Button("Generate"))
            {
                GenerateLLMOutput();
            }
            if (GUILayout.Button("Regenerate with Feedback"))
            {
                RegenerateWithFeedback();
            }
            if (GUILayout.Button("Apply Changes"))
            {
                ApplyChanges();
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (isProcessing)
            {
                EditorGUILayout.HelpBox("Processing... Please wait.", MessageType.Info);
            }
        }
    }

    private void LoadAssetsFromFolder()
    {
        assetPaths.Clear();
        currentAssetIndex = 0;

        string[] guids = AssetDatabase.FindAssets("", new[] { selectedFolderPath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (File.Exists(path) && !AssetDatabase.IsValidFolder(path))
            {
                assetPaths.Add(path);
            }
        }

        if (assetPaths.Count > 0)
        {
            LoadCurrentAsset();
        }
    }

    private void LoadCurrentAsset()
    {
        if (currentAssetIndex >= 0 && currentAssetIndex < assetPaths.Count)
        {
            string path = assetPaths[currentAssetIndex];
            try
            {
                currentAssetContent = File.ReadAllText(path);
            }
            catch
            {
                currentAssetContent = "[Binary or unreadable file]";
            }
            llmOutput = "";
            userFeedback = "";
        }
    }

    private void GenerateLLMOutput()
    {
        // TODO: Replace with actual LLM API call
        // This is a placeholder that simulates LLM output
        isProcessing = true;

        // Example: You would call your LLM API here
        // For now, this just echoes the content with a note
        llmOutput = $"[LLM would process this content with prompt: {systemPrompt}]\n\n{currentAssetContent}";

        isProcessing = false;
        Repaint();
    }

    private void RegenerateWithFeedback()
    {
        // TODO: Replace with actual LLM API call including feedback
        isProcessing = true;

        // Example: Include user feedback in the regeneration
        llmOutput = $"[LLM regenerated with feedback: {userFeedback}]\n\n{currentAssetContent}";

        isProcessing = false;
        Repaint();
    }

    private void ApplyChanges()
    {
        if (currentAssetIndex >= 0 && currentAssetIndex < assetPaths.Count && !string.IsNullOrEmpty(llmOutput))
        {
            string path = assetPaths[currentAssetIndex];
            if (EditorUtility.DisplayDialog("Confirm", $"Apply changes to {Path.GetFileName(path)}?", "Yes", "No"))
            {
                File.WriteAllText(path, llmOutput);
                AssetDatabase.Refresh();
                currentAssetContent = llmOutput;
                EditorUtility.DisplayDialog("Success", "Changes applied successfully.", "OK");
            }
        }
    }
}
