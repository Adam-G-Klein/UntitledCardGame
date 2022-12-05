using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyType",
    menuName = "Enemies/Enemy Type")]
public class EnemyTypeSO : ScriptableObject
{
    public int maxHealth;
    public int baseAttackDamage;
    [Space]
    public Sprite sprite;

    public EnemyIntent getNewIntent(List<string> possibleTargets, CombatEntityInEncounterStats selfStats) {
        return new EnemyIntent(
            possibleTargets[Random.Range(0, possibleTargets.Count)],
            selfStats.currentAttackDamage,
            new Dictionary<StatusEffect, int>(){
                {StatusEffect.Weakness, 1}
            });

    }
}
