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
    // Instead of doing workflow interrupts on the global EffectManager class,
    // let's do it on the effect workflow itself by storing something in the document
    public bool workflowInterrupted = false;

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

    public void printBoolMap() {
        foreach (KeyValuePair<string, bool> pair in boolMap) {
            Debug.Log("Key: " + pair.Key + "\nvalue: " + pair.Value.ToString());
        }
    }

    public class GenericListDictionary {
        private Dictionary<Tuple<string, Type>, List<object>> _dict = new Dictionary<Tuple<string, Type>, List<object>>();
        private Dictionary<Tuple<string, Type>, string> _linkDict = new Dictionary<Tuple<string, Type>, string>();

        public Dictionary<Tuple<string, Type>, List<object>> GetDict() {
            return _dict;
        }

        public void AddItem<T>(string key, T value, string link = null) where T : class {
            Tuple<string, Type> dictKey = new Tuple<string, Type>(key, typeof(T));
            if (!_dict.ContainsKey(dictKey)) {
                _dict[dictKey] = new List<object>();
            }
            _dict[dictKey].Add(value);
            if (link != null) {
                _linkDict[dictKey] = link;
            }
        }

        public void AddItems<T>(string key, List<T> items, string link = null) where T : class {
            foreach (T item in items) {
                AddItem<T>(key, item, link);
            }
        }

        public void AddItemOfType(string key, object value, Type type, string link = null) {
            Tuple<string, Type> dictKey = new Tuple<string, Type>(key, type);
            if (!_dict.ContainsKey(dictKey)) {
                _dict[dictKey] = new List<object>();
            }
            _dict[dictKey].Add(value);
            if (link != null) {
                _linkDict[dictKey] = link;
            }
        }

        public void AddItemsOfType(string key, List<object> items, Type type, string link = null) {
            foreach (object item in items) {
                AddItemOfType(key, item, type, link);
            }
        }

        public T TryGetItem<T>(string key, int index) where T : class {
            Tuple<string, Type> dictKey = new Tuple<string, Type>(key, typeof(T));
            if (!_dict.ContainsKey(dictKey)) {
                return default(T);
            }

            if (index >= _dict[dictKey].Count) {
                return default(T);
            }

            return _dict[dictKey][index] as T;
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

        public List<T> TryGetList<T>(string key) where T : class {
            if(ContainsValueWithKey<T>(key)) {
                return GetList<T>(key);
            } else {
                return new List<T>();
            }
        }

        public bool ContainsValueWithKey<T>(string key) {
            Tuple<string, Type> dictKey = new Tuple<string, Type>(key, typeof(T));
            if (_dict.ContainsKey(dictKey) && _dict[dictKey].Count > 0) {
                return true;
            }
            return false;
        }

        public int CountItemsWithKeyString(string key) {
            int count = 0;
            HashSet<string> dedupeLinks = new HashSet<string>();
            foreach (KeyValuePair<Tuple<string, Type>, List<object>> keyValuePair in _dict) {
                if (keyValuePair.Key.Item1.Equals(key)) {
                    // If the keyValuePair is not part of a linked group, count all the elements as normal.
                    // Otherwise, we only want to count the number of elements of a linked group once.
                    if (!_linkDict.ContainsKey(keyValuePair.Key)) {
                        count += keyValuePair.Value.Count;
                    } else if (!dedupeLinks.Contains(_linkDict[keyValuePair.Key])) {
                        dedupeLinks.Add(_linkDict[keyValuePair.Key]);
                        count += keyValuePair.Value.Count;
                    }
                }
            }
            return count;
        }

        public List<Type> GetTypesWithKey(string key) {
            List<Type> returnList = new List<Type>();
            foreach (KeyValuePair<Tuple<string, Type>, List<object>> keyValuePair in _dict) {
                if (keyValuePair.Key.Item1.Equals(key)) {
                    returnList.Add(keyValuePair.Key.Item2);
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
