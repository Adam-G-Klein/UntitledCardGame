using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EncounterManager : Manager 
{
    
    private Encounter activeEncounter;

    void Start() {}

    // Update is called once per frame
    void Update() {}
    public void setActiveEncounter(Encounter encounter){
        activeEncounter = encounter;
    }

    public Encounter getActiveEncounter(){
        return activeEncounter;
    }

    // singleton boiler plate
    private static EncounterManager instance;
    void Awake()
    {
        // If the instance reference has not been set yet, 
        if (instance == null)
        {
            // Set this instance as the instance reference.
            instance = this;
        }
        else if(instance != this)
        {
            // If the instance reference has already been set, and this is not the
            // the instance reference, destroy this game object.
            Destroy(gameObject);
        }

        // Do not destroy this object when we load a new scene
        DontDestroyOnLoad(gameObject);
    }   
}

