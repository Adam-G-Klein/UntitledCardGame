using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public enum StatusEffectType {
    Strength,
    Weakness,
    // How we'll implement block for now
    Defended,
    DamageMultiply,
    Invulnerability,
    MaxHpBounty,
    TemporaryStrength,
    // Disabled, see comment in processOnDeathStatusEffects in CombatEntityInstance
    MinionsOnDeath,
    PlatedArmor,
    Orb,
    Thorns,
    MoneyOnDeath
}

public class StatusEffectsDisplayViewModel {
    public Dictionary<StatusEffectType, int> statusEffects;
    public StatusEffectsDisplayViewModel(Dictionary<StatusEffectType, int> statusEffectValues) {
        this.statusEffects = statusEffectValues;
    }
}

public class StatusEffectsDisplay: MonoBehaviour
{
    private CombatInstance combatInstance;
    private VisualElement statusEffectsParent;

    [SerializeField]
    private StatusEffectsSO statusEffectsSO;

    private List<VisualElement> drawnTabs = new List<VisualElement>();


    public void Setup(CombatInstance combatInstance, WorldPositionVisualElement wpve)  {
        Debug.Log("Setting up status effect displays for " + combatInstance.name);
        this.combatInstance = combatInstance;

        statusEffectsParent = wpve.rootElement.Q<VisualElement>(
            className: wpve.portraitContainerName + CombatEncounterView.STATUS_EFFECTS_CONTAINER_SUFFIX
        );

    }

    // WILL NOT set the uistate to dirty
    private void AddStatusEffectToDisplay(UnityEngine.Sprite image, int statusValue) {
        VisualElement statusEffectContainer = new VisualElement();
        statusEffectContainer.AddToClassList(CombatEncounterView.STATUS_EFFECTS_TAB_CLASSNAME);
        
        Label statusText = new Label();
        statusText.AddToClassList(CombatEncounterView.STATUS_EFFECTS_TEXT_CLASSNAME);
        statusText.text = statusValue.ToString();
        statusEffectContainer.Add(statusText);

        VisualElement statusImage = new VisualElement();
        statusImage.AddToClassList(CombatEncounterView.STATUS_EFFECTS_IMAGE_CLASSNAME);
        statusImage.style.backgroundImage = new StyleBackground(image);
        statusEffectContainer.Add(statusImage);

        statusEffectsParent.Add(statusEffectContainer);
        drawnTabs.Add(statusEffectContainer);
    }

    public void UpdateStatusDisplays(StatusEffectsDisplayViewModel viewModel) {
        Debug.Log("Updating status displays for " + combatInstance.name);
        Dictionary<StatusEffectType, int> statusEffectsToDisplay = GetStatusesToDisplay(viewModel);
        foreach(VisualElement element in drawnTabs) {
            statusEffectsParent.Remove(element);
        }
        drawnTabs.Clear();
        foreach(KeyValuePair<StatusEffectType, int> kv in statusEffectsToDisplay) {
            Sprite img = statusEffectsSO.GetStatusEffectImage(kv.Key);
            AddStatusEffectToDisplay(img, kv.Value);
        }
        UIStateManager.Instance.SetUIDocDirty();
    }

    private Dictionary<StatusEffectType, int> GetStatusesToDisplay(StatusEffectsDisplayViewModel viewModel) {
        Dictionary<StatusEffectType, int> statusEffectsToDisplay = new Dictionary<StatusEffectType, int>();
        foreach (KeyValuePair<StatusEffectType, int> kv in viewModel.statusEffects) {
            int statusValue = kv.Value;
            if (kv.Key == StatusEffectType.Strength) {
                statusValue += combatInstance.combatStats.baseAttackDamage;
            }
            if(statusValue != CombatInstance.initialStatusEffects[kv.Key]) {
                statusEffectsToDisplay.Add(kv.Key, statusValue);
            }
        }
        return statusEffectsToDisplay;
    }

}
