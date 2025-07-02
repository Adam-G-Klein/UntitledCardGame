using UnityEditor;
using UnityEngine;

[FilePath("ProjectSettings/GameStateSingletonSO.asset", FilePathAttribute.Location.ProjectFolder)]
public class GameStateVariableSingletonSO : ScriptableSingleton<GameStateVariableSingletonSO>
{
    public GameStateVariableSO gameState;

    public void SaveSettings()
    {
        Save(true);
    }

    [MenuItem("Tools/Open GameStateVariableSingletonSO")]
    public static void Open() {
        Selection.activeObject = GameStateVariableSingletonSO.instance;
        EditorGUIUtility.PingObject(GameStateVariableSingletonSO.instance);
    }
}