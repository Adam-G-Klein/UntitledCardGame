using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyType",
    menuName = "Enemies/Enemy Type")]
public class EnemyType : ScriptableObject
{
    public int maxHealth;
    [Space]
    public Sprite sprite;
}