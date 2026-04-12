using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "StaticShopEncounter",
    menuName = "Encounters/Shop/New Static Shop Encounter")]
public class StaticShopEncounterSO : ScriptableObject
{
    [Header("Static pools of cards and rats to pull from")]
    public StaticShopPoolEncounter shopPoolEncounter;
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
