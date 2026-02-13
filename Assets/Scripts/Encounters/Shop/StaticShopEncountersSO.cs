using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "StaticShopEncounters",
    menuName = "Encounters/Shop/New Static Shop Encounters")]
public class StaticShopEncountersSO : ScriptableObject
{
    public List<StaticShopEncounter> shopEncounters;
}

[Serializable]
public class StaticShopEncounter
{
    public List<CardType> cardTypes;
    public List<CompanionTypeSO> companionTypes;
}
