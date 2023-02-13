using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

public class UICompanion : MonoBehaviour
{
    public Companion companion;
    public CompanionViewUI companionViewUI;
    public Image image;
    public GameObject background;

    public void setup() {
        this.image.sprite = companion.getSprite();
    }

    public void companionClickedEventHandler() {
        companionViewUI.companionClickedEventHandler(this);
    }

    public void toggleBackground() {
        if (background.activeSelf == true) {
            background.SetActive(false);
        } else {
            background.SetActive(true);
        }
    }
}