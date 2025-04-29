using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EncounterType {
    Enemy,
    Shop
}

[System.Serializable]
public abstract class Encounter : ISerializableGameState<EncounterSerializable>
{
    public string id = Id.newGuid();
    public bool isCompleted = false;

    public abstract void BuildWithEncounterBuilder(IEncounterBuilder encounterBuilder);

    public EncounterType encounterType;

    // TEMPORARY: removed once we can serialize the base encounter types and 
    // all of their subsidiary data.
    // Then call the constructor for those types and serialize / deserialize there.
    // This will allow us to 1) store the encounter type from those serialized objects
    // and 2) ACTUALLY build the encounter into a playable state from the serialized data.
    // TODO: Serialize the enemy Encounter Type, requriing serialization of the enemy list, requiring serialization of the enemy type
    /*
    public Encounter(EncounterSerializable encounterSerializable) {
        this.id = encounterSerializable.id;
        this.isCompleted = encounterSerializable.isCompleted;
        this.encounterType = encounterSerializable.encounterType;
    }
    */

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

    public EncounterSerializable GetSerializableData()
    {
        return new EncounterSerializable(this);
    }

}

// TODO: Serialize the enemy Encounter Type, 
// requriing serialization of the enemy list, 
//requiring serialization of the enemy type

[System.Serializable]
public class EncounterSerializable
{
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


