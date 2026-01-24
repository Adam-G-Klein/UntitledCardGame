using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System;

public class GenerateIconDescriptionsWindow : EditorWindow
{
    private OpenAI.OpenAIApi openai = new OpenAI.OpenAIApi();
    private string selectedFolderPath = "";
    private List<string> assetPaths = new List<string>();
    private int currentAssetIndex = 0;
    private string currentAssetDescription = "";
    private string currentAssetIconDescription = "";
    private string llmOutput = "";
    private string userFeedback = "";
    private string systemPrompt = @"
You are generating IconDescriptions for a game.

TokenType enum:
0 = Text
1 = NewLine
2 = Icon

DescriptionIconType enum:
0 = None
1 = Attack
2 = Block
3 = Draw
4 = Random
5 = Adjacent
6 = Leftmost
7 = Strength
8 = TemporaryStrength
9 = Bleed
10 = Energy
11 = Self
12 = SelfAndAdjacent
13 = Discard
14 = Exhaust
15 = WhenDiscarded
16 = WhenExhausted
17 = InHandEndOfTurn

Rules:
- Convert the Description into an IconDescription YAML list
- Use tokenType 0 for text, 1 for newline, 2 for icon
- Insert a newline when there is an '&' or a new sentence
- Only output YAML
- Do NOT explain anything
- When there is a duplicate effect, e.g. (draw 1 from 3 random rats), repeat the icon instead of using text to explain it.
- Refer to the examples below for how to convert descriptions into IconDescriptions.
- Make sure it begins with the YAML key 'IconDescription:'
- Never end with a newline token
- For examples like 'Give any rat 3 block', output the number as text token '3' and then the block icon. Omit the 'any rat' part, that's implicit.
- If there is no icon for the effect, just use text tokens.
- For text such as 'When exhausted' or 'When discarded', output the WhenDiscarded or WhenExhausted icon, then a text colon ':' token afterwards.

Examples:
Description: Draw 1 card from 3 random rats
IconDescription:
- tokenType: 2
text:
icon: 3
- tokenType: 2
text:
icon: 3
- tokenType: 2
text:
icon: 3
- tokenType: 2
text:
icon: 4

Description: Deal {rpl_damage} damage & bleed self and adjacent 1 HP
IconDescription:
- tokenType: 0
text: '{rpl_damage}'
icon: 0
- tokenType: 2
text:
icon: 1
- tokenType: 1
text:
icon: 0
- tokenType: 0
text: 1
icon: 0
- tokenType: 2
text:
icon: 9
- tokenType: 2
text:
icon: 12

Description: 'Discard 2 cards in hand & deal {rpl_damage} damage '
IconDescription:
- tokenType: 2
text:
icon: 13
- tokenType: 2
text:
icon: 13
- tokenType: 1
text:
icon: 0
- tokenType: 0
text: '{rpl_damage}'
icon: 0
- tokenType: 2
text:
icon: 1

Description: 'Unplayable. End of turn, give leftmost rat 2 block. When exhausted, give leftmost rat 6 block'
IconDescription:
- tokenType: 0
  text: 'Unplayable.'
  icon: 0
- tokenType: 1
  text: ''
  icon: 0
- tokenType: 2
  text: ''
  icon: 17
- tokenType: 0
  text: ':'
  icon: 0
- tokenType: 0
  text: '2'
  icon: 0
- tokenType: 2
  text: ''
  icon: 2
- tokenType: 2
  text: ''
  icon: 6
- tokenType: 1
  text: ''
  icon: 0
- tokenType: 2
  text: ''
  icon: 16
- tokenType: 0
  text: ':'
  icon: 0
- tokenType: 0
  text: '6'
  icon: 0
- tokenType: 2
  text: ''
  icon: 2
- tokenType: 2
  text: ''
  icon: 6
";

    private Vector2 scrollPositionOriginal;
    private Vector2 scrollPositionIconDescription;
    private Vector2 scrollPositionOutput;
    private Vector2 scrollPositionFeedback;
    private bool isProcessing = false;

    private CardType currentCardType;

    List<OpenAI.ChatMessage> messages = new List<OpenAI.ChatMessage>();


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
        systemPrompt = EditorGUILayout.TextArea(systemPrompt, GUILayout.Height(120));

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
            GUILayout.Label("Original Description:");
            scrollPositionOriginal = EditorGUILayout.BeginScrollView(scrollPositionOriginal, GUILayout.Height(50));
            EditorGUILayout.TextArea(currentAssetDescription, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            GUILayout.Label("Current Icon Description:");
            scrollPositionIconDescription = EditorGUILayout.BeginScrollView(scrollPositionIconDescription, GUILayout.Height(100));
            EditorGUILayout.TextArea(currentAssetIconDescription, GUILayout.ExpandHeight(true));
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

        string[] guids = AssetDatabase.FindAssets("t:CardType", new[] { selectedFolderPath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            assetPaths.Add(path);
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
            CardType cardType = AssetDatabase.LoadAssetAtPath<CardType>(path);
            if (cardType != null)
            {
                currentAssetDescription = cardType.Description;
                currentAssetIconDescription = cardType.IconDescription != null ? IconDescriptionToString(cardType.IconDescription) : "";
                currentCardType = cardType;
            }
            llmOutput = "";
            userFeedback = "";
        }
        Repaint();
    }

    private string IconDescriptionToString(List<DescriptionToken> iconDescription)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("IconDescription:");
        foreach (var token in iconDescription)
        {
            sb.AppendLine("- tokenType: " + (int)token.tokenType);
            sb.AppendLine("  text: '" + token.text.Replace("'", "''") + "'");
            sb.AppendLine("  icon: " + (int)token.icon);
        }
        return sb.ToString();
    }

