using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI text;
    private Enemy enemy = null;

    // Update is called once per frame
    void Update()
    {
        checkGetEnemy();
        updateSlider();
        updateText();
    }

    void updateSlider() {
        float healthPercent = (float) enemy.getHealth() / (float) enemy.getMaxHealth();
        slider.value = healthPercent;
    }

    void updateText() {
        text.text = enemy.getHealth().ToString() + "/" + enemy.getMaxHealth().ToString();
    }

    void checkGetEnemy() {
        // Since the enemy game objects are instantiated then set up,
        // this needs to go in update instead of start since the enemy
        // field of the data store might not be set yet when start is ran
        if (enemy == null) {
            enemy = GetComponentInParent<EnemyData>().getEnemy();
        }
    }
}
