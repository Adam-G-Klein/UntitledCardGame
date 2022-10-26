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
}
