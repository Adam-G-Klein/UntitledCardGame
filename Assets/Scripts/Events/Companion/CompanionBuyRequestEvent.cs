using UnityEngine;

[System.Serializable]
public class CompanionBuyRequest {
    public Companion companion;
    public int price;
    public GameObject keepsakeInShop;

    public CompanionBuyRequest(Companion companion, int price, GameObject keepsakeInShop) {
        this.companion = companion;
        this.price = price;
        this.keepsakeInShop = keepsakeInShop;
    }
}

[CreateAssetMenu(
    fileName = "NewCompanionBuyRequestEvent", 
    menuName = "Companions/Companion/Companion Buy Request Event")]
public class CompanionBuyRequestEvent : BaseGameEvent<CompanionBuyRequest> { }
