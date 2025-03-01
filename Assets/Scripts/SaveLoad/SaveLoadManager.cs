using UnityEngine;
using System.IO;

public class SaveLoadManager : MonoBehaviour
{
    private static string filePath => Path.Combine(Application.persistentDataPath, "save.json");

    // Save the data to a file
    public static void SaveData(GameStateVariableSO playerData)
    {
        string json = JsonUtility.ToJson(playerData);
        File.WriteAllText(filePath, json);
        Debug.Log("Game saved!");
    }

    // Load the data from a file
    public static GameStateVariableSO LoadData()
    {
        if (File.Exists(filePath))
        {
            GameStateVariableSO gameState = new GameStateVariableSO();
            string json = File.ReadAllText(filePath);
            JsonUtility.FromJsonOverwrite(json, gameState);
            return gameState;
        }
        else
        {
            return null;
        }
    }
}