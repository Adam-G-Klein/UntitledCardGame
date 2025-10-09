using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MeothraController : MonoBehaviour, IBossController
{

    private MeothraIntentDisplay meothraIntentDisplay;
    private MeothraIntroAnimationDisplay meothraIntroAnimation;
    public void Setup()
    {
        meothraIntentDisplay = GetComponent<MeothraIntentDisplay>();
        meothraIntentDisplay.Setup();
        meothraIntroAnimation = GetComponentInChildren<MeothraIntroAnimationDisplay>();
        meothraIntroAnimation.Setup();
    }
}
