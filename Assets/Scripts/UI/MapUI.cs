using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MapUI : MonoBehaviour
{
    public GameObject iconPrefab;
    public MapVariableSO activeMapVariable;
    public EncounterVariableSO activeEncounterVariable;
    public CompanionListVariableSO activeCompanions;
    public GameObject iconGroup;
    public LineRenderer lineRenderer;
    public GameObject companionViewUIPrefab;
    public TMP_Text playerGoldTMPText;
    
    private List<Transform> iconPositions = new List<Transform>();
    
    // Start is called before the first frame update
    void Start()
    {
        bool isEncounterOutOfRange = false;
        foreach (Encounter encounter in activeMapVariable.GetValue().encounters) {
            // Encounter encounter = encounterVariable.GetValue();
            IconState iconState;
            if (encounter.isCompleted) {
                iconState = IconState.COMPLETED;
            } else if (isEncounterOutOfRange) {
                iconState = IconState.OUT_OF_RANGE;
            } else {
                iconState = IconState.UPCOMING;
                isEncounterOutOfRange = true;
            }
            GameObject newIcon = GameObject.Instantiate(
                iconPrefab, 
                Vector3.zero,
                Quaternion.identity,
                iconGroup.transform);
            iconPositions.Add(newIcon.transform);
            MapIcon mapIcon = newIcon.GetComponent<MapIcon>();
            mapIcon.Setup(encounter, iconState);
        }
    }

    void Update() {
        Vector3[] points = new Vector3[iconPositions.Count];
        int i = 0;
        lineRenderer.positionCount = iconPositions.Count;
        foreach (Transform trans in iconPositions) {
            points[i] = trans.position;
            i++;
        }
        lineRenderer.SetPositions(points);
    }

    public void showCompanionView() {
        GameObject companionViewUI = GameObject.Instantiate(
                        companionViewUIPrefab,
                        new Vector3(Screen.width / 2, Screen.height / 2, 0),
                        Quaternion.identity);
        companionViewUI
            .GetComponent<CompanionViewUI>()
            .setupCompanionDisplay(activeCompanions, new List<CompanionActionType>() {
                CompanionActionType.VIEW_DECK,
                CompanionActionType.MOVE_COMPANION
            });
    }

    public void setGoldUIHandler(int gold) {
        playerGoldTMPText.text = gold.ToString();
    }
}
