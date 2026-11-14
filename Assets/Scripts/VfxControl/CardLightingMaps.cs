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

    private void Start()
    {
        PlayableCard playable = GetComponent<PlayableCard>();
        if (playable == null || playable.card == null) return;

        Material shineMaterial = GetComponent<SpriteRenderer>().material;
        if (shineMaterial.HasProperty(ShineColorId)) shineMaterial.SetColor(ShineColorId, ShineFor(playable.card.shopRarity));

        PackSO pack = playable.card.cardType.packFrom;
        if (pack == null && playable.deckFrom != null && playable.deckFrom.GetCompanionTypeSO() != null)
            pack = playable.deckFrom.GetCompanionTypeSO().pack;
        if (pack == null) return;

        PackMaps maps = packMaps.Find(m => m.pack == pack);
        if (maps == null) return;

        if (maps.normal != null) shineMaterial.SetTexture(NormalId, maps.normal);
        if (maps.shineMask != null) shineMaterial.SetTexture(ShineMaskId, maps.shineMask);
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