    private async System.Threading.Tasks.Task SendReply()
    {
        // Complete the instruction
        var completionResponse = await openai.CreateChatCompletion(new OpenAI.CreateChatCompletionRequest()
        {
            Model = "gpt-4o-mini",
            Messages = messages
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();

            Debug.Log("LLM Response: " + message.Content);

            messages.Add(message);
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }
    }


    private async void GenerateLLMOutput()
    {
        // TODO: Replace with actual LLM API call
        // This is a placeholder that simulates LLM output
        isProcessing = true;

        // Example: You would call your LLM API here
        // For now, this just echoes the content with a note
        messages = new List<OpenAI.ChatMessage>();
        var systemMessage = new OpenAI.ChatMessage()
        {
            Role = "system",
            Content = systemPrompt
        };
        messages.Add(systemMessage);


        var newMessage = new OpenAI.ChatMessage()
        {
            Role = "user",
            Content = "Description:\n" + currentAssetDescription + "\n\nPlease generate the IconDescription YAML as per the system prompt, omit tick marks, just give the raw YAML"
        };
        messages.Add(newMessage);

        await SendReply();

        llmOutput = messages[messages.Count - 1].Content;

        isProcessing = false;
        Repaint();
    }

    private async void RegenerateWithFeedback()
    {
        // TODO: Replace with actual LLM API call including feedback
        isProcessing = true;

        OpenAI.ChatMessage feedbackMessage = new OpenAI.ChatMessage()
        {
            Role = "user",
            Content = "Please regenerate the IconDescription with the following feedback:\n" + userFeedback
        };

        messages.Add(feedbackMessage);

        await SendReply();

        // Example: Include user feedback in the regeneration
        llmOutput = messages[messages.Count - 1].Content;

        isProcessing = false;
        Repaint();
    }

    private List<DescriptionToken> ParseLLMOutput(string llmOutput)
    {
        var result = new List<DescriptionToken>();

        if (string.IsNullOrWhiteSpace(llmOutput))
            return result;

        DescriptionToken current = null;

        var lines = llmOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            // Ignore header
            if (line.StartsWith("IconDescription:", StringComparison.OrdinalIgnoreCase))
                continue;

            // Start of a new token
            if (line.StartsWith("- tokenType:"))
            {
                // Flush previous token
                if (current != null)
                    result.Add(current);

                current = new DescriptionToken();

                int tokenTypeValue = ParseIntAfterColon(line);
                current.tokenType = Enum.IsDefined(typeof(DescriptionToken.TokenType), tokenTypeValue)
                    ? (DescriptionToken.TokenType)tokenTypeValue
                    : DescriptionToken.TokenType.Text;

                continue;
            }

            if (current == null)
                continue;

            if (line.StartsWith("text:"))
            {
                current.text = ParseStringAfterColon(line);
                continue;
            }

            if (line.StartsWith("icon:"))
            {
                int iconValue = ParseIntAfterColon(line);
                current.icon = Enum.IsDefined(typeof(DescriptionToken.DescriptionIconType), iconValue)
                    ? (DescriptionToken.DescriptionIconType)iconValue
                    : DescriptionToken.DescriptionIconType.None;
                continue;
            }
        }

        // Flush last token
        if (current != null)
            result.Add(current);

        return result;
    }

    private static int ParseIntAfterColon(string line)
    {
        int colon = line.IndexOf(':');
        if (colon < 0)
            return 0;

        var value = line.Substring(colon + 1).Trim();

        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0;
    }

    private static string ParseStringAfterColon(string line)
    {
        int colon = line.IndexOf(':');
        if (colon < 0)
            return string.Empty;

        var value = line.Substring(colon + 1).Trim();

        // Handle empty text:
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Strip single quotes if present
        if (value.Length >= 2 && value[0] == '\'' && value[^1] == '\'')
            value = value.Substring(1, value.Length - 2);

        return value;
    }


    private void ApplyChanges()
    {
        if (currentAssetIndex >= 0 && currentAssetIndex < assetPaths.Count && !string.IsNullOrEmpty(llmOutput))
        {
            string path = assetPaths[currentAssetIndex];
            CardType cardType = AssetDatabase.LoadAssetAtPath<CardType>(path);
            if (cardType != null)
            {
                // Here you would parse llmOutput and update the cardType's IconDescription property
                // For demonstration, we will just log the output
                Debug.Log($"Applying changes to {path}:\n{llmOutput}");

                cardType.IconDescription = ParseLLMOutput(llmOutput); // Placeholder assignment

                // Example: cardType.IconDescription = parsedIconDescription;
                EditorUtility.SetDirty(cardType);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
