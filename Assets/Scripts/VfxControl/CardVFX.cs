using System.Reflection;
using UnityEngine;

/*
    Plays a small effect on a card in hand (it triggered, changed, or was retained), drawn just above the card
    so it isn't hidden behind it. With follow on, the effect rides the card as the hand moves.

    A following effect is sized for the card at rest, not at the moment it lands: a newly dealt card scales up
    from nothing (PlayerHand scales its wrapper from 0), so keeping the effect's world size when it was parented
    to a still-tiny card blew it up many times over as the card grew in (the drawn-card rim flashed as huge
    columns of light). It now takes the card's resting scale (its non-hover scale) as its own, so it grows in with
    the card and keeps the size it was built for (world scale 1 on a resting hand card).
*/
public static class CardVFX
{
    private static readonly FieldInfo RestingScaleField = typeof(PlayableCard).GetField("nonHoverScale", BindingFlags.Instance | BindingFlags.NonPublic);

    public static GameObject Play(GameObject prefab, Component card, bool follow = false)
    {
        if (prefab == null || card == null) return null;
        Vector3 at = card.transform.position + new Vector3(0f, 0f, -0.5f);
        GameObject fx = Object.Instantiate(prefab, at, Quaternion.identity);
        if (follow)
        {
            fx.transform.SetParent(card.transform, true);
            float resting = RestingScale(card);
            fx.transform.localScale = Vector3.one / Mathf.Max(0.0001f, resting);
            fx.transform.localRotation = Quaternion.identity;
        }
        SpriteRenderer face = card.GetComponent<SpriteRenderer>();
        if (face != null)
            foreach (Renderer r in fx.GetComponentsInChildren<Renderer>(true))
            {
                r.sortingLayerID = face.sortingLayerID;
                r.sortingOrder = face.sortingOrder + 20 + r.sortingOrder;
            }
        return fx;
    }

    // the card's scale at rest in the hand (its non-hover scale), or its current scale if that can't be read
    private static float RestingScale(Component card)
    {
        PlayableCard playable = card as PlayableCard ?? card.GetComponent<PlayableCard>();
        if (playable != null && RestingScaleField != null && RestingScaleField.GetValue(playable) is float s && s > 0f) return s;
        return card.transform.localScale.x;
    }
}
