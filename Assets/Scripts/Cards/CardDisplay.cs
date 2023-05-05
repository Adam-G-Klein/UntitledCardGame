using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Won't execute in prefab edit mode, 
// apparently there's some weird logic there
// we'd have to account for
[ExecuteInEditMode]
public class CardDisplay : MonoBehaviour
{
    public Card cardInfo;

    public TMP_Text CardName;
    public TMP_Text CostText;
    public TMP_Text CardDesc;
    public Image Artwork;
    private Camera mainCamera;

    // Start is called before the first frame update
    void Update()
    {
        // runInEditMode = true;
        CardName.text = cardInfo.name;
        CardDesc.text = cardInfo.description;
        CostText.text = cardInfo.cost.ToString();
        Artwork.sprite = cardInfo.artwork;
    }

    void Awake()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
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
