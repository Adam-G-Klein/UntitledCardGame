using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultRoom : Room
{
    //test data set, overidden at worldGen
    private List<Encounter> encounters = new List<Encounter>()
    {
        // these will be encounters once they're created
        new DefaultEncounter(new Vector2(-7.5f, 4.2f)),
        new DefaultEncounter(new Vector2(2.3f, 4.5f)),
        new DefaultEncounter(new Vector2(8.6f, 0.35f))
    };

    //set at worldgen
    private List<Room> connectedRooms; 

    private string roomSceneString = "Scenes/Rooms/DefaultRoom";
    private RoomType roomType = RoomType.DefaultRoom;
    private LocationStore doorStore;

    // Start is called before the first frame update
    void Start() {
        doorStore = GameObject.FindGameObjectWithTag("DoorStore").GetComponent<LocationStore>();
    }

    // Update is called once per frame
    void Update() {}

    public virtual string getSceneString()
    {
        return roomSceneString;
    }

    public RoomType getRoomType()
    {
        return roomType;
    }

    public List<Encounter> getEncounters()
    {
        return encounters;
    }

    public void setEncounters(List<Encounter> encounters){
        this.encounters = encounters;
    }

    public virtual void build()
    {
        DefaultRoomFactory roomFactory = new DefaultRoomFactory();
        roomFactory.generateRoom(this);
    }

    public void setConnectedRooms(List<Room> connectedRooms){
        this.connectedRooms= connectedRooms;
    }

    public List<Vector2> getDoorLocations(){
        if(connectedRooms.Count > doorStore.getTopLevelCount()) {
            Debug.LogError("Not enough doors for the amount of connected rooms in room: " 
                + getSceneString() + " of type: " + getRoomType().ToString());
        }
        List<Vector2> retList = new List<Vector2>();
        for(int i = 0; i < connectedRooms.Count; i += 1) {
            retList.Add(doorStore.getLoc(i));
        }
        return retList;
    }
}
