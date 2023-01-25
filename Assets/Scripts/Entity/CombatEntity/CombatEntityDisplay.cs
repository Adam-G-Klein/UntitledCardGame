using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class CombatEntityDisplay : MonoBehaviour
{
    // Component will handle all of the code for animating the companions
    // when we're ready for that

    private CombatEntityInstance instance;
    public Image displayImage;

    protected void Start() {
        instance = GetComponent<CombatEntityInstance>();
        Debug.Log("Displaying " + instance.baseStats.getId());
        displayImage.sprite = instance.baseStats.getSprite();
    }
    


}
