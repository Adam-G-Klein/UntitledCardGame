using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUI : MonoBehaviour
{
    public GameObject iconPrefab;
    public MapReference mapReference;
    public EncounterVariableSO activeEncounterVariable;
    public GameObject iconGroup;
    public LineRenderer lineRenderer;
    
    private List<Transform> iconPositions = new List<Transform>();
    
    // Start is called before the first frame update
    void Start()
    {
        bool isEncounterOutOfRange = false;
        foreach (EncounterReference encounterReference in mapReference.Value.encounters) {
            Encounter encounter = encounterReference.Value;
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

    public void encounterInitiateEventHandler(string encounterId) {
        mapReference.Value.loadEncounterById(encounterId, activeEncounterVariable);
    }
}
