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
    public GameObject vfxPrefab;
    // For unplayable status cards
    public bool playable = true;
    // Need to SerializeReference to have the ScriptableObject 
    // Keep a reference to the procedures we add rather than
    // trying to reinstantiate them by value all the time.
    [SerializeReference]
    public List<EffectProcedure> EffectProcedures;
}
