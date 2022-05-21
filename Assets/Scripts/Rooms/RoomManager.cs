using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : Manager 
{
    
    public Room activeRoom;

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {}

    public void setActiveRoom(Room room){
        this.activeRoom = room;
        print("Set active room to: " + room.getSceneString());
    }
    public Room getActiveRoom(){
        return activeRoom;
    }

    public void loadActiveRoom(){
        if(activeRoom == null) {
            Debug.LogError("Trying to load the active room... but it's not set in the RoomManager");
        }
        loadScene(activeRoom);
    }

    //boiler plate singleton code
    private static RoomManager instance;
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


