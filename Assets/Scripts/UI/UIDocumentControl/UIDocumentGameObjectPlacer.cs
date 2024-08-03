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
    public string UIDocumentElementName;
    public GameObject gameObject;
    
    public UIDocGOMapping(GameObject gameObject, string UIDocumentElementName) {
        this.UIDocumentElementName = UIDocumentElementName;
        this.gameObject = gameObject;
    }
}

// Feel free to rename if you can think of a better name
// This is used to get / keep track of the name we should be using 
// for the next placed gameObject in the UIdoc.
// Could be combined with the mapping in the future, but for now this
// is more specific
public class ElementIndex {
    public int index;
    public string prefix;
    public ElementIndex(string prefix, int index) {
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
    private static string CARD_UIDOC_ELEMENT_PREFIX = "card";
    private static int INITIAL_INDEX = 1;

    [SerializeField]
    private bool autoPlaceCompanions = true;
    public List<UIDocGOMapping> mappings = new List<UIDocGOMapping>();
    private ElementIndex companionIndex = new ElementIndex(COMPANION_UIDOC_ELEMENT_PREFIX, INITIAL_INDEX);
    private ElementIndex enemyIndex = new ElementIndex(ENEMY_UIDOC_ELEMENT_PREFIX, INITIAL_INDEX);
    private ElementIndex cardIndex = new ElementIndex(CARD_UIDOC_ELEMENT_PREFIX, INITIAL_INDEX);

    public float zPlane = -10;
    void Start() {
        companionIndex = new ElementIndex(COMPANION_UIDOC_ELEMENT_PREFIX, INITIAL_INDEX);
        enemyIndex = new ElementIndex(ENEMY_UIDOC_ELEMENT_PREFIX, INITIAL_INDEX);        
        cardIndex = new ElementIndex(CARD_UIDOC_ELEMENT_PREFIX, INITIAL_INDEX);         
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
        mappings.RemoveAll(mapping => mapping.UIDocumentElementName == elementName);
    }

    // TODO: a class for each of the indices, and a method to get the next index while incrementing
    private ElementIndex getElementIndexFromGameObject(GameObject gameObject, bool incrementIndex = false) {
        if(gameObject.GetComponent<CompanionInstance>() != null) {
            return companionIndex;
        } else if(gameObject.GetComponent<EnemyInstance>() != null) {
            return enemyIndex;
        } else if(gameObject.GetComponent<PlayableCard>() != null) {
            return cardIndex;
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
        return getWorldPositionFromElementName(mapping.UIDocumentElementName);
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
    public Vector3 getNextCardPosition() {
        return getNextPosition(CARD_UIDOC_ELEMENT_PREFIX);
    }

    private Vector3 getNextPosition(string prefix) {
        if(prefix == ENEMY_UIDOC_ELEMENT_PREFIX) {
            return getNextPositionFromIndex(enemyIndex);
        } else if(prefix == COMPANION_UIDOC_ELEMENT_PREFIX) {
            return getNextPositionFromIndex(companionIndex);
        } else if (prefix == CARD_UIDOC_ELEMENT_PREFIX) {
            return getNextPositionFromIndex(cardIndex);
        } else {
            Debug.LogError("Invalid prefix for getNextPosition");
            return new Vector3(0, 0, 0);
        }
    }

    private Vector3 getNextPositionFromIndex(ElementIndex index) {
        return getWorldPositionFromElementName(index.get());
    }

    private Vector3 getWorldPositionFromElementName(string elementName) {
        Debug.Log("Getting world position for element: " + elementName);
        VisualElement element = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(elementName);
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
        ElementIndex index = new ElementIndex(prefix, 1);
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