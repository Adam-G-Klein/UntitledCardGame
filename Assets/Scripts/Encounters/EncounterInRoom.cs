using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EncounterInRoom : MonoBehaviour
{
    // will be set by the room factory
    private Encounter encounter;
    private EncounterManager encounterManager;
    // Start is called before the first frame update
    void Start()
    {
        encounterManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<EncounterManager>();
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("encounter collision, going to " + encounter.getSceneString());
        encounterManager.loadScene(encounter);
    }

    public void setEncounter(Encounter encounter){
        this.encounter = encounter;
    }



    
}
