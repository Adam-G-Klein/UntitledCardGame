using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDocument
{
    // Used to set/retrieve the origin of series of effects from a map
    // For example: the card being cast or companion who's ability is triggered
    public static string ORIGIN = "Origin";
    public EntityType originEntityType = EntityType.Unknown;
    
    public ListMap<CompanionInstance> companionMap = new ListMap<CompanionInstance>();
    
    public ListMap<MinionInstance> minionMap = new ListMap<MinionInstance>();

    public ListMap<EnemyInstance> enemyMap = new ListMap<EnemyInstance>();

    public ListMap<PlayableCard> playableCardMap = new ListMap<PlayableCard>();
    
    public ListMap<Card> cardMap = new ListMap<Card>();

    public ListMap<UICard> uiCardMap = new ListMap<UICard>();

    public Dictionary<string, int> intMap = new Dictionary<string, int>();
    
    public Dictionary<string, string> stringMap = new Dictionary<string, string>();

    public List<CombatEntityInstance> getCombatEntityInstances(string key) {
        List<CombatEntityInstance> returnList = new List<CombatEntityInstance>();
        if (companionMap.containsValueWithKey(key)) {
            returnList.AddRange(companionMap.getList(key));
        }

        if (minionMap.containsValueWithKey(key)) {
            returnList.AddRange(minionMap.getList(key));
        }

        if (enemyMap.containsValueWithKey(key)) {
            returnList.AddRange(enemyMap.getList(key));
        }

        return returnList;
    }

    public List<CombatEntityWithDeckInstance> getCombatEntitiesWithDeckInstance(string key) {
        List<CombatEntityWithDeckInstance> returnList = new List<CombatEntityWithDeckInstance>();
        if (companionMap.containsValueWithKey(key)) {
            returnList.AddRange(companionMap.getList(key));
        }

        if (minionMap.containsValueWithKey(key)) {
            returnList.AddRange(minionMap.getList(key));
        }

        return returnList;
    }

    public List<TargettableEntity> getTargettableEntities(string key) {
        List<TargettableEntity> entities = new List<TargettableEntity>();
        if (companionMap.containsValueWithKey(key)) {
            entities.AddRange(companionMap.getList(key));
        }

        if (minionMap.containsValueWithKey(key)) {
            entities.AddRange(minionMap.getList(key));
        }

        if (enemyMap.containsValueWithKey(key)) {
            entities.AddRange(enemyMap.getList(key));
        }

        if (playableCardMap.containsValueWithKey(key)) {
            entities.AddRange(playableCardMap.getList(key));
        }

        return entities;
    }

    public void addEntityToDocument(string key, TargettableEntity entity) {
        if (entity is CompanionInstance) {
            CompanionInstance companion = entity as CompanionInstance;
            companionMap.addItem(key, companion);
        } else if (entity is MinionInstance) {
            MinionInstance minion = entity as MinionInstance;
            minionMap.addItem(key, minion);
        } else if (entity is EnemyInstance) {
            EnemyInstance enemy = entity as EnemyInstance;
            enemyMap.addItem(key, enemy);
        } else if (entity is PlayableCard) {
            PlayableCard playableCard = entity as PlayableCard;
            playableCardMap.addItem(key, playableCard);
        }
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
}