using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class StatusEffect {
    public StatusEffectType type;
    public Sprite image;
    public TooltipViewModel tooltip;

    public StatusEffect() {}

    public static bool operator== (StatusEffect a, StatusEffect b) {
        return a.type == b.type;
    }

    public static bool operator!= (StatusEffect a, StatusEffect b) {
        return a.type != b.type;
    }
}

[CreateAssetMenu(fileName = "StatusEffectsSO", menuName = "ScriptableObjects/StatusEffectsSO", order = 0)]
public class StatusEffectsSO : ScriptableObject {

    [SerializeField]
    public List<StatusEffect> statusEffects;

    public Sprite GetStatusEffectImage(StatusEffectType type) {
        foreach (StatusEffect statusEffect in statusEffects) {
            if (statusEffect.type == type) {
                return statusEffect.image;
            }
        }
        return null;
    }
    
}
