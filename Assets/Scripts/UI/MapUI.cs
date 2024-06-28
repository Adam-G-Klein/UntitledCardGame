using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class MapUI : MonoBehaviour
{
    public GameObject iconPrefab;
    public GameStateVariableSO gameState;
    public GameObject iconGroup;
    public LineRenderer lineRenderer;
    public GameObject companionViewUIPrefab;
    public TMP_Text playerGoldTMPText;
    public ScrollRect mapScrollRect;
    public UIDocument mapDocument;

    private VisualElement root;
    
    private List<Transform> iconPositions = new List<Transform>();
    
    // Start is called before the first frame update
    void Start()
    {
        root = mapDocument.GetComponent<UIDocument>().rootVisualElement; 
        bool isEncounterOutOfRange = false;
        int activeEncounterIndex = -2;
        int currentIndex = 0;
        foreach (Encounter encounter in gameState.map.GetValue().encounters) {
            IconState iconState;
            var button = root.Q<UnityEngine.UIElements.Button>(currentIndex.ToString());
            Color c = Color.white;
            c.a = 0.3f;
            button.style.unityBackgroundImageTintColor = c;
            if (encounter.id == gameState.activeEncounter.GetValue().id) {
                iconState = IconState.UPCOMING;
                isEncounterOutOfRange = true;
                button.style.unityBackgroundImageTintColor = Color.white;
            } else if (isEncounterOutOfRange) {
                iconState = IconState.OUT_OF_RANGE;
                button.style.unityBackgroundImageTintColor = c;
            } else {
                //button.style.unityBackgroundImageTintColor = Color.green;
                var done = button.Q<VisualElement>("done");
                if (done != null)
                {
                   done.style.unityBackgroundImageTintColor = Color.black;
                }
                iconState = IconState.COMPLETED;
                activeEncounterIndex++;
                
            }
            GameObject newIcon = GameObject.Instantiate(
                iconPrefab, 
                Vector3.zero,
                Quaternion.identity,
                iconGroup.transform);
            iconPositions.Add(newIcon.transform);
            MapIcon mapIcon = newIcon.GetComponent<MapIcon>();
            mapIcon.Setup(encounter, iconState);
            button.clicked += () => encounterClicked(mapIcon, encounter);
            currentIndex++;

        }
        float scrollRectPosition = (float) activeEncounterIndex / (float) gameState.map.GetValue().encounters.Count;
        mapScrollRect.horizontalNormalizedPosition = Mathf.Max(scrollRectPosition, 0f);
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

    public void encounterClicked(MapIcon icon, Encounter encounter) {
        print("here");
        icon.raiseEncounter(encounter.id);
    }

    public void setGoldUIHandler(int gold) {
        playerGoldTMPText.text = gold.ToString();
    }
}
