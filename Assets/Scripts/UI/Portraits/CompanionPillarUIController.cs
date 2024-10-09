using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(StatusEffectsDisplay))]
public class CompanionPillarUIController : MonoBehaviour
{
    [SerializeField] private StatusEffectsDisplay statusEffectsDisplay;
    public CompanionInstance companionInstance;

    public void Setup(CompanionInstance companionInstance, WorldPositionVisualElement wpve) {
        Debug.Log("Setting up UI for " + companionInstance.name);
        this.companionInstance = companionInstance;
        this.statusEffectsDisplay.Setup(companionInstance.combatInstance, wpve);
    }

}
