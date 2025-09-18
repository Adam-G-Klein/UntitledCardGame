using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

[System.Serializable]
public class Map
{
    [SerializeReference]
    public List<Encounter> encounters;

    public Map(List<Encounter> encounters) {
        this.encounters = encounters;
    }

    public Map(List<EncounterSerializable> encounters, SORegistry registry, ShopDataSO shopData) {
        this.encounters = encounters.Select<EncounterSerializable, Encounter>(encounter => encounter.encounterType == EncounterType.Enemy ?
            new EnemyEncounter(encounter as EnemyEncounterSerializable, registry) : new ShopEncounter(encounter as ShopEncounterSerializable, registry, shopData)).ToList();
    }
}