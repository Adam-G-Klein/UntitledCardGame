using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

// TODO: have a discussion about Entity, I think it 
// may still be useful for UI stuff like this health bar
public class EntityHealthBar : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI text;
    [SerializeField] private CombatInstance combatInstance = null;

    // Update is called once per frame
    void Update()
    {
        if (combatInstance != null) {
            updateSlider();
            updateText();
        }
    }

    public void Setup(CombatInstance instance) {
        this.combatInstance = instance;
    }

    void updateSlider() {
        CombatStats combatStats = combatInstance.combatStats;
        float healthPercent = (float)combatStats.currentHealth /
            (float) combatStats.maxHealth;
        slider.value = healthPercent;
    }

    void updateText() {
        CombatStats combatStats = combatInstance.combatStats;
        text.text = combatStats.currentHealth.ToString() + "/" + combatStats.maxHealth.ToString();
    }
}
