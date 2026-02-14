using Unity.VisualScripting;
using UnityEngine;

// <> denotes this is a generic class
public class GenericSingleton<T> : MonoBehaviour where T : Component
{
    // create a private reference to T instance
    private static T instance;

    public static bool isDestroyed {
        get {
            return instance == null;
        }
    }

    public static T Instance
    {
        get
        {
            // if instance is null
            if (instance == null)
            {
                // find the generic instance
                T[] objs = FindObjectsOfType<T>();

                if (objs.Length > 0)
                {
                    instance = objs[0];
                }

                if (objs.Length > 1)
                {
                    for (int i = 1; i < objs.Length; i++)
                    {
                        Debug.LogError("Found more than one singleton, all but one are being destroyed.");
                        Destroy(objs[i].gameObject);
                    }
                }

                // if it's null again create a new object
                // and attach the generic instance
                if (instance == null)
                {
                    Debug.LogError("Found a null singleton, creating a new one. This should not be happening.");
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    instance = obj.AddComponent<T>();
                }
            }
            return instance;
        }
    }
    
    public static T CheckInstance
    {
        get
        {
            return instance;
        }
    }
}