using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionViewUI : MonoBehaviour
{
    public GameObject positionsParent;
    public GameObject companionInViewPrefab;
    public CompanionListVariableSO companionListVariable;
    public VoidGameEvent companionViewExitedEvent;

    public void Start() {
        Debug.Log(transform);
        setupCompanionDisplay(companionListVariable);
    }

    public void setupCompanionDisplay(CompanionListVariableSO companionList) {
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
                        companionInViewPrefab,
                        positionsOnCanvas[i],
                        Quaternion.identity,
                        transform);
            CompanionInView companionInView = 
                companionImage.GetComponent<CompanionInView>();
            companionInView.companion = companionList.companionList[i];
            companionInView.setup();
        }
    }

    public void exitView() {
        companionViewExitedEvent.Raise(null);
        Destroy(this.gameObject);
    }
}
