using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(
    fileName ="Card",
    menuName = "Cards/New Card")]
public class CardInfo : ScriptableObject
{
    public string Name;
    public string Description;
    public int Cost;
    public Sprite Artwork;
    /// I think the next step here is to have an EffectProcedureClassName
    /// field and then have a dictionary of EffectProcedureClassName to
    /// EffectProcedure. Then we can have a custom editor that lets us
    /// select the EffectProcedureClassName and then have a custom editor
    /// for that EffectProcedureClassName that lets us set the fields
    /// wow written by autopilot I'm fucked lol
    [SerializeField]
    public List<SimpleEffect> EffectProcedures;
    public string id = Id.newGuid();
}
