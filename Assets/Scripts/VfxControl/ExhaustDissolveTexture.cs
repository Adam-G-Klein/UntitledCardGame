using UnityEngine;

/*
    The exhaust experience dissolves the card it's bound to through the card shader's fade texture (its timeline
    animates the cutoff). The shared card material's fade texture is a generic noise that the cast, consume and
    create animations also use, so this swaps it on the exhausting card's own material instance only, so the
    card washes away in the game's watercolour. The card is parented under the experience before it starts.
*/
public class ExhaustDissolveTexture : MonoBehaviour
{
    [SerializeField] private Texture fadeTexture;

    private static readonly int FadeTex = Shader.PropertyToID("_Fade_Tex");

    private void Start()
    {
        if (fadeTexture == null) return;
        PlayableCard card = GetComponentInChildren<PlayableCard>();
        if (card == null) return;
        SpriteRenderer face = card.GetComponent<SpriteRenderer>();
        if (face != null && face.material.HasProperty(FadeTex)) face.material.SetTexture(FadeTex, fadeTexture);
    }
}
