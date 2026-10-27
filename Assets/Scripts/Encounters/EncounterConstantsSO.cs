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

    [Header("Status VFX")]
    [Tooltip("Block soaks a hit and some block is left.")]
    public GameObject blockAbsorbVFX;
    [Tooltip("A hit takes the last of the block.")]
    public GameObject blockBreakVFX;
    [Tooltip("Leftover block is cleared at the end of the turn.")]
    public GameObject blockExpireVFX;
    [Tooltip("Temporary strength wears off at the end of the turn.")]
    public GameObject temporaryStrengthExpireVFX;
    [Tooltip("An enemy with money on death dies and pays out.")]
    public GameObject moneyOnDeathVFX;

    [Header("Hit Weight (damage dealt after block)")]
    public int heavyHitDamage = 8;
    public int massiveHitDamage = 15;
    [Tooltip("Played on top of the card's own hit VFX.")]
    public GameObject heavyHitVFX;
    public GameObject massiveHitVFX;
    public float heavyHitShake = 0.15f;
    public float massiveHitShake = 0.3f;
    public bool useHitStop = true;
    [Tooltip("Real seconds the game nearly freezes on impact.")]
    public float heavyHitStop = 0.05f;
    public float massiveHitStop = 0.09f;

    [Header("Last Kill")]
    public bool useLastKillSlowMo = true;
    [Tooltip("Game speed during the slow-mo, relative to the player's speed setting.")]
    public float lastKillSlowMoScale = 0.3f;
    [Tooltip("Real seconds of slow-mo, eased back to full speed over the last third.")]
    public float lastKillSlowMoTime = 0.6f;

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