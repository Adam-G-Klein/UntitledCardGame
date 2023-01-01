using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EncounterType {
    Enemy,
    Shop
}

[System.Serializable]
public abstract class Encounter
{
    public string id = Id.newGuid();
    public abstract void build(EncounterConstants constants);

    protected EncounterType encounterType;

    public void setId(string id) {
        this.id = id;
    }

    public EncounterType getEncounterType() {
        return this.encounterType;
    }
}
