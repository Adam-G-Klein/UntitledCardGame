using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "StaticShopEncounters",
    menuName = "Encounters/Shop/New Static Shop Encounters")]
public class StaticShopEncountersSO : ScriptableObject
{
    [Header("One set of cards and companions for each shop encounter that will be shown first")]
    public List<StaticShopEncounter> shopEncounters;
}

[Serializable]
public class StaticShopEncounter
{
    public List<CardType> cardTypes;
    public List<CompanionTypeSO> companionTypes;
}
