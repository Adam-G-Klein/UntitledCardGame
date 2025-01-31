using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public Card card;

    public TMP_Text CardNameGO;
    public TMP_Text CostTextGO;
    public TMP_Text CardDescGO;

    public TMP_Text CardCategoryGO;

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

    public Image rarityDisplayGO;
    public Sprite commonRaritySymbol;
    public Sprite uncommonRaritySymbol;
    public Sprite rareRaritySymbol;

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
        if(initialized && (card.CardModificationsHasKey(CardModification.ThisTurnManaDecrease)
            || card.CardModificationsHasKey(CardModification.ThisCombatManaDecrease))) {
                manaCostText.color = modifiedManaCostColor;
                manaCostText.text = card.GetManaCost().ToString();
            } else {
                manaCostText.color = initManaCostColor;
            }

    }

    void Awake()
    {
        if(card == null)
        {
            Debug.LogError("CardDisplay " + gameObject.name + " has no cardInfo");
        }
    }

    public void Initialize(Card cardInfo) {
        StartCoroutine(InitializeCorout(cardInfo));
    }

    public IEnumerator InitializeCorout(Card cardInfo) {
        this.card = cardInfo;
        // these are also done on Update(), keeping em here in case we wanna remove that
        // easy perf boost if we can
        CardNameGO.text = cardInfo.name;
        string description = card.cardType.Description;
        foreach (var defaultValue in card.cardType.defaultValues)
        {
            description = description.Replace($"{{{defaultValue.key}}}", $"<b>{defaultValue.value}</b>"); 
        }
        CardDescGO.text = description;
        CostTextGO.text = cardInfo.GetManaCost().ToString();
        string cardCategoryText = "";
        if (cardInfo.generated) {
            cardCategoryText += "Generated - ";
        }
        cardCategoryText += cardInfo.cardType.cardCategory.ToString();
        CardCategoryGO.text = cardCategoryText;
        manaCostText = CostTextGO.GetComponent<TextMeshProUGUI>();
        initManaCostColor = manaCostText.color;
        ArtworkGO.sprite = cardInfo.artwork;
        CompanionTypeSO companionType = cardInfo.getCompanionFrom();
        if (companionType != null) {
            if(companionType.cardIdleVfxPrefab) {
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
        }
        if (rarityDisplayGO != null) {
            switch (cardInfo.shopRarity) {
            case Card.CardRarity.NONE:
                rarityDisplayGO.enabled = false;
                break;
            case Card.CardRarity.COMMON:
                rarityDisplayGO.sprite = commonRaritySymbol;
                break;
            case Card.CardRarity.UNCOMMON:
                rarityDisplayGO.sprite = uncommonRaritySymbol;
                break;
            case Card.CardRarity.RARE:
                rarityDisplayGO.sprite = rareRaritySymbol;
                break;
            }
        }

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
        return card;
    }


}
