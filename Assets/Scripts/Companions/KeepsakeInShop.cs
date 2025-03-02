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

    [SerializeField]
    private GameObject soldOutSign;

    bool isSold = false;

    // Start is called before the first frame update
    void Start()
    {
        this.priceText.text = price.ToString();
        this.id = Id.newGuid();
        this.hoverBackground.enabled = false;
        this.keepsakeImage.sprite = companion.companionType.keepsake;
        this.soldOutSign.SetActive(false);
    }

    public void Setup() {
        Start();
    }


    public void OnPointerClick(PointerEventData eventData) {
        if (isSold) return;
        CompanionBuyRequest companionBuyRequest = new CompanionBuyRequest(
            companion, 
            price, 
            this);
        // ShopManager.Instance.processCompanionBuyRequest(companionBuyRequest);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSold) return;
        hoverBackground.enabled = true;
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSold) return;
        hoverBackground.enabled = false;
    }

    public void shopRefreshEventHandler() {
        Destroy(this.gameObject);
    }

    public void sold() {
        isSold = true;
        hoverBackground.enabled = false;
        soldOutSign.SetActive(true);
        keepsakeImage.gameObject.SetActive(false);
    }
}
