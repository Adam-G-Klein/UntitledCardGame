using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class EnemyIntentInfo {
    public EnemyIntentType type;
    public Sprite image;

    public EnemyIntentInfo() {}

    public static bool operator== (EnemyIntentInfo a, EnemyIntentInfo b) {
        return a.type == b.type;
    }

    public static bool operator!= (EnemyIntentInfo a, EnemyIntentInfo b) {
        return a.type != b.type;
    }
}

[CreateAssetMenu(fileName = "EnemyIntentsSO", menuName = "ScriptableObjects/EnemyIntentsSO", order = 0)]
public class EnemyIntentsSO : ScriptableObject {

    [SerializeField]
    public List<EnemyIntentInfo> intentTypes;

    public Sprite GetIntentImage(EnemyIntentType type) {
        foreach (EnemyIntentInfo info in intentTypes) {
            if (info.type == type) {
                return info.image;
            }
        }
        return null;
    }
    
}
