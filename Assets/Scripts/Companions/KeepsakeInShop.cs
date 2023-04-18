using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 
using TMPro;

public class KeepsakeInShop : MonoBehaviour
    , IPointerClickHandler
    , IPointerEnterHandler
    , IPointerExitHandler
{
    public int price;
    public string id;

    public TMP_Text priceText;
    public Companion companion;
    public Image keepsakeImage;
    public Image hoverBackground;

    // Start is called before the first frame update
    void Start()
    {
        this.priceText.text = price.ToString();
        this.id = Id.newGuid();
        this.hoverBackground.enabled = false;
        this.keepsakeImage.sprite = companion.companionType.keepsake;
    }

    public void Setup() {
        Start();
    }


    public void OnPointerClick(PointerEventData eventData) {
        CompanionBuyRequest companionBuyRequest = new CompanionBuyRequest(
            companion, 
            price, 
            gameObject);
        ShopManager.Instance.processCompanionBuyRequest(companionBuyRequest);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverBackground.enabled = true;
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        hoverBackground.enabled = false;
    }

    public void shopRefreshEventHandler() {
        Destroy(this.gameObject);
    }
}
