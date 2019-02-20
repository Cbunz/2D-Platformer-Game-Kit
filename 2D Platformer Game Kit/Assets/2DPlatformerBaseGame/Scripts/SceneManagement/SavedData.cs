using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavedData : MonoBehaviour {

    protected static SavedData instance;

    public static SavedData Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<SavedData>();

            if (instance != null)
            {
                return instance;
            }

            GameObject spawnManagerGameObject = new GameObject("SpawnManager");
            instance = spawnManagerGameObject.AddComponent<SavedData>();

            return instance;
        }
    }

    protected Dictionary<string, string> stringSavedData;
    protected Dictionary<string, bool> boolSavedData;
    protected Dictionary<string, int> intSavedData;
    protected Dictionary<string, float> floatSavedData;
    protected Dictionary<string, Vector2> vector2SavedData;

    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public static string Get(string key, out string value)
    {
        if (Instance.stringSavedData.TryGetValue(key, out value))
        {
            return value;
        }
        throw new UnityException("No string data was found with the key - " + key);
    }

    public static bool Get(string key, out bool value)
    {
        if (Instance.boolSavedData.TryGetValue(key, out value))
        {
            return value;
        }
        throw new UnityException("No bool data was found with the key - " + key);
    }

    public static int Get(string key, out int value)
    {
        if (Instance.intSavedData.TryGetValue(key, out value))
        {
            return value;
        }
        throw new UnityException("No int data was found with the key = " + key);
    }

    public static float Get(string key, out float value)
    {
        if (Instance.floatSavedData.TryGetValue(key, out value))
        {
            return value;
        }
        throw new UnityException("No float data was found with the key - " + key);
    }

    public static Vector2 Get(string key, out Vector2 value)
    {
        if (Instance.vector2SavedData.TryGetValue(key, out value))
        {
            return value;
        }
        throw new UnityException("No Vector2 data was found with the key - " + key);
    }

    public static void Set(string key, string value)
    {
        if (Instance.stringSavedData.ContainsKey(key))
        {
            Instance.stringSavedData[key] = value;
        }
        else
        {
            Instance.stringSavedData.Add(key, value);
        }
    }

    public static void Set(string key, bool value)
    {
        if (Instance.boolSavedData.ContainsKey(key))
        {
            Instance.boolSavedData[key] = value;
        }
        else
        {
            Instance.boolSavedData.Add(key, value);
        }
    }

    public static void Set(string key, int value)
    {
        if (Instance.intSavedData.ContainsKey(key))
        {
            Instance.intSavedData[key] = value;
        }
        else
        {
            Instance.intSavedData.Add(key, value);
        }
    }

    public static void Set(string key, float value)
    {
        if (Instance.floatSavedData.ContainsKey(key))
        {
            Instance.floatSavedData[key] = value;
        }
        else
        {
            Instance.floatSavedData.Add(key, value);
        }
    }

    public static void Set(string key, Vector2 value)
    {
        if (Instance.vector2SavedData.ContainsKey(key))
        {
            Instance.vector2SavedData[key] = value;
        }
        else
        {
            Instance.vector2SavedData.Add(key, value);
        }
    }
}
