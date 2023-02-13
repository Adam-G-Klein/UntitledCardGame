using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCompanionViewUI : MonoBehaviour
{
    public GameObject companionViewUIPrefab;
    public CompanionListVariableSO companionList;

    private GameObject companionViewUI;

    // Start is called before the first frame update
    void Start()
    {
        this.companionViewUI = GameObject.Instantiate(
                        companionViewUIPrefab,
                        new Vector3(Screen.width / 2, Screen.height / 2, 0),
                        Quaternion.identity);
        this.companionViewUI
            .GetComponent<CompanionViewUI>()
            .setupCompanionDisplay(companionList, new List<CompanionActionType>() {
                CompanionActionType.SELECT,
                CompanionActionType.VIEW_DECK,
                CompanionActionType.MOVE_COMPANION
            });
    }
}
