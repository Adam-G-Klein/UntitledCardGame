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
    public bool isCompleted = false;

    public abstract void BuildWithEncounterBuilder(IEncounterBuilder encounterBuilder);

    protected EncounterType encounterType;

    public Location ToLocation(){
        switch (this.encounterType) {
            case EncounterType.Enemy:
                return Location.COMBAT;
            case EncounterType.Shop:
                return Location.SHOP;
            default:
                Debug.LogError("Encounter with id " + this.id + " has an invalid encounter type");
                return Location.NONE;
        }
    }

    public void setId(string id) {
        this.id = id;
    }

    public EncounterType getEncounterType() {
        return this.encounterType;
    }
}

[System.Serializable]
public abstract class EncounterSerializable {
    public string id;
    public bool isCompleted;
    public EncounterType encounterType;

    public EncounterSerializable(Encounter encounter)
    {
        this.id = encounter.id;
        this.isCompleted = encounter.isCompleted;
        this.encounterType = encounter.getEncounterType();
    }
}