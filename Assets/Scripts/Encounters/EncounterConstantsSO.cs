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

    [Header("Deck VFX")]
    [Tooltip("Trail drawn behind each little card that flies from the discard pile to the draw pile on a reshuffle " +
             "(UIElementTrail, tinted the pack's colour). Leave empty for none.")]
    public GameObject reshuffleTrailVFX;

    [Header("Hit Timing")]
    [Tooltip("The hit sound, damage number, shake and health bar wait until the card's effect visibly strikes " +
             "(see HitTiming). Gameplay is unchanged either way. Off: they play when the damage is applied.")]
    public bool syncHitFeedbackToVfx = true;

    [Header("Companion Attack Lunge")]
    [Tooltip("When a companion's attack card damages enemies, the companion lunges toward its target and the hit " +
             "lands as it arrives (CompanionAttackLunge).")]
    public bool useCompanionAttackLunge = true;
    [Tooltip("How far the companion travels toward the target, as a fraction of the distance between them.")]
    [Range(0f, 1f)]
    public float companionLungeDistance = 0.2f;
    [Tooltip("Seconds to reach the target. The hit lands at the end of this.")]
    public float companionLungeOutTime = 0.15f;
    [Tooltip("Seconds to return to its spot after the hit.")]
    public float companionLungeReturnTime = 0.3f;

    [Header("Last Kill")]
    public bool useLastKillSlowMo = true;
    [Tooltip("Game speed during the slow-mo, relative to the player's speed setting.")]
    public float lastKillSlowMoScale = 0.3f;
    [Tooltip("Real seconds of slow-mo, eased back to full speed over the last third.")]
    public float lastKillSlowMoTime = 0.6f;
    [Tooltip("On the last death of a fight (ally or enemy) the screen eases to stark grey and white and back " +
             "while an ink splash bursts on the one who fell (ImpactFrame).")]
    public bool useLastDeathImpactFrame = true;
    [Tooltip("Real seconds the impact frame takes, easing in and out. Keep it long enough that it doesn't read as a flash.")]
    public float impactFrameTime = 0.6f;
    public GameObject impactFrameSplashVFX;

    [Header("Screen Borders")]
    [Tooltip("The dark watercolour ink around the screen in boss fights (elite encounters, not Meothra's) (BossInkBorder). " +
             "Leave empty for none.")]
    public GameObject bossInkBorderVFX;
    [Tooltip("The red border that wicks in from the screen's edges when the party is low on health, in every fight " +
             "(LowHealthBorder). Leave empty for none.")]
    public GameObject lowHealthBorderVFX;

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