using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionViewUI : MonoBehaviour
{
    public GameObject positionsParent;
    public GameObject uICompanionPrefab;
    public VoidGameEvent companionViewExitedEvent;

    public void setupCompanionDisplay(CompanionListVariableSO companionList,
            List<CompanionActionType> actionTypes) {
        List<Vector3> positionsOnCanvas = new List<Vector3>();
        foreach (Transform child in positionsParent.transform) {
            positionsOnCanvas.Add(child.gameObject.GetComponent<RectTransform>().position);
        }
        if (companionList.companionList.Count > positionsOnCanvas.Count) {
            Debug.LogError("Too many companions to display in companion view UI");
            return;
        }

        for (int i = 0; i < companionList.companionList.Count; i++) {
            GameObject companionImage = GameObject.Instantiate(
                        uICompanionPrefab,
                        positionsOnCanvas[i],
                        Quaternion.identity,
                        transform);
            UICompanion uICompanion = 
                companionImage.GetComponent<UICompanion>();
            uICompanion.companion = companionList.companionList[i];
            uICompanion.Setup(actionTypes);
        }
    }

    public void exitView() {
        companionViewExitedEvent.Raise(null);
        Destroy(this.gameObject);
    }
}
