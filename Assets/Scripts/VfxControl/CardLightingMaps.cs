using System;
using System.Collections.Generic;
using UnityEngine;

/*
    Gives each card the lighting maps for its frame, so only that frame's lines and ornaments catch
    the light (CS_WorldCardShader: embossed normal + shine mask that drives smoothness and the shine
    colour). The frame comes from the card's pack, the same way CardView picks the card back.

    Works on the card's own material instance (the one UIDocumentCard already uses for its render
    texture), so the shared CM_WorldCardShader material is never changed. A pack with no entry keeps
    the material's default maps.

    The shine is tinted to the card's rarity metal. The three frames differ only in the metal of their
    border lines and gems (copper, silver, gold), which is exactly what the shine lights, so a single
    gold shine made every rarity read as gold. Cards with no rarity show the copper frame (CardView),
    so they shine copper too.
*/
[RequireComponent(typeof(SpriteRenderer))]
public class CardLightingMaps : MonoBehaviour
{
    [Serializable]
    public class PackMaps
    {
        public PackSO pack;
        public Texture2D normal;
        public Texture2D shineMask;
    }

    [SerializeField] private List<PackMaps> packMaps = new List<PackMaps>();

    [Header("Shine by rarity (HDR)")]
    [Tooltip("Common: the copper frame. A little softer than the others.")]
    [ColorUsage(false, true)] [SerializeField] private Color commonShine = new Color(4.6f, 2.5f, 1.4f);
    [Tooltip("Uncommon: the silver frame. Cool white.")]
    [ColorUsage(false, true)] [SerializeField] private Color uncommonShine = new Color(4.7f, 5.1f, 5.7f);
    [Tooltip("Rare: the gold frame. The material's own gold, the strongest.")]
    [ColorUsage(false, true)] [SerializeField] private Color rareShine = new Color(6.1f, 5.1f, 3.4f);

    private static readonly int NormalId = Shader.PropertyToID("_NormalMainTex");
    private static readonly int ShineMaskId = Shader.PropertyToID("_ShineMask");
    private static readonly int ShineColorId = Shader.PropertyToID("_Shine_Color");

    [Tooltip("Seconds the shine takes to go out once the card starts casting (it read on for a beat after the card was played).")]
    [SerializeField] private float castFadeTime = 0.1f;

    private PlayableCard playable;
    private Material shineMaterial;
    private Color shineColor;
    private float shineLevel = 1f;
    private bool castStarted;

    private void Start()
    {
        playable = GetComponent<PlayableCard>();
        if (playable == null || playable.card == null) return;

        shineMaterial = GetComponent<SpriteRenderer>().material;
        shineColor = ShineFor(playable.card.shopRarity);
        if (shineMaterial.HasProperty(ShineColorId)) shineMaterial.SetColor(ShineColorId, shineColor);

        PackSO pack = playable.card.cardType.packFrom;
        if (pack == null && playable.deckFrom != null && playable.deckFrom.GetCompanionTypeSO() != null)
            pack = playable.deckFrom.GetCompanionTypeSO().pack;
        if (pack == null) return;

        PackMaps maps = packMaps.Find(m => m.pack == pack);
        if (maps == null) return;

        if (maps.normal != null) shineMaterial.SetTexture(NormalId, maps.normal);
        if (maps.shineMask != null) shineMaterial.SetTexture(ShineMaskId, maps.shineMask);
    }

    // the shine goes out as soon as the card starts casting (not when it lands), and comes back if the cast is
    // cancelled and the card returns to the hand
    private void Update()
    {
        if (playable == null || shineMaterial == null || !shineMaterial.HasProperty(ShineColorId)) return;
        if (playable.currentlyCastingCard) castStarted = true;
        else if (castStarted && PlayerHand.Instance != null && PlayerHand.Instance.ContainsPlayableCard(playable)) castStarted = false;   // cancelled
        bool off = castStarted;
        float target = off ? 0f : 1f;
        if (Mathf.Approximately(shineLevel, target)) return;
        shineLevel = Mathf.MoveTowards(shineLevel, target, Time.deltaTime / Mathf.Max(castFadeTime, 0.01f));
        shineMaterial.SetColor(ShineColorId, shineColor * shineLevel);
    }

    private Color ShineFor(Card.CardRarity rarity)
    {
        switch (rarity)
        {
            case Card.CardRarity.RARE: return rareShine;
            case Card.CardRarity.UNCOMMON: return uncommonShine;
            default: return commonShine;
        }
    }
}
