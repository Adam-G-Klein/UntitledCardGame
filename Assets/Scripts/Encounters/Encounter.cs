using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EncounterType {
    Enemy,
    Shop
}

[System.Serializable]
public class Encounter
{
    public string id = Id.newGuid();
    public bool isCompleted = false;
    public virtual void build(List<Companion> companionList, EncounterConstantsSO constants) {
        return;
    }

    protected EncounterType encounterType;

    public void setId(string id) {
        this.id = id;
    }

    public EncounterType getEncounterType() {
        return this.encounterType;
    }
}
