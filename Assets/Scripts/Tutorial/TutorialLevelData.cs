using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class TutorialLevelData : MonoBehaviour
{
    [SerializeField]
    private List<TutorialData> data;

    public TutorialData Get(int tutorialID) {
        foreach(TutorialData tutorialData in data) {
            if (tutorialData.ID == tutorialID) {
                return tutorialData;
            }
        }

        Debug.LogError("Unable to find specified tutorial: " + tutorialID.ToString());
        return default;
    }
}
