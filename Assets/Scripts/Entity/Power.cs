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

    // By default, powers will stack.
    [SerializeField]
    public bool stackable = true;

    public enum PowerType
    {
        None,
        // Barricade: do not lose leftover Block end of turn
        Barricade,
        // DoubleDamage: all attacks played from this companion deal double damage
        DoubleDamage,
        // EveryAttackBlock: gain block each time you play an attack from this companion
        EveryAttackBlock,
        // Ironheart: generate an "Ironskin" each turn
        Ironheart,
        // Bloodfire: each turn, lose 1 HP and gain ...
        Bloodfire,
    }

    public TooltipViewModel GetTooltip()
    {
        if (tooltip == null)  return new TooltipViewModel();
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
        if (!power.stackable && HasPower(power.powerType))
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
