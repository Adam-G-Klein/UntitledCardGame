using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EffectDocument
{
    // Used to set/retrieve the origin of series of effects from a map
    // For example: the card being cast or companion who's ability is triggered
    public static string ORIGIN = "Origin";
    public EntityType originEntityType = EntityType.Unknown;
    public GenericListDictionary map = new GenericListDictionary();
    
    public Dictionary<string, int> intMap = new Dictionary<string, int>();
    
    public Dictionary<string, string> stringMap = new Dictionary<string, string>();

    public Dictionary<string, bool> boolMap = new Dictionary<string, bool>();

    public List<GameObject> GetGameObjects(string key) {
        List<GameObject> returnList = new List<GameObject>();
        foreach(KeyValuePair<Tuple<string, Type>, List<object>> pair in map.GetDict()) {
            if (pair.Key.Item1 == key) {
                if (pair.Key.Item2.IsSubclassOf(typeof(MonoBehaviour))) {
                    pair.Value.ForEach(item => returnList.Add(((MonoBehaviour)item).gameObject));
                }
            }
        }

        return returnList;
    }

    public void printIntMap() {
        foreach (KeyValuePair<string, int> pair in intMap) {
            Debug.Log("Key: " + pair.Key + "\nvalue: " + pair.Value);
        }
    }

    public void printStringMap() {
        foreach (KeyValuePair<string, string> pair in stringMap) {
            Debug.Log("Key: " + pair.Key + "\nvalue: " + pair.Value);
        }
    }

    public class GenericListDictionary {
        private Dictionary<Tuple<string, Type>, List<object>> _dict = new Dictionary<Tuple<string, Type>, List<object>>();

        public Dictionary<Tuple<string, Type>, List<object>> GetDict() {
            return _dict;
        }

        public void AddItem<T>(string key, T value) where T : class {
            Tuple<string, Type> dictKey = new Tuple<string, Type>(key, typeof(T));
            if (!_dict.ContainsKey(dictKey)) {
                _dict[dictKey] = new List<object>();
            }
            _dict[dictKey].Add(value);
        }

        public void AddItems<T>(string key, List<T> items) where T : class {
            foreach (T item in items) {
                AddItem<T>(key, item);
            }
        }

        public T GetItem<T>(string key, int index) where T : class {
            Tuple<string, Type> dictKey = new Tuple<string, Type>(key, typeof(T));
            if (!_dict.ContainsKey(dictKey)) {
                Debug.LogError("Key " + dictKey + " not found in dictionary!");
                return default(T);
            }

            if (index >= _dict[dictKey].Count) {
                return default(T);
            }

            return _dict[dictKey][index] as T;
        }

        public List<T> GetList<T>(string key) where T : class {
            Tuple<string, Type> dictKey = new Tuple<string, Type>(key, typeof(T));
            if (!_dict.ContainsKey(dictKey)) {
                Debug.LogError("Key " + dictKey + " not found in dictionary!");
                return new List<T>();
            }

            List<T> returnList = new List<T>();
            foreach (object item in _dict[dictKey]) {
                returnList.Add(item as T);
            }
            return returnList;
        }

        public bool ContainsValueWithKey<T>(string key) {
            Tuple<string, Type> dictKey = new Tuple<string, Type>(key, typeof(T));
            if (_dict.ContainsKey(dictKey) && _dict[dictKey].Count > 0) {
                return true;
            }
            return false;
        }
        
        public List<object> GetAllItemsWithKeyString(string key) {
            List<object> returnList = new List<object>();
            foreach (KeyValuePair<Tuple<string, Type>, List<object>> keyValuePair in _dict) {
                if (keyValuePair.Key.Item1.Equals(key)) {
                    returnList.AddRange(keyValuePair.Value);
                }
            }
            return returnList;
        }

        public void Print() {
            foreach (KeyValuePair<Tuple<string, Type>, List<object>> pair in _dict) {
                Debug.Log("Key: " + pair.Key.Item1 + 
                    "\nType: " + pair.Key.Item2 + "\nValue: " + pair.Value);
            }
        }
    }
}
