using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine.SceneManagement;

public static class EditorHotkeys
{
    [ShortcutAttribute("Open Gamestate")]
    public static void FocusGameStateSO()
    {
        Selection.activeObject = GameStateVariableSingletonSO.instance.gameState;
        EditorGUIUtility.PingObject(GameStateVariableSingletonSO.instance.gameState);
    }

    // --- Scene loading hotkeys ---

    [Shortcut("Load CombatScene", KeyCode.Alpha4, ShortcutModifiers.Action)]
    public static void LoadCombatScene() =>
        LoadScene("Assets/Scenes/Encounters/CombatScene.unity");

    [Shortcut("Load PlaceholderShopEncounter", KeyCode.Alpha5, ShortcutModifiers.Action)]
    public static void LoadShopScene() =>
        LoadScene("Assets/Scenes/Encounters/PlaceholderShopEncounter.unity");

    [Shortcut("Load MainMenu", KeyCode.Alpha1, ShortcutModifiers.Action)]
    public static void LoadMainMenuScene() =>
        LoadScene("Assets/Scenes/Menus/MainMenu.unity");

    [Shortcut("Load IntroCutscene", KeyCode.Alpha2, ShortcutModifiers.Action)]
    public static void LoadIntrocutsceneScene() =>
        LoadScene("Assets/Scenes/Loading/IntroCutscene.unity");

    [Shortcut("Load StartingTeam", KeyCode.Alpha3, ShortcutModifiers.Action)]
    public static void LoadStartingTeamScene() =>
        LoadScene("Assets/Scenes/Rooms/StartingTeam.unity");

    private static void LoadScene(string scenePath)
    {
        if (Application.isPlaying)
        {
            EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
        }
        else
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(scenePath);
        }
    }
}
