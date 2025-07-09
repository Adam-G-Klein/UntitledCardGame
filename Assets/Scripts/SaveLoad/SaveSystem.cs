
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    public enum SaveType
    {
        GameState,
        Settings,
        Progress
    }

    private static readonly Dictionary<SaveType, string> savePaths = new Dictionary<SaveType, string>
    {
        { SaveType.GameState, Application.persistentDataPath + "/savefile.dat" },
        { SaveType.Settings, Application.persistentDataPath + "/settings.dat" },
        { SaveType.Progress, Application.persistentDataPath + "/progress.dat" }
    };
    
    
    public static void Save<T>(T data, SaveType saveType = SaveType.GameState)
    {
        string SavePath = savePaths[saveType];
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(SavePath, FileMode.Create))
        {
            formatter.Serialize(stream, data);
        }
    }

    public static T Load<T>(SaveType saveType = SaveType.GameState)
    {
        string SavePath = savePaths[saveType];
        Debug.Log(SavePath + (File.Exists(SavePath) ? "true": "false"));
        if (File.Exists(SavePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(SavePath, FileMode.Open))
            {
                return (T)formatter.Deserialize(stream);
            }
        }
        else
        {
            Debug.LogWarning("Save file not found.");
            return default;
        }
    }

    public static void DeleteSave(SaveType saveType = SaveType.GameState)
    {
        string SavePath = savePaths[saveType];
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }

    public static bool SaveExists(SaveType saveType = SaveType.GameState) {
        string SavePath = savePaths[saveType];
        return File.Exists(SavePath);
    }
}