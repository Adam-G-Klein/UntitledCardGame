using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;

public class EditorHotkeys 
{

    [ShortcutAttribute("Open Gamestate")]
    public static void FocusGameStateSO()
    {
        Selection.activeObject = GameStateVariableSO.instance;
        EditorGUIUtility.PingObject(GameStateVariableSO.instance);
    }
}
