using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public CardInfo cardInfo;

    public TMP_Text CardName;
    public TMP_Text CardDesc;
    public Image Artwork;

    // Start is called before the first frame update
    void Start()
    {
        CardName.text = cardInfo.Name;
        CardDesc.text = cardInfo.Description;
        Artwork.sprite = cardInfo.Artwork;
    }

    // TODO
    // move this to a cardEffect Manager 
    private void Update()
    {
        // left click to Cast this card
        if (Input.GetMouseButtonDown(0))
        {
            cardInfo.Cast();
        }
    }


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
