using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionFactory
{
    public void generateCompanion(
            Companion companion,
            GameObject prefab,
            Vector2 location) {
        GameObject companionGameObject;

        companionGameObject = Object.Instantiate(
            prefab,
            location, 
            Quaternion.identity) as GameObject;
        companionGameObject
            .GetComponent<CompanionInScene>()
            .setCompanion(companion);
    }
}
