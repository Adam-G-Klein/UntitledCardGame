using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class WorldspacePlayerHandManager : MonoBehaviour {


    public List<SpriteRenderer> worldSpaceCards = new List<SpriteRenderer>();

    public float placementPadding = 0.1f;

    // Update the layout of the worldspace hand, placing the cards on the same y axis as this transform,
    // but spaced out with placementPadding between each card on the x axis.
    private void UpdateLayout() {
        // Get the initial position based on half the total width of the hand

        float x = transform.position.x - totalWidth() / 2;
        foreach(SpriteRenderer card in worldSpaceCards) {
            card.transform.position = new Vector3(x, transform.position.y, transform.position.z);
            x += placementPadding + cardWidth();
        }
    }

    // Add a card to the worldspace hand
    public void AddCard(SpriteRenderer card) {
        worldSpaceCards.Add(card);
        UpdateLayout();
    }

    private float totalWidth() {
        float width = 0;
        foreach(SpriteRenderer card in worldSpaceCards) {
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
