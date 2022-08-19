using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionManager : Manager
{
    private List<Companion> activeCompanions = new List<Companion>();

    // Start is called before the first frame update
    void Start()
    {
        // Temporarily give the player the best companion, ditto with a gun
        activeCompanions.Add(new Companion());
    }

    public List<Companion> getActiveCompanions() {
        return activeCompanions;
    }

    //boiler plate singleton code
    private static CompanionManager instance;
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
