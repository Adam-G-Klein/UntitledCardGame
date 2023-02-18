using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName ="EncounterConstants",
    menuName = "Constants/Encounter Constants")]
public class EncounterConstants : ScriptableObject {
    [Header("Enemy Encounter")]
    public GameObject companionPrefab;
    public GameObject enemyPrefab;
    public GameObject minionPrefab;
    public GameObject playerPrefab;
    public GameObject cardPrefab;
    [Header("Shop Encounter")]
    public GameObject cardInShopPrefab;
    public GameObject cardSoldOutPrefab;
    public GameObject randomBackgroundPrefab;
}