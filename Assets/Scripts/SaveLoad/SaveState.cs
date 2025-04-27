using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
The top level Serializable object that will contain all the data we want to save.


*/
[System.Serializable]
public class SaveState 
{
    public string testText;
    public Encounter activeEncounter;

    public SaveState(string testText, Encounter activeEncounter) {
        this.testText = testText;
        this.activeEncounter = activeEncounter;
    }

}
