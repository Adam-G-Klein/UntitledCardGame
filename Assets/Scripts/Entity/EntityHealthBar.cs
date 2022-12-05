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
    private CombatEntityStatsDisplay entity = null;

    // Update is called once per frame
    void Update()
    {
        checkGetEntity();
        updateSlider();
        updateText();
    }

    void updateSlider() {
        float healthPercent = (float) entity.currentHealth / (float) entity.maxHealth;
        slider.value = healthPercent;
    }

    void updateText() {
        text.text = entity.currentHealth.ToString() + "/" + entity.maxHealth.ToString();
    }

    void checkGetEntity() {
        // Since the entity game objects are instantiated then set up,
        // this needs to go in update instead of start since the entity
        // field of the data store might not be set yet when start is ran
        if (entity == null) {
            entity = GetComponentInParent<CombatEntityStatsDisplay>();
            if (entity == null) {
                Debug.LogError("Cannot find entity data in parent game object");
                return;
            }
        }
    }
}
