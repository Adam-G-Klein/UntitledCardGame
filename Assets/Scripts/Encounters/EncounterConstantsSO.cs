using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(
    fileName ="EncounterConstants",
    menuName = "Constants/Encounter Constants")]
public class EncounterConstantsSO : ScriptableObject {
    [Header("Enemy Encounter")]
    public GameObject companionPrefab;
    public GameObject enemyPrefab;
    public GameObject minionPrefab;
    public GameObject playerPrefab;
    public GameObject cardPrefab;
    public GameObject companionDeathPrefab;
    public GameObject enemyDeathPrefab;
    public GameObject companionRevivePrefab;
    [Header("Shop Encounter")]
    public GameObject cardInShopPrefab;
    public GameObject cardSoldOutPrefab;
    public GameObject keepsakeInShopPrefab;
    public GameObject randomBackgroundPrefab;
    [Header("Misc")]
    public VisualTreeAsset companionViewTemplate;
    public VisualTreeAsset companionViewTemplateOld;
    public VisualTreeAsset companionManagementViewTemplate;
    public CardType cardGeneratedByOrb;
    public GameObject cardDrawVFXPrefab;
    public Sprite genericCardCompanionTypeSprite;
}