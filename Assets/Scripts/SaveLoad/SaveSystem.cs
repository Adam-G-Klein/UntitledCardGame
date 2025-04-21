
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Application.persistentDataPath + "/savefile.dat";

    public static void Save<T>(T data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(SavePath, FileMode.Create))
        {
            formatter.Serialize(stream, data);
        }
    }

    public static T Load<T>()
    {
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

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }
}