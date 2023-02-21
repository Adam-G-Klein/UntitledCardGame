using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class RandomBackground: MonoBehaviour
{
    
    private Canvas canvas;
    void Start() {
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        Sprite[] backgroundSprites = Resources.LoadAll<Sprite>("Backgrounds");
        GetComponentInChildren<Image>().sprite = backgroundSprites[Random.Range(0, backgroundSprites.Length)];
    }
}