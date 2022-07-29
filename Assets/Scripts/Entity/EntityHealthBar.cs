using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EntityHealthBar : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI text;
    private Entity entity = null;

    // Update is called once per frame
    void Update()
    {
        checkGetEntity();
        updateSlider();
        updateText();
    }

    void updateSlider() {
        float healthPercent = (float) entity.getHealth() / (float) entity.getMaxHealth();
        slider.value = healthPercent;
    }

    void updateText() {
        text.text = entity.getHealth().ToString() + "/" + entity.getMaxHealth().ToString();
    }

    void checkGetEntity() {
        // Since the entity game objects are instantiated then set up,
        // this needs to go in update instead of start since the entity
        // field of the data store might not be set yet when start is ran
        if (entity == null) {
            EntityInScene entityInScene = GetComponentInParent<EntityInScene>();
            if (entityInScene == null) {
                Debug.LogError("Cannot find entity data in parent game object");
                return;
            }
            entity = entityInScene.getEntity();
        }
    }
}
