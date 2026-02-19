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
    [Header("Static groups of cards and companions for each shop encounter. The shop rotates through the groups so we get to control the options")]
    public List<StaticShopPoolEncounter> shopPoolEncounters;
}

[Serializable]
public class StaticShopEncounter
{
    public List<CardType> cardTypes;
    public List<CompanionTypeSO> companionTypes;
}

[Serializable]
public class StaticShopPoolEncounter
{
    [SerializeField]
    public List<StaticCardTypeGroup> cardGroups;

    [SerializeField]
    public List<StaticCompanionTypeGroup> ratGroups;
}

[Serializable]
public class StaticCardTypeGroup
{
    public List<CardType> cardTypes;
}

[Serializable]
public class StaticCompanionTypeGroup
{
    public List<CompanionTypeSO> companionTypes;
}
