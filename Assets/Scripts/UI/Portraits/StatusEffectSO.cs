using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class StatusEffect {
    public StatusEffectType type;
    public Sprite image;
}

[CreateAssetMenu(fileName = "StatusEffectsSO", menuName = "StatusEffectsSO", order = 0)]
public class StatusEffectsSO : ScriptableObject {
    
}
