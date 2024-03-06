using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public Card cardInfo;

    public TMP_Text CardName;
    public TMP_Text CostText;
    public TMP_Text CardDesc;
    public Image Artwork;
    private Camera mainCamera;

    // Start is called before the first frame update
    // TODO, run on instantiation
    void Update()
    {
        // runInEditMode = true;
        CardName.text = cardInfo.name;
        CardDesc.text = cardInfo.description;
        CostText.text = cardInfo.GetManaCost().ToString();
        Artwork.sprite = cardInfo.artwork;
    }

    void Awake()
    {
        mainCamera = Camera.main;
        if(cardInfo == null)
        {
            Debug.LogError("CardDisplay " + gameObject.name + " has no cardInfo");
        }
    }

    // Looks like double calls happen when something in the hierarchy is using OnGUI()
    // https://forum.unity.com/threads/onmouseenter-exit-double-call.19553/
    void OnMouseEnter()
    {
    }
    void OnMouseExit()
    {
    }

}
