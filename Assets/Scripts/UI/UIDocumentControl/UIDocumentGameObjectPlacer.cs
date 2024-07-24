using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class UIDocGOMapping {
    public string UIDocumentElementName;
    public GameObject gameObject;
    
    public UIDocGOMapping(GameObject gameObject, string UIDocumentElementName) {
        this.UIDocumentElementName = UIDocumentElementName;
        this.gameObject = gameObject;
    }
}
// lets make this a singleton, it should work as our new location store with the encounterBuilder
public class UIDocumentGameObjectPlacer : MonoBehaviour {

    [SerializeField]
    private bool autoPlaceEnemies = true;
    private static string ENEMY_UIDOC_ELEMENT_PREFIX = "enemy";
    private static string COMPANION_UIDOC_ELEMENT_PREFIX = "companion";
    private static string CARD_UIDOC_ELEMENT_PREFIX = "card";

    [SerializeField]
    private bool autoPlaceCompanions = true;
    public List<UIDocGOMapping> mappings = new List<UIDocGOMapping>();
    private int nextCompanionIndex = 0;
    private int nextEnemyIndex = 0;
    private int nextCardIndex = 0;
    void Start() {
        nextCompanionIndex = 0;
        nextEnemyIndex = 0;
        nextCardIndex = 0;
    }

    void Update() {

        // Set the gameobject to the worldspace position of the element in the ui document (which is in screenspace)
        foreach(UIDocGOMapping mapping in mappings) {
            VisualElement element = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(mapping.UIDocumentElementName);
            if (element != null) {
                Transform mappingTransform = mapping.gameObject.GetComponent<Transform>();
                Debug.Log("element.localBound.center.x: " + element.localBound.center.x);
                Debug.Log("element.localBound.center.y: " + element.localBound.center.y);
                Vector3 screenPosition =  new Vector3(
                    element.localBound.center.x,
                    element.localBound.center.y,
                    0
                );
                mappingTransform.position = Camera.main.ScreenToWorldPoint(screenPosition);

            }
        }
    }


    public void addMapping(GameObject gameObject) {
        string elementName = getStringFromGameObject(gameObject);
        mappings.Add(new UIDocGOMapping(gameObject, elementName));
    }

    public void removeMapping(GameObject gameObject) {
        string elementName = getStringFromGameObject(gameObject);
        mappings.RemoveAll(mapping => mapping.UIDocumentElementName == elementName);
        decrementIndexFromGameObject(gameObject);
    }

    private string getStringFromGameObject(GameObject gameObject, bool incrementIndex = false) {
        if(gameObject.GetComponent<CompanionInstance>() != null) {
            if(incrementIndex) {
                return COMPANION_UIDOC_ELEMENT_PREFIX + nextCompanionIndex++;
            } else {
                return COMPANION_UIDOC_ELEMENT_PREFIX + nextCompanionIndex;
            }
        } else if(gameObject.GetComponent<EnemyInstance>() != null) {
            if(incrementIndex) {
                return ENEMY_UIDOC_ELEMENT_PREFIX + nextEnemyIndex++;
            } else {
                return ENEMY_UIDOC_ELEMENT_PREFIX + nextEnemyIndex;
            }
        } else if(gameObject.GetComponent<PlayableCard>() != null) {
            if(incrementIndex) {
                return CARD_UIDOC_ELEMENT_PREFIX + nextCardIndex++;
            } else {
                return CARD_UIDOC_ELEMENT_PREFIX + nextCardIndex;
            }
        } else {
        }
    }

    private void decrementIndexFromGameObject(GameObject gameObject) {
        if (gameObject.GetComponent<CompanionInstance>() != null) {
            nextCompanionIndex--;
        } else if (gameObject.GetComponent<EnemyInstance>() != null) {
            nextEnemyIndex--;
        } else if (gameObject.GetComponent<PlayableCard>() != null) {
            nextCardIndex--;
        }
    }

}