using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

// Has a list of gameobjects, places them in worldspace relative to this
// gameobjects transform

// For now, just put the cards to 
public class WorldspacePlayerHandManager : MonoBehaviour {


    public List<GameObject> worldSpaceCards = new List<GameObject>();

    public float placementPadding = 0.1f;

    // Update the layout of the worldspace hand, placing the cards on the same y axis as this transform,
    // but spaced out with placementPadding between each card on the x axis.
    private void UpdateLayout() {
        // Get the initial position based on half the total width of the hand

        float x = transform.position.x - totalWidth() / 2;
        foreach(GameObject card in worldSpaceCards) {
            card.transform.position = new Vector3(x, transform.position.y, transform.position.z);
            x += placementPadding + cardWidth();
        }
    }

    void OnDestroy() {
        Debug.Log("Hand destroyed");
    }

    // Add a card to the worldspace hand
    public void AddCard(GameObject card) {
        worldSpaceCards.Add(card);
        UpdateLayout();
    }

    public void RemoveCard(GameObject card) {
        worldSpaceCards.Remove(card);
        UpdateLayout();
    }

    private float totalWidth() {
        float width = 0;
        foreach(SpriteRenderer card in worldSpaceCards.Select(card => card.GetComponent<SpriteRenderer>())) {
            width += card.size.x;
        }
        return width;
    }

    private float cardWidth() {
        if(worldSpaceCards.Count == 0) {
            return 0;
        }
        return worldSpaceCards[0].GetComponent<SpriteRenderer>().size.x;
    }

    
}
