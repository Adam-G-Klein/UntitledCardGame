using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    [SerializeField]
    private Card cardInfo;

    public TMP_Text CardNameGO;
    public TMP_Text CostTextGO;
    public TMP_Text CardDescGO;
    public Image ArtworkGO;
    public Image FrameGO;
    public Image TypeIconGO;
    // make a placeholder with a rectangle that has the outline effect on it
    public GameObject vfxGO;
    [Header("Set by referencing the companionType passed in code")]
    public Sprite Frame;
    public Sprite TypeIcon;
    public Sprite CardBack;

    public bool initialized {
        get {
            return CardNameGO != null 
                && CostTextGO != null 
                && CardDescGO != null 
                && ArtworkGO != null
                && FrameGO != null
                && TypeIconGO != null
                && FrameGO != null
                && Frame != null
                && TypeIcon != null
                && CardBack != null;
        }
    }

    // Start is called before the first frame update
    // TODO, run on instantiation
    void Update()
    {
        // runInEditMode = true;
        if(CardNameGO == null || CostTextGO == null || CardDescGO == null || ArtworkGO == null)
        {
            Debug.LogError("CardDisplay " + cardInfo.cardType + " is missing a reference to a UI element");
        } else {
            CardNameGO.text = cardInfo.name;
            CardDescGO.text = cardInfo.description;
            CostTextGO.text = cardInfo.GetManaCost().ToString();
            ArtworkGO.sprite = cardInfo.artwork;

        }
    }

    void Awake()
    {
        if(cardInfo == null)
        {
            Debug.LogError("CardDisplay " + gameObject.name + " has no cardInfo");
        }
    }

    public void Initialize(Card cardInfo, CompanionTypeSO companionType = null) {
        StartCoroutine(InitializeCorout(cardInfo, companionType));
    }

    public IEnumerator InitializeCorout(Card cardInfo, CompanionTypeSO companionType = null) {
        this.cardInfo = cardInfo;
        // these are also done on Update(), keeping em here in case we wanna remove that
        // easy perf boost if we can
        CardNameGO.text = cardInfo.name;
        CardDescGO.text = cardInfo.description;
        CostTextGO.text = cardInfo.GetManaCost().ToString();
        ArtworkGO.sprite = cardInfo.artwork;
        if(companionType != null) {
            cardInfo.setCompanionFrom(companionType);
            Debug.Log("companion type had needed assets? " + 
                (companionType.typeIcon != null) + " " + 
                (companionType.cardBack != null) + " " + 
                (companionType.cardFrame != null));
        } else {
            companionType = cardInfo.getCompanionFrom();
            if (companionType == null)
                Debug.LogError("No companion from provided to cardDisplay or present in cardInfo. cardInfo: " + cardInfo.name + " companionType: " + companionType);
        }
        if(companionType.cardIdleVfxPrefab) {
            vfxGO = Instantiate(companionType.cardIdleVfxPrefab, transform);
            vfxGO.transform.SetSiblingIndex(0);
        }
        FrameGO.sprite = companionType.cardFrame;
        TypeIconGO.sprite = companionType.typeIcon;
        yield return null;


    }

    // Looks like double calls happen when something in the hierarchy is using OnGUI()
    // https://forum.unity.com/threads/onmouseenter-exit-double-call.19553/
    void OnMouseEnter()
    {
    }
    void OnMouseExit()
    {
    }

    public Card getCardInfo() {
        return cardInfo;
    }


}
