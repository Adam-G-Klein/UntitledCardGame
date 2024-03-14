using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum IconState {
    COMPLETED,
    UPCOMING,
    OUT_OF_RANGE
}

public class MapIcon : MonoBehaviour
    , IPointerClickHandler
{
    public Sprite enemySprite;
    public Sprite shopSprite;
    public Color defaultColor;
    public Color outOfRangeColor;
    public GameObject completedFootsteps;
    public Image iconImage;
    public StringGameEvent encounterInitiateEvent;
    public IconState iconState;

    private string encounterId;

    public void Setup(Encounter encounter, IconState iconState) {
        setEncounterId(encounter.id);
        this.iconState = iconState;

        // Setup the sprite for the encounter type
        switch (encounter.getEncounterType()) {
            case EncounterType.Enemy:
                this.iconImage.sprite = enemySprite;
            break;

            case EncounterType.Shop:
                this.iconImage.sprite = shopSprite;
            break;
        }

        // Setup the icon given the state of the corresponding encounter
        this.completedFootsteps.SetActive(false);
        switch (this.iconState) {
            case IconState.COMPLETED:
                this.completedFootsteps.SetActive(true);
            break;

            case IconState.UPCOMING:
                // Currently nothing to actually do here
            break;

            case IconState.OUT_OF_RANGE:
                this.iconImage.color = outOfRangeColor;
            break;
        }
    }

    public void setEncounterId(string encounterId) {
        this.encounterId = encounterId;
    }

    public void OnPointerClick(PointerEventData eventData) {
        raiseEncounter(encounterId);
    }
    public void raiseEncounter(string encounterId) {
        if (this.iconState == IconState.UPCOMING) {
            encounterInitiateEvent.Raise(encounterId);
        }
    }

}
