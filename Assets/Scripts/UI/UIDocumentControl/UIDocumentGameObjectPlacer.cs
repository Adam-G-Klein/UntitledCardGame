using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
// A mapping between a gameObject and a UIDocument element name
public class UIDocGOMapping {
    public string elementName;
    public GameObject gameObject;
    
    public UIDocGOMapping(GameObject gameObject, string UIDocumentElementName) {
        this.elementName = UIDocumentElementName;
        this.gameObject = gameObject;
    }
}

public class UIElementPlacement {
    public string elementName;
    public bool inUse;

    public UIElementPlacement(string elementName, bool inUse) {
        this.elementName = elementName;
        this.inUse = inUse;
    }

}

// Feel free to rename if you can think of a better name
// This is used to get / keep track of the name we should be using 
// for the next placed gameObject in the UIdoc.
// Could be combined with the mapping in the future, but for now this
// is more specific
public class PlacementPool {
    public int index;
    public string prefix;
    public PlacementPool(string prefix, int index) {
        this.index = index;
        this.prefix = prefix;
    }

    public string get() {
        return prefix + index;
    }

    public string getAndIncrement() {
        if(index < 0) {
            Debug.LogError("Element index is less than 0 for prefix: " + prefix);
        }
        return prefix + index++;
    }
    
    public string getAndDecrement() {
        string s = prefix + index--;
        if (index < 0) {
            Debug.LogError("Element index is less than 0 for prefix: " + prefix);
        }
        return s;
    }
}
// lets make this a singleton, it should work as our new location store with the encounterBuilder
public class UIDocumentGameObjectPlacer : GenericSingleton<UIDocumentGameObjectPlacer> {

    [SerializeField]
    private bool autoPlaceEnemies = true;
    private static string ENEMY_UIDOC_ELEMENT_PREFIX = "enemy";
    private static string COMPANION_UIDOC_ELEMENT_PREFIX = "companion";
    private static string HAND_UIDOC_ELEMENT_PREFIX = "hand";
    private static int INITIAL_INDEX = 1;

    [SerializeField]
    private bool autoPlaceCompanions = true;
    public List<UIDocGOMapping> mappings = new List<UIDocGOMapping>();
    private PlacementPool companionIndex = new PlacementPool(COMPANION_UIDOC_ELEMENT_PREFIX, INITIAL_INDEX);
    private PlacementPool enemyIndex = new PlacementPool(ENEMY_UIDOC_ELEMENT_PREFIX, INITIAL_INDEX);
    private PlacementPool handIndex = new PlacementPool(HAND_UIDOC_ELEMENT_PREFIX, INITIAL_INDEX);

    public float zPlane = -10;
    void Start() {
        companionIndex = new PlacementPool(COMPANION_UIDOC_ELEMENT_PREFIX, INITIAL_INDEX);
        enemyIndex = new PlacementPool(ENEMY_UIDOC_ELEMENT_PREFIX, INITIAL_INDEX);        
        handIndex = new PlacementPool(HAND_UIDOC_ELEMENT_PREFIX, INITIAL_INDEX);         
    }

    void Update() {

        // Set the gameobject to the worldspace position of the element in the ui document (which is in screenspace)
        //PlaceMappings();
    }

    private void PlaceMappings() {
        foreach(UIDocGOMapping mapping in mappings) {
            mapping.gameObject.transform.position = getWorldPositionFromMapping(mapping);
        }
    }

    public bool UIDocumentReady() {
        VisualElement element = GetComponent<UIDocument>().rootVisualElement;
        if(element == null) {
            Debug.LogError("UIDocumentGameObjectPlacer: UIDocument rootVisualElement is null");
            return false;
        }
        VisualElement enemyElement = element.Q<VisualElement>(ENEMY_UIDOC_ELEMENT_PREFIX + INITIAL_INDEX);
        if(enemyElement == null || enemyElement.worldBound.width == float.NaN) {
            Debug.LogError("UIDocumentGameObjectPlacer: Enemy element not ready");
            return false;
        }

        VisualElement companionElement = element.Q<VisualElement>(COMPANION_UIDOC_ELEMENT_PREFIX + INITIAL_INDEX);
        if(companionElement == null || companionElement.worldBound.width == float.NaN) {
            Debug.LogError("UIDocumentGameObjectPlacer: Companion element not ready");
            return false;
        }
        return true;

    }


    public Vector3 addMapping(GameObject gameObject) {
        string elementName = getElementIndexFromGameObject(gameObject).getAndIncrement();
        mappings.Add(new UIDocGOMapping(gameObject, elementName));
        return getWorldPositionFromGameObject(gameObject);
    }

    public void removeMapping(GameObject gameObject) {
        string elementName = getElementIndexFromGameObject(gameObject).getAndDecrement();
        mappings.RemoveAll(mapping => mapping.elementName == elementName);
    }

