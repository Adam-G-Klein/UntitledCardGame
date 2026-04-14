using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(
    fileName ="EncounterConstants",
    menuName = "Constants/Encounter Constants")]
public class EncounterConstantsSO : ScriptableObject {
    [Header("Global Config")]
    public int goldEarnedPerBattle;
    public float interestRate;
    public int interestCap;
    public int benchHealingAmount;
    public int postEliteHealingAmount;
    [Header("Combat Encounter")]
    public Vector2 bossSpawnLocation = Vector2.zero;
    [Header("Shop Encounter")]
    public List<ShopLevel> shopLevels;
    [Header("Asset References")]
    public GameObject companionPrefab;
    public GameObject enemyPrefab;
    public GameObject SmokeAndArmsBossPrefab;
    public GameObject cardPrefab;
    public GameObject companionDeathPrefab;
    public GameObject enemyDeathPrefab;
    public GameObject companionRevivePrefab;
    public VisualTreeAsset companionViewTemplate;
    public VisualTreeAsset companionManagementViewTemplate;

    public ShopLevel GetShopLevel(int level)
    {
        foreach (ShopLevel shopLevel in shopLevels)
        {
            if (shopLevel.level == level)
            {
                return shopLevel;
            }
        }
        // Default to whatever is first in the list
        Debug.LogWarning("Specified shop level not found, using default shop level");
        return shopLevels[0];
    }
}