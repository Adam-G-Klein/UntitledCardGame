using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// TODO: have a discussion about Entity, I think it 
// may still be useful for UI stuff like this health bar
public class EntityHealthBar : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI text;
    private CombatStats combatStats = null;
    private CombatInstance combatInstance = null;

    // Update is called once per frame
    void Update()
    {
        checkGetEntity();
        updateSlider();
        updateText();
        Debug.Log("EntityHealthBar update.\ncombatInstanceGO name: " + combatInstance.gameObject.name + "\ncombatStats: " + combatStats + "\ncombatStats.currentHealth: " + combatStats.currentHealth + "\ncombatStats.maxHealth: " + combatStats.maxHealth);
    }

    void updateSlider() {
        float healthPercent = (float)combatStats.currentHealth /
            (float) combatStats.maxHealth;
        slider.value = healthPercent;
    }

    void updateText() {
        text.text = combatStats.currentHealth.ToString() + "/" + combatStats.maxHealth.ToString();
    }

    void checkGetEntity() {
        // Since the entity game objects are instantiated then set up,
        // this needs to go in update instead of start since the entity
        // field of the data store might not be set yet when start is ran
        if (combatStats == null) {
            combatInstance = GetComponentInParent<CombatInstance>();
            combatStats = combatInstance.combatStats;
            if (combatStats == null) {
                Debug.LogError("Cannot find combat stats data in parent game object");
                return;
            }
        }
    }
}
