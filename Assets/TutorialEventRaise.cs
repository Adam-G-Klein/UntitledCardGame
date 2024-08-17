using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEventRaise : MonoBehaviour
{
    public TutorialEvent tutorialEvent;

    public void TimeToRAISE() {
        tutorialEvent.Raise(null);
    }
}
