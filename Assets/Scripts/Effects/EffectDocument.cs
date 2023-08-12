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
    
    public ListMap<CompanionInstance> companionMap = new ListMap<CompanionInstance>();
    
    public ListMap<MinionInstance> minionMap = new ListMap<MinionInstance>();

    public ListMap<EnemyInstance> enemyMap = new ListMap<EnemyInstance>();

    public ListMap<PlayableCard> playableCardMap = new ListMap<PlayableCard>();
    
    public ListMap<Card> cardMap = new ListMap<Card>();

    public ListMap<UICard> uiCardMap = new ListMap<UICard>();

    public Dictionary<string, int> intMap = new Dictionary<string, int>();
    
    public Dictionary<string, string> stringMap = new Dictionary<string, string>();

    public Dictionary<string, bool> boolMap = new Dictionary<string, bool>();

    public List<CombatInstance> GetCombatInstances(string key) {
        List<CombatInstance> returnList = new List<CombatInstance>();
        if (companionMap.containsValueWithKey(key)) {
            companionMap.getList(key).ForEach(companion => 
                returnList.Add(companion.combatInstance));
        }

        if (minionMap.containsValueWithKey(key)) {
            minionMap.getList(key).ForEach(minion => 
                returnList.Add(minion.combatInstance));
        }

        if (enemyMap.containsValueWithKey(key)) {
            enemyMap.getList(key).ForEach(enemy =>
                returnList.Add(enemy.combatInstance));
        }

        if (map.ContainsValueWithKey<CombatInstance>(key)) {
            map.GetList<CombatInstance>(key).ForEach(instance =>
                returnList.Add(instance));
        }

        return returnList;
    }

    public List<DeckInstance> GetDeckInstances(string key) {
        List<DeckInstance> returnList = new List<DeckInstance>();
        if (companionMap.containsValueWithKey(key)) {
            companionMap.getList(key).ForEach(companion => 
                returnList.Add(companion.deckInstance));
        }

        if (minionMap.containsValueWithKey(key)) {
            minionMap.getList(key).ForEach(minion => 
                returnList.Add(minion.deckInstance));
        }

        return returnList;
    }

    public List<GameObject> GetGameObjects(string key) {
        List<GameObject> returnList = new List<GameObject>();
        if (companionMap.containsValueWithKey(key)) {
            companionMap.getList(key).ForEach(companion => 
                returnList.Add(companion.gameObject));
        }

        if (minionMap.containsValueWithKey(key)) {
            minionMap.getList(key).ForEach(minion => 
                returnList.Add(minion.gameObject));
        }

        if (enemyMap.containsValueWithKey(key)) {
            enemyMap.getList(key).ForEach(enemy =>
                returnList.Add(enemy.gameObject));
        }

        if (playableCardMap.containsValueWithKey(key)) {
            playableCardMap.getList(key).ForEach(playableCard =>
                returnList.Add(playableCard.gameObject));
        }

        if (map.ContainsValueWithKey<CombatInstance>(key)) {
            map.GetList<CombatInstance>(key).ForEach(instance =>
                returnList.Add(instance.gameObject));
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

    public class ListMap<T> {
        public Dictionary<string, List<T>> dict = new Dictionary<string, List<T>>();

        public void addItem(string key, T item) {
            if (!dict.ContainsKey(key)) {
                dict[key] = new List<T>();
            }
            dict[key].Add(item);
        }

        public void addItems(string key, List<T> items) {
            if (!dict.ContainsKey(key)) {
                dict[key] = new List<T>();
            }
            dict[key].AddRange(items);
        }

        public T getItem(string key, int index) {
            if (!dict.ContainsKey(key)) {
                return default(T);
            }

            if (index >= dict[key].Count) {
                return default(T);
            }

            return dict[key][index];
        }

        public List<T> getList(string key) {
            if (!dict.ContainsKey(key)) {
                return new List<T>();
            }
            
            return dict[key];
        }

        public bool containsValueWithKey(string key) {
            if (dict.ContainsKey(key) && dict[key].Count > 0) {
                return true;
            }
            return false;
        }

        public void printDictionary() {
            Debug.Log("Printing Map Contents");
            foreach (KeyValuePair<string, List<T>> pair in dict) {
                Debug.Log("Key: " + pair.Key + "\nvalue: " + pair.Value);
            }
        }
    }

    public class GenericListDictionary {
        private Dictionary<Tuple<string, Type>, List<object>> _dict = new Dictionary<Tuple<string, Type>, List<object>>();

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

        public void Print() {
            foreach (KeyValuePair<Tuple<string, Type>, List<object>> pair in _dict) {
                Debug.Log("Key: " + pair.Key.Item1 + 
                    "\nType: " + pair.Key.Item2 + "\nValue: " + pair.Value);
            }
        }
    }
}
