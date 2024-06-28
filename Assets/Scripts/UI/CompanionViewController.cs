using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class CompanionViewController : MonoBehaviour
{

    [SerializeField]
    private GameStateVariableSO gameState;

    [SerializeField]
    private GameObject companionViewUIPrefab;
    public void showCompanionView() {
        GameObject companionViewUI = GameObject.Instantiate(
                        companionViewUIPrefab,
                        new Vector3(Screen.width / 2, Screen.height / 2, 0),
                        Quaternion.identity);
        companionViewUI
            .GetComponent<CompanionViewUI>()
            .setupCompanionDisplay(gameState.companions, new List<CompanionActionType>() {
                CompanionActionType.VIEW_DECK,
                CompanionActionType.MOVE_COMPANION,
                CompanionActionType.COMBINE_COMPANION
            });
    }
}
