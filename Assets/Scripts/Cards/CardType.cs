using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(
    fileName ="Card",
    menuName = "Cards/New Card Type")]
[Serializable]
public class CardType: ScriptableObject
{
    public string Name;
    public string Description;
    public int Cost;
    public Sprite Artwork;
    public CardCategory cardCategory;
    public GameObject vfxPrefab;
    // For unplayable status cards
    public bool playable = true;

    [SerializeReference]
    public List<EffectWorkflow> effectWorkflows;

    [SerializeReference]
    public EffectWorkflow onExhaustEffectWorkflow;

    // Revisit this implementation of card type level modifications
    public Dictionary<CardModification, int> cardModifications = new Dictionary<CardModification, int>();

    public void ResetCardModifications() {
        cardModifications = new Dictionary<CardModification, int>();
        foreach(int i in Enum.GetValues(typeof(CardModification))) {
            cardModifications.Add((CardModification)i, 0);
        }
    }

    public void ChangeCardModification(CardModification modification, int scale) {
        cardModifications[modification] += scale;
    }
}

public enum CardCategory {
    None,
    Attack,
    NonAttack,
    Saga,
    Status
}