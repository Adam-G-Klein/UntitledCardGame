using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEventRaise : MonoBehaviour
{
    public TutorialEvent tutorialEvent;

    public void Raise() { // just raises the event
        Debug.Log("testing pls be here");
        tutorialEvent.Raise(null);
    }
}
