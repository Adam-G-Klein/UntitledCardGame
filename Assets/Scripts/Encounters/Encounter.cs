using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EncounterType {
    Enemy,
    Shop
}

public abstract class Encounter
{
    public string id = Id.newGuid();
    
    public abstract void build();
}
