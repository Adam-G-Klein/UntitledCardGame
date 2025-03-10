using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class FilterCards : EffectStep, IEffectStepCalculation {

    [Header("Gets the PlayableCards and Cards at input key, returns all Cards that meet filter condition from both,\n" +
        "also outputs PlayableCards meeting condition at the same key")]
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private CardFilter filter;
    [SerializeField]
    private string outputKey = "";

    public FilterCards() {
        effectStepName = "FilterCards";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<Card> inputCards = new List<Card>();
        List<PlayableCard> inputPlayableCards = new List<PlayableCard>();
        if(document.map.ContainsValueWithKey<Card>(inputKey)) {
            inputCards = document.map.GetList<Card>(inputKey);
        }
        if(document.map.ContainsValueWithKey<PlayableCard>(inputKey)) {
            inputPlayableCards = document.map.GetList<PlayableCard>(inputKey);
        }
        List<Card> outputCards = new List<Card>();
        List<PlayableCard> playableCards = new List<PlayableCard>();
        foreach (Card card in inputCards) {
            if (filter.ApplyFilter(card)) {
                outputCards.Add(card);
            }
        }
        foreach (PlayableCard card in inputPlayableCards) {
            if (filter.ApplyFilter(card.card)) {
                playableCards.Add(card);
            }
        }
        document.map.AddItems<Card>(outputKey, outputCards, outputKey);
        document.map.AddItems<PlayableCard>(outputKey, playableCards, outputKey);
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}