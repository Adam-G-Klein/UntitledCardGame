using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ShopEncounter", 
    menuName = "Encounters/Encounter Type/Shop Encounter")]
public class ShopEncounter : EncounterTypeSO
{
    public List<GameObject> items;

    public override void Build()
    {
        return;
    }
}
