using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

[CreateAssetMenu(
    fileName = "NewPowerSO",
    menuName = "ScriptableObjects/powerSO")]
[System.Serializable]
public class PowerSO : ScriptableObject
{
    [SerializeField]
    public PowerType powerType = PowerType.None;

    [SerializeField]
    [Header("Title and description populate the card that activates this passive.")]
    public string title;

    [SerializeField]
    public string description;

    // The abilities are optional: once we activate the power, we will create an EntityAbilityInstance.
    // That allows us to do simple things in response to common game events.
    // If there is something special, we can do a switch on whether the companion has a particular power active
    // in the code without using the abilities.
    [SerializeReference]
    public List<EntityAbility> abilities;

    [SerializeField]
    public Sprite displaySprite;

    [SerializeReference]
    private TooltipViewModel tooltip = new();

    // By default, powers will NOT stack.
    // [SerializeField]
    // private bool stackable = false;

    // get accessor for stackable:
    // For now, let's ignore the property and make all powers non-stackable.
    public bool Stackable { get { return false; } }

    // Use this to display cached values for the power (e.g. number of triggers left).
    public CacheConfiguration cacheConfiguration = new CacheConfiguration();


    public enum PowerType
    {
        None,
        // Barricade: do not lose leftover Block end of turn
        Barricade,
        // DoubleDamage: all attacks played from this companion deal double damage
        DoubleDamage,
        // EveryAttackBlock: gain block each time you play an attack
        EveryAttackBlock,
        // Ironheart: generate an "Ironskin" each turn
        Ironheart,
        // Bloodfire: each turn, lose 1 HP and gain ...
        Bloodfire,
        // Witchwork: end of turn, give all Status cards in hand +2 damage
        Witchwork,
        // PerpetualMotion: whenever the hand is empty, draw cards
        PerpetualMotion,
        // FollowUp: If the last card played this turn was an attack, attacks from this companion deal +3 damage
        FollowUp,
        // SoulOfOak: Gain X block for each status card in hand.
        SoulOfOak,
        // RainOfBlows: Each attack from this companion deals +1 damage for each attack you played this turn.
        RainOfBlows,
        // Lifesink: end of turn, bleed all 1 HP and gain some defense
        Lifesink,
        // DustUntoDust: end of turn, exhaust all cards in hand and give self 4 block
        DustUntoDust,
        DemonRatForm,
        // Retaliation: whenever self loses HP, deal X to a random enemy
        Retaliation,
        // EternalFlame: shuffle greek fire into own deck.
        EternalFlame,
        // Maelstrom: whenever you play a card, do X
        Maelstrom,
        // VortexOfSilence: end of turn, if your hand is empty, deal a shit ton of damage
        VortexOfSilence,
        // Patience: end of turn, retain 1 card.
        Patience,
        // Whenever you discard a card, deal dmg equal to the number of cards discarded this turn to a random enemy
        RadiantCharisma,
        // HotStreak: attacks deal +2 damage for each card you discarded this turn.
        HotStreak,
        // KeepRolling: when you discard a card from this companion, draw a card at random
        KeepRolling,
        // ForgeGodsWrath: less than 7 cards, add a cool card to deck.
        ForgeGodsWrath,
        // EverythingBurns: Whenever you exhaust a Status card, give all Greek Fire cards +1 damage
        EverythingBurns,
        // PainTrain: deck shuffled damage
        PainTrain,
        // HeatVents: each turn, draw an extra card then exhaust 2 cards in hand.
        HeatVents,
        // CrushingWeight: dmg based on deck size
        CrushingWeight,
        // EbbAndFlow: dmg / block for exhausting status cards
        EbbAndFlow,
        // ForestProphet: create Tiefvision cards
        ForestProphet,
        // TotalMeltdown: add Firebreath cards to each deck
        TotalMeltdown,
        // BalancedTemper: self damage until below half HP
        BalancedTemper,
        // _: whenever heals, they gain 3 block
        Rejuvenation,
        // _: Whenever a companion gains block, bleed self 1 HP and draw a card from self
        OldGodsSanctuary,
        // _: Whenever a companion gains block, if self below half HP, heal 2 HP and deal 7 damage to random enemy
        UncheckedVitality,
        // _: End of turn, retain an Attack card and give it +3 damage
        Whetstone,
        // _: Whenever you play an Attack, give all 1 block
        LayeredSteel,
        // _: The first attack you play each turn, gain 1 energy and draw a card
        SeizeTheInitiative,
        // _: End turn, if you exhausted at least 3 cards, deal a ton of damage
        Apocalypse,
    }

    public TooltipViewModel GetTooltip()
    {
        if (tooltip.empty || tooltip.lines.Count == 0)
        {
            TooltipLine x = new TooltipLine(title, description);
            return new TooltipViewModel(new List<TooltipLine> {x} );
        }
        return tooltip;
    }
}

// PowerPool manages the powers for a single entity.
public class PowerPool
{
    // Invariant: we only have one active power of a given type in the pool.
    List<PowerSO> activePowers = new();

    public bool ActivatePower(PowerSO power)
    {
        // If it's not stackable, check if the list already contains the power.
        if (!power.Stackable && HasPower(power.powerType))
        {
            return false;
        }
        activePowers.Add(power);
        return true;
    }

    public bool HasPower(PowerSO.PowerType powerType)
    {
        return activePowers.Any(p => p.powerType == powerType);
    }

    public List<PowerSO> GetPowers()
    {
        return activePowers;
    }
}