    // TODO: a class for each of the indices, and a method to get the next index while incrementing
    private PlacementPool getElementIndexFromGameObject(GameObject gameObject, bool incrementIndex = false) {
        if(gameObject.GetComponent<CompanionInstance>() != null) {
            return companionIndex;
        } else if(gameObject.GetComponent<EnemyInstance>() != null) {
            return enemyIndex;
        } else if(gameObject.GetComponent<PlayableCard>() != null) {
            return handIndex;
        } else {
            Debug.LogError("GameObject does not have a valid component for UIDocumentGameObjectPlacer");
            return null;
        }
    }

    private Vector3 getWorldPositionFromGameObject(GameObject gameObject) {
        UIDocGOMapping mapping = getMappingFromGameObject(gameObject);
        Debug.Log("UIDocumentGameObjectPlacer: UIDocGOMapping from gamebject " + gameObject.name + ": " + mapping);
        return getWorldPositionFromMapping(mapping);
    }

    private Vector3 getWorldPositionFromMapping(UIDocGOMapping mapping) {
        return getWorldPositionFromElementName(mapping.elementName);
    }

    private UIDocGOMapping getMappingFromGameObject(GameObject gameObject) {
        return mappings.Find(mapping => mapping.gameObject == gameObject);
    }

    // Used for the initial prefab instantiation call, so we don't flicker
    // objects to random positions before their mappings are added
    public Vector3 getNextEnemyPosition() {
        return getNextPosition(ENEMY_UIDOC_ELEMENT_PREFIX);
    }

    // Used for the initial prefab instantiation call, so we don't flicker
    // objects to random positions before their mappings are added
    public Vector3 getNextCompanionPosition() {
        return getNextPosition(COMPANION_UIDOC_ELEMENT_PREFIX);
    }

    // Used for the initial prefab instantiation call, so we don't flicker
    // objects to random positions before their mappings are added
    public Vector3 getNextHandPosition() {
        return getNextPosition(HAND_UIDOC_ELEMENT_PREFIX);
    }

    private Vector3 getNextPosition(string prefix) {
        if(prefix == ENEMY_UIDOC_ELEMENT_PREFIX) {
            return getNextPositionFromIndex(enemyIndex);
        } else if(prefix == COMPANION_UIDOC_ELEMENT_PREFIX) {
            return getNextPositionFromIndex(companionIndex);
        } else if (prefix == HAND_UIDOC_ELEMENT_PREFIX) {
            return getNextPositionFromIndex(handIndex);
        } else {
            Debug.LogError("Invalid prefix for getNextPosition");
            return new Vector3(0, 0, 0);
        }
    }

    private Vector3 getNextPositionFromIndex(PlacementPool index) {
        return getWorldPositionFromElementName(index.get());
    }

    private Vector3 getWorldPositionFromElementName(string elementName) {
        Debug.Log("Getting world position for element: " + elementName);
        VisualElement element = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(elementName);
        if(element == null) {
            Debug.LogError("UIDocumentGameObjectPlacer: Element not found for name: " + elementName);
            return new Vector3(0, 0, 0);
        }
        Debug.Log("element: " + element);
        Vector3 uiDocumentPosition =  new Vector3(
            element.worldBound.center.x,
            element.worldBound.center.y,
            0
        );
        Debug.Log("uiDocPos: " + uiDocumentPosition);
        //get the height of the screen
        Vector3 screenPosition = new Vector3(
            uiDocumentPosition.x,
            Screen.height - uiDocumentPosition.y,
            0
        );
        Debug.Log("screenPosition: " + screenPosition);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        Debug.Log("worldPosition: " + worldPosition);
        Vector3 worldPositionZCorrected = new Vector3(worldPosition.x, worldPosition.y, zPlane);  
        Debug.Log("worldPositionZCorrected: " + worldPositionZCorrected);
        return worldPositionZCorrected;
    }

    public int getEnemyPlacesCount() {
        int count = getPlacesCount(ENEMY_UIDOC_ELEMENT_PREFIX);
        Debug.Log("Enemy places count: " + count);
        return count;
    }

    public int getCompanionPlacesCount() {
        int count = getPlacesCount(COMPANION_UIDOC_ELEMENT_PREFIX);
        Debug.Log("Companion places count: " + count);
        return count;
    }


    private int getPlacesCount(string prefix) {
        PlacementPool index = new PlacementPool(prefix, 1);
        int loopSaver = 0;
        while(GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(index.getAndIncrement()) != null && loopSaver < 1000) { 
            Debug.Log("UIDocumentGameObjectPlacer.getPlacesCount: found element before: " + index.get());
            loopSaver++; 
            }
        if (loopSaver >= 1000) {
            Debug.LogError("UIDocumentGameObjectPlacer.getPlacesCount looped more than 1000 times, returning 0");
            return 0;
        }
        return index.index - 2;
    }






}