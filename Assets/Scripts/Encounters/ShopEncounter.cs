using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShopEncounter : Encounter
{
    [Header("Shop")]
    public List<GameObject> items;

    public override void build()
    {
        return;
    }
}
