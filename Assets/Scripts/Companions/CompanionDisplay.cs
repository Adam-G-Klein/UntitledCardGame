using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CompanionInstance))]
public class CompanionDisplay : MonoBehaviour
{
    // Component will handle all of the code for animating the companions
    // when we're ready for that

    private CompanionInstance companionInstance;
    public Image companionDisplayImage;

    void Start() {
        companionInstance = GetComponent<CompanionInstance>();
        companionDisplayImage.sprite = companionInstance.companion.companionType.sprite;

    }
    


}
