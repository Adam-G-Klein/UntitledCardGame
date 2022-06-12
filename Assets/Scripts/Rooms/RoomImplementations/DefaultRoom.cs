using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;

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
    private List<string> doorNames = new List<string>(){
        "EastDoor",
        "WestDoor"
    };

    //map used for comparison in the map to match doors up, roomId needed so we know what room the door we're connecting to is in
    private Dictionary<string, Door> outgoingDoors = new Dictionary<string, Door>();
    private List<Door> incomingDoors = new List<Door>();
    
    public DefaultRoom(){
    }

    private string id = "unsetID!";

    private string roomSceneString = "Scenes/Rooms/DefaultRoom";
    private RoomType roomType = RoomType.DefaultRoom;
    private LocationStore doorStore;


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


    //dead code I think
    /*
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
    */

    public string getId(){
        return id;
    }

    public void setId(string id){
        this.id = id;
    }

    public void addOutgoingDoor(Door door, string roomId){
        //map used for comparison in the map to match doors up, roomId all that's needed
        outgoingDoors.Add(roomId,door);
    }
    public void addIncomingDoor(Door door){
        incomingDoors.Add(door);
    } 
    public Dictionary<string, Door> getOutgoingDoorsMapGen(){
        return outgoingDoors;
    }

    public List<Door> getOutgoingDoors(){
        return outgoingDoors.Values.ToList<Door>();
    }
    public List<Door> getIncomingDoors(){
        return incomingDoors;
    }

    public void cleanupRoom(){
        foreach(Door door in getOutgoingDoors()){
            door.setEnteredThrough(false);
        }
    }
}

