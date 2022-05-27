using System.Collections;
using System.Collections.Generic;
using GameEventBus;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private EventBus eventBus = new EventBus();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void endPlayerTurn() {
        eventBus.Publish<EndPlayerTurnEvent>(new EndPlayerTurnEvent());
    }

    public void startPlayerTurn() {
        eventBus.Publish<StartPlayerTurnEvent>(new StartPlayerTurnEvent());
    }

    public EventBus getEventBus() {
        return eventBus;
    }

     //boiler plate singleton code
    private static BattleManager instance;
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