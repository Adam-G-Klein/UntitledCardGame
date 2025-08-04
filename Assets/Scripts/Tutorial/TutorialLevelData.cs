using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class TutorialLevelData : MonoBehaviour
{

    //Not a set because it does not work in inspector
    [SerializeField]
    private List<TutorialData> data;

    public TutorialData Get(string tutorialID)
    {
        foreach (TutorialData tutorialData in data)
        {
            //Null check because this can very easily be unset in the inspector
            if (tutorialData != default)
            {
                if (tutorialData.ID == tutorialID)
                {
                    return tutorialData;
                }
            }
            else
            {
                Debug.Log("TUTORIAL ERROR: null tutorial data in level data");
            }
        }

        Debug.Log("TUTORIAL ERROR: Unable to find specified tutorial: " + tutorialID.ToString());
        return default;
    }

    public TutorialData GetFirst()
    {
        if (data != default && data.Count != 0) return data[0];
        return default;
    }
}
