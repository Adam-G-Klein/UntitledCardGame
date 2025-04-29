using UnityEngine;

public static class ScriptableObjectLoader
{
    private const string SCRIPTABLE_OBJECTS_PATH = "ScriptableObjects/";

    /// <summary>
    /// Loads a ScriptableObject of type T from the Resources/ScriptableObjects directory
    /// </summary>
    /// <typeparam name="T">The type of ScriptableObject to load</typeparam>
    /// <param name="name">The name of the ScriptableObject to load</param>
    /// <returns>The loaded ScriptableObject, or null if not found</returns>
    public static T Load<T>(string path, string name) where T : ScriptableObject
    {
        T loadedObject = RecursiveLoad<T>(SCRIPTABLE_OBJECTS_PATH + path, name);
        
        if (loadedObject == null)
        {
            Debug.LogError($"Failed to load ScriptableObject of type {typeof(T)} with name {name} after searching all subdirectories of {SCRIPTABLE_OBJECTS_PATH}");
        }
        
        return loadedObject;
    }

    private static T RecursiveLoad<T>(string currentPath, string name) where T : ScriptableObject
    {
        // Try loading from current path
        T loadedObject = Resources.Load<T>(currentPath);
        if (loadedObject != null)
        {
            return loadedObject;
        }

        // Get all subdirectories at current path
        Object[] subDirs = Resources.LoadAll(currentPath, typeof(Object));
        
        // Subdirs are actually also the subfiles, so one of these could be what we're looking for
        foreach (Object subDir in subDirs)
        {
            string subPath = currentPath + "/" + subDir.name; 
            T result = RecursiveLoad<T>(subPath, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
} 