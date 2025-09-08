using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class WorldPositionVisualElement {
    public Vector3 worldPos;
    public VisualElement ve;

    public string portraitContainerName { 
        get {
            return ve.name;
        }
    }

    public VisualElement rootElement;

    public WorldPositionVisualElement(VisualElement ve, Vector3 worldPos) {
        this.ve = ve;
        this.worldPos = worldPos;
        rootElement = UIDocumentUtils.GetRootElement(ve);
    }
    public WorldPositionVisualElement() {
        this.worldPos = Vector3.zero;
    }
    public void UpdatePosition() {
        worldPos = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(this.ve);
    }
}

public class PlacementPool {
    public string prefix;
    private VisualElement root;
    Dictionary<WorldPositionVisualElement, GameObject> placements;
    // Used by encounterBuilder to get the position before instantiation
    // so the gameobject isn't available yet
    private WorldPositionVisualElement placementOnDeck;
    private Dictionary<GameObject, WorldPositionVisualElement> gameObjectToPosition;
    public PlacementPool(string prefix, VisualElement root) {
        this.prefix = prefix;
        this.root = root;
        placements = new Dictionary<WorldPositionVisualElement, GameObject>();
        gameObjectToPosition = new Dictionary<GameObject, WorldPositionVisualElement>();
    }

    // Called by the gameobject placer once it detects the UIDocument is ready for us
    // there's at least one frame of delay because of the render texture
    public void InitializeMap(){
        int index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        VisualElement ve = root.Q<VisualElement>(prefix + index);
        while(ve != null) {
            Vector3 worldPos = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(ve);
            placements.Add(new WorldPositionVisualElement(ve, worldPos), null);
            index += 1;
            ve = root.Q<VisualElement>(prefix + index);
        }
        Debug.Log("UIDocGameObjectPlacer: " + prefix + " has " + (index - 1) + "placements");
    }

    // I'd rather eat a raw can full of tomato sauce than make this better than O(N)
    /// <summary>
    /// adds a mapping between a visualElement and a gameObject
    /// WorldPositionVisualElement must first have been checked out
    /// This is so that encounterbuilders can get positions for objects before having instantiated them
    /// </summary>
    /// <param name="ve"></param>
    /// <param name="go"></param>
    public void addMapping(WorldPositionVisualElement ve, GameObject go){
        if(ve == null) {
            Debug.LogError("UIDocGameObjectPlacer: WorldPosVE null in addmapping");
        }
        if(ve.ve == null) {
            Debug.LogError("UIDocGameObjectPlacer: WorldPosVE.ve null in addmapping");
        }
        if(ve != placementOnDeck) {
            Debug.LogWarning("UIDocGameObjectPlacer: visual element " + ve.ve.name + " mapped without being requested first");
        }
        placements[ve] = go;
        gameObjectToPosition[go] = ve;
    }

    public WorldPositionVisualElement GetCardWPVEFromGO(GameObject GO) {
        return gameObjectToPosition[GO];
    }

    public void removeMapping(GameObject go) {
        WorldPositionVisualElement placementToNull = null;
        foreach(KeyValuePair<WorldPositionVisualElement, GameObject> kvp in placements) {
            if(kvp.Value == go) {
                // don't modify during iteration
                placementToNull = kvp.Key;
            }
        }
        if (placementToNull != null) {
            placements[placementToNull] = null;
        } else {
            Debug.LogError("UIDocGameObjectPlacer: failed to remove gameobject " + go.name + " from placements, it wasn't in there somehow");
        }
    }

    public int getCount() {
        return placements.Count;
    }

    public WorldPositionVisualElement checkoutPlacement(){
        foreach(KeyValuePair<WorldPositionVisualElement, GameObject> kvp in placements) {
            if(kvp.Value == null) {
                placementOnDeck = kvp.Key;
                return kvp.Key;
            }
        }
        Debug.LogError("UIDocGameObjectPlacer: failed to checkout placement from prefix " + prefix + ", not enough UIDoc placements");
        return new WorldPositionVisualElement(null, Vector3.zero);
    }

}
// lets make this a singleton, it should work as our new location store with the encounterBuilder
public class UIDocumentGameObjectPlacer : GenericSingleton<UIDocumentGameObjectPlacer> {
    [Header("Set from component on this gameObj to save\nthe GetComponent and Start() calls on an already slow scene load time")]
    [SerializeField]
    private UIDocument uiDoc;

    [SerializeField]
    private bool autoPlaceEnemies = true;
    public static string ENEMY_UIDOC_ELEMENT_PREFIX = "enemy-view-parent-container";
    // public static string COMPANION_UIDOC_ELEMENT_PREFIX = "companion";
    public static string COMPANION_UIDOC_ELEMENT_PREFIX = "companion-view-parent-container";
    public static string CARD_UIDOC_ELEMENT_PREFIX = "card";
    public static int INITIAL_INDEX = 1;

    private PlacementPool companionPlacements;
    private PlacementPool enemyPlacements;
    private VisualElement cardsContainer;
    private PlacementPool cardPlacements;
    private List<PlacementPool> placementPools; 
    private bool mapsInitialized = false;

    public static float zPlane = 0;
    void Awake() {
        mapsInitialized = false;
        VisualElement root = uiDoc.rootVisualElement;
        cardsContainer = root.Q<VisualElement>(name:"cardContainer");
        companionPlacements = new PlacementPool(COMPANION_UIDOC_ELEMENT_PREFIX, root);
        enemyPlacements = new PlacementPool(ENEMY_UIDOC_ELEMENT_PREFIX, root);        
        cardPlacements = new PlacementPool(CARD_UIDOC_ELEMENT_PREFIX, root);         
        placementPools = new List<PlacementPool>(){
            companionPlacements,
            enemyPlacements,
            cardPlacements
        };
        StartCoroutine(InitializeMapsWhenReady());
    }

    void Update() {

        // Set the gameobject to the worldspace position of the element in the ui document (which is in screenspace)
        //PlaceMappings();
    }

    public bool IsReady(){
        return mapsInitialized;
    }

    private IEnumerator InitializeMapsWhenReady() {
        yield return new WaitUntil(() => readyToInitializeMaps());
        Debug.Log("UIDocumentGameObjectPlacer: Ready to initialize maps");
        foreach(PlacementPool pool in placementPools) {
            pool.InitializeMap();
        }
        mapsInitialized = true;
        Debug.Log("UIDocumentGameObjectPlacer: Maps initialized, ready");
    }

    private bool readyToInitializeMaps() {
        if(uiDoc == null) {
            Debug.LogError("No UIDoc set on UIDocGameObject placer. Just drag a ref to its game object onto the field");
            return false;
        }
        VisualElement element = uiDoc.rootVisualElement;
        if(element == null) {
            Debug.Log("UIDocumentGameObjectPlacer: STALLING, UIDocument rootVisualElement is null");
            return false;
        }
        VisualElement enemyElement = element.Q<VisualElement>(ENEMY_UIDOC_ELEMENT_PREFIX + INITIAL_INDEX);
        if(enemyElement == null || float.IsNaN(enemyElement.worldBound.width)) {
            Debug.Log("UIDocumentGameObjectPlacer: STALLING, Enemy element not ready");
            return false;
        }

        VisualElement companionElement = element.Q<VisualElement>(COMPANION_UIDOC_ELEMENT_PREFIX + INITIAL_INDEX);
        if(companionElement == null || float.IsNaN(companionElement.worldBound.width)) {
            Debug.Log("UIDocumentGameObjectPlacer: STALLING,  Companion element not ready");
            return false;
        }
        return true;

    }

    public WorldPositionVisualElement checkoutCompanionMapping(){
        return companionPlacements.checkoutPlacement();
    }

    public WorldPositionVisualElement checkoutEnemyMapping(){
        return enemyPlacements.checkoutPlacement();
    }

    public WorldPositionVisualElement CreateCardSlot(Action callback = null)
    {
        VisualElement newCardContainer = new VisualElement();
        newCardContainer.AddToClassList("companion-card-placer");
        var card = new VisualElement();
        //card.name = UIDocumentGameObjectPlacer.CARD_UIDOC_ELEMENT_PREFIX + index;
        card.AddToClassList("cardPlace");
        newCardContainer.Add(card);

        // when the visual element is done understanding it's new position call the provided callback function
        if (callback != null) {
            EventCallback<GeometryChangedEvent> handler = null;
            handler = evt => {
                (evt.target as VisualElement).UnregisterCallback(handler);
                callback();
            };
            newCardContainer.RegisterCallback(handler);
        }

        cardsContainer.Add(newCardContainer);
        UpdateSlotStylings();
        return new WorldPositionVisualElement(newCardContainer, UIDocumentGameObjectPlacer.GetWorldPositionFromElement(newCardContainer));
    }

