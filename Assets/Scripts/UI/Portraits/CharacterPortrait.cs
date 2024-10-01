using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPortrait : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private EntityHealthViewController healthBar;
    [SerializeField] private DeckDisplay deckDisplay;
    [SerializeField] private StatusEffectDisplaysController statusEffectsController;
    public CompanionInstance companionInstance;

    public void Setup(CompanionInstance companionInstance) {
        this.companionInstance = companionInstance;
        this.healthBar.Setup(companionInstance.combatInstance);
        this.deckDisplay.Setup(companionInstance.deckInstance);
        this.statusEffectsController.Setup(companionInstance.combatInstance);
        UpdatePortrait();
    }

    private void UpdatePortrait() {
        this.image.sprite = companionInstance.companion.companionType.portrait;
    }
}
