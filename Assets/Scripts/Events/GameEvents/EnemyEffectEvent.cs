using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyEffectEventInfo {
    public int damage;
    public List<string> targets;
    public Dictionary<StatusEffect, int> statusEffects;

    public EnemyEffectEventInfo(int damage, List<string> targets, Dictionary<StatusEffect, int> statusEffects) {
        this.damage = damage;
        this.targets = targets;
        this.statusEffects = statusEffects;
    }

}
[CreateAssetMenu(
    fileName = "EnemyEffectEvent", 
    menuName = "Events/Game Event/Enemy Effect Event")]
public class EnemyEffectEvent : BaseGameEvent<EnemyEffectEventInfo> { }