    public void UpdateSlotStylings() {
        float center = (cardsContainer.childCount - 1) / 2.0f;
        Debug.Log(center);
        for (var i = 0; i < cardsContainer.childCount; i++) {
            VisualElement child = cardsContainer[i];
            child.style.rotate = new Rotate(new Angle((center - i) * 2.0f));
            child.style.translate = new StyleTranslate(new Translate(0, 10f * Math.Abs(i - center)));
        }
    }

    public WorldPositionVisualElement GetCardWPVEFromGO(GameObject GO)
    {
        return cardPlacements.GetCardWPVEFromGO(GO);
    }

    public void RemoveCardSlot(GameObject GO, Action onLayoutComplete = null) {
        if (GO == null) return;
        WorldPositionVisualElement WPVE = GetCardWPVEFromGO(GO);
        if (WPVE == null) return;
        VisualElement ve = WPVE.ve;
        if (ve == null || cardsContainer == null) return;
        
        if (cardsContainer.childCount > 1) {
            VisualElement siblingToWatch = null;
            foreach (var child in cardsContainer.Children()) {
                if (child != ve) {
                    siblingToWatch = child;
                    break;
                }
            }
            
            if (siblingToWatch != null && onLayoutComplete != null) {
                EventCallback<GeometryChangedEvent> handler = null;
                handler = evt => {
                    siblingToWatch.UnregisterCallback(handler);
                    onLayoutComplete();
                };
                
                siblingToWatch.RegisterCallback(handler);
            }
        } else if (onLayoutComplete != null) {
            onLayoutComplete();
        }
        
        ve.RemoveFromHierarchy();
        UpdateSlotStylings();
    }

    public void addMapping(WorldPositionVisualElement wpve, GameObject go) {
        PlacementPool pool = getPoolFromGameObject(go);
        pool.addMapping(wpve, go);
    }

    public void removeMapping(GameObject gameObject) {
        PlacementPool pool = getPoolFromGameObject(gameObject);
        pool.removeMapping(gameObject);
    }

    private PlacementPool getPoolFromGameObject(GameObject go) {
        if(go.GetComponent<CompanionInstance>() != null) {
            return companionPlacements;
        } else if(go.GetComponent<EnemyInstance>() != null) {
            return enemyPlacements;
        } else if(go.GetComponent<PlayableCard>() != null) {
            return cardPlacements;
        } else {
            Debug.LogError("GameObject does not have a valid component for UIDocumentGameObjectPlacer");
            return null;
        }
    }

    public static Vector3 GetWorldPositionFromElement(VisualElement element) {
        Debug.Log("Getting world position for element: " + element.name);
        if(element == null) {
            Debug.LogError("UIDocumentGameObjectPlacer: Element not found for name: " + element.name);
            return new Vector3(0, 0, 0);
        }
        Debug.Log("element: " + element);
        Vector3 uiDocumentPosition =  new Vector3(
            element.worldBound.center.x,
            element.worldBound.center.y,
            0
        );
        Debug.Log("uiDocPos: " + uiDocumentPosition);
        return GetWorldPositionFromUIDocumentPosition(uiDocumentPosition);
    }

    public static Vector3 GetWorldPositionFromUIDocumentPosition(Vector3 uiDocumentPosition) {
        //get the height of the screen
        Vector3 screenPosition = new Vector3(
            uiDocumentPosition.x,
            Screen.height - uiDocumentPosition.y,
            0
        );
        Debug.Log("screenPosition: " + screenPosition);
        if(float.IsNaN(screenPosition.x) || float.IsNaN(screenPosition.y))
        {
            return Vector3.zero;
        }
        Vector3 worldPosition;
        try {
            worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        } catch {
            Debug.LogWarning("Tried to place a view element off the screen");
            return Vector3.zero;
        }
        Debug.Log("worldPosition: " + worldPosition);
        Vector3 worldPositionZCorrected = new Vector3(worldPosition.x, worldPosition.y, zPlane);  
        Debug.Log("worldPositionZCorrected: " + worldPositionZCorrected);
        return worldPositionZCorrected;
    }

    public int getCompanionPlacesCount() {
        int count = companionPlacements.getCount();
        Debug.Log("Companion places count: " + count);
        return count;
    }

    public int getEnemyPlacesCount() {
        int count = enemyPlacements.getCount();
        Debug.Log("Companion places count: " + count);
        return count;
    }
}