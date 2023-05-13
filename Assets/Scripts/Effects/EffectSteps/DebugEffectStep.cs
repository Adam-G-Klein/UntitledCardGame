using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugEffectStep : EffectStep {
    [SerializeField]
    private bool companions;
    [SerializeField]
    private bool minions;
    [SerializeField]
    private bool enemies;
    [SerializeField]
    private bool cards;
    [SerializeField]
    private bool playableCards;
    [SerializeField]
    private bool ints;
    [SerializeField]
    private bool strings;

    public DebugEffectStep() {
        effectStepName = "DebugEffectStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        if (companions)
            document.companionMap.printDictionary();
        if (minions)
            document.minionMap.printDictionary();
        if (enemies)
            document.enemyMap.printDictionary();
        if (cards)
            document.cardMap.printDictionary();
        if (playableCards)
            document.playableCardMap.printDictionary();
        if (ints)
            document.printIntMap();
        if (strings)
            document.printStringMap();

        yield return null;
    }
}