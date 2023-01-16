using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName ="EncounterConstants",
    menuName = "Constants/Encounter Constants")]
public class EncounterConstants : ScriptableObject {
    [Header("Enemy Encounter")]
    public GameObject companionPrefab;
    public GameObject enemyPrefab;
    public GameObject playerPrefab;
    [Header("Shop Encounter")]
    public GameObject cardInShopPrefab;
}