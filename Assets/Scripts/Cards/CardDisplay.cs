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
    public Color modifiedManaCostColor = Color.green;
    public Color initManaCostColor;
    public TextMeshProUGUI manaCostText;

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
                && TypeIcon != null;
        }
    }

    // Start is called before the first frame update
    // TODO, run on instantiation
    void Update()
    {
        // this code is changing anyways :p
        if(initialized && (cardInfo.CardModificationsHasKey(CardModification.ThisTurnManaDecrease)
            || cardInfo.CardModificationsHasKey(CardModification.ThisCombatManaDecrease))) {
                manaCostText.color = modifiedManaCostColor;
                manaCostText.text = cardInfo.GetManaCost().ToString();
            } else {
                manaCostText.color = initManaCostColor;
            }
        
    }

    void Awake()
    {
        if(cardInfo == null)
        {
            Debug.LogError("CardDisplay " + gameObject.name + " has no cardInfo");
        }
    }

    public void Initialize(Card cardInfo) {
        StartCoroutine(InitializeCorout(cardInfo));
    }

    public IEnumerator InitializeCorout(Card cardInfo) {
        this.cardInfo = cardInfo;
        // these are also done on Update(), keeping em here in case we wanna remove that
        // easy perf boost if we can
        CardNameGO.text = cardInfo.name;
        CardDescGO.text = cardInfo.description;
        CostTextGO.text = cardInfo.GetManaCost().ToString();
        manaCostText = CostTextGO.GetComponent<TextMeshProUGUI>();
        initManaCostColor = manaCostText.color;
        ArtworkGO.sprite = cardInfo.artwork;
        CompanionTypeSO companionType = cardInfo.getCompanionFrom();
        if (companionType == null) {
            Debug.LogError("No companion from provided to cardDisplay or present in cardInfo. cardInfo: " + cardInfo.name + " companionType: " + companionType);
        }
        else if(companionType.cardIdleVfxPrefab) {
            vfxGO = Instantiate(companionType.cardIdleVfxPrefab, transform);
            vfxGO.transform.SetSiblingIndex(0);
        }
        if(companionType.cardFrame == null) {
            Debug.LogError("CompanionTypeSO " + companionType.name + " has no cardFrame set");
        }  
        FrameGO.sprite = companionType.cardFrame;
        Frame = FrameGO.sprite;
        if(companionType.typeIcon == null) {
            Debug.LogError("CompanionTypeSO " + companionType.name + " has no typeIcon set");
        }
        TypeIconGO.sprite = companionType.typeIcon;
        TypeIcon = TypeIconGO.sprite;
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
