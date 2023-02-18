using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RandomBackground: MonoBehaviour
{
    
    void Start() {
        Sprite[] backgroundSprites = Resources.LoadAll<Sprite>("Backgrounds");
        GetComponent<Image>().sprite = backgroundSprites[Random.Range(0, backgroundSprites.Length)];
    }
}