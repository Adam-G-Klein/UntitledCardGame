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
        print(mapDocument);
        root = mapDocument.GetComponent<UIDocument>().rootVisualElement; 
        bool isEncounterOutOfRange = false;
        int activeEncounterIndex = -2;
        int currentIndex = 0;
        Debug.Log("here" + gameState.map.GetValue().encounters.Count);
        foreach (Encounter encounter in gameState.map.GetValue().encounters) {
            IconState iconState;
            var button = root.Q<UnityEngine.UIElements.Button>(currentIndex.ToString());
            Debug.Log("asf:" + button);
            button.style.unityBackgroundImageTintColor = UnityEngine.Color.white.WithAlpha(0.3f);
            if (encounter.id == gameState.activeEncounter.GetValue().id) {
                iconState = IconState.UPCOMING;
                isEncounterOutOfRange = true;
                button.style.unityBackgroundImageTintColor = UnityEngine.Color.white.WithAlpha(1f);
            } else if (isEncounterOutOfRange) {
                iconState = IconState.OUT_OF_RANGE;
                button.style.unityBackgroundImageTintColor = UnityEngine.Color.white.WithAlpha(0.3f);
            } else {
                button.style.unityBackgroundImageTintColor = UnityEngine.Color.green.WithAlpha(1f);
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

    public void encounterClicked(MapIcon icon, Encounter encounter) {
        print("here");
        icon.raiseEncounter(encounter.id);
    }

    public void setGoldUIHandler(int gold) {
        playerGoldTMPText.text = gold.ToString();
    }
}
