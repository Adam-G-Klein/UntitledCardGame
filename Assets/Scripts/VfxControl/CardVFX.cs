using UnityEngine;

/*
    Plays a small effect on a card in hand (it triggered, changed, or was retained), drawn just above the card
    so it isn't hidden behind it. With follow on, the effect rides the card as the hand moves.
*/
public static class CardVFX
{
    public static GameObject Play(GameObject prefab, Component card, bool follow = false)
    {
        if (prefab == null || card == null) return null;
        Vector3 at = card.transform.position + new Vector3(0f, 0f, -0.5f);
        GameObject fx = Object.Instantiate(prefab, at, Quaternion.identity);
        if (follow) fx.transform.SetParent(card.transform, true);
        SpriteRenderer face = card.GetComponent<SpriteRenderer>();
        if (face != null)
            foreach (Renderer r in fx.GetComponentsInChildren<Renderer>(true))
            {
                r.sortingLayerID = face.sortingLayerID;
                r.sortingOrder = face.sortingOrder + 20 + r.sortingOrder;
            }
        return fx;
    }
}
