using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;

public static class EditorHotkeys
{

    [ShortcutAttribute("Open Gamestate")]
    public static void FocusGameStateSO()
    {
        Selection.activeObject = GameStateVariableSingletonSO.instance.gameState;
        EditorGUIUtility.PingObject(GameStateVariableSingletonSO.instance.gameState);
    }
}
