using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;

public class DefaultDoor : Door 
{
    public DefaultDoor(Room inRoom){
        this.room = inRoom;
    }
    private RoomManager manager;
    private string prefabName = "DoorInRoom";
    private string roomId;
    private string doorName;
    private Door connectedDoor = null;
    private Room room = null;
    public bool enteredThrough = false;
    public Door getConnectedDoor(){
        return connectedDoor;
    }

    public void setConnectedDoor(Door door){
        this.connectedDoor = door;
    }

    public void loadConnectedRoom(){
        if(connectedDoor == null){
            Debug.LogError("Cannot load connected room from door " + getDoorId() + ", no connected room");
            return;
        }
        room.cleanupRoom();
        connectedDoor.loadRoom(manager);
    }

    public void loadRoom(RoomManager manager){
        if(room == null){
            Debug.LogError("Cannot load room that door " + getDoorId() + " is inside of, room object is null");
        }
        if(manager == null){
            Debug.LogError("Cannot load room that door " + getDoorId() + " is inside of, manager object is null");
        }
        manager.loadScene(room);
        enteredThrough = true;
    }

    public Room getRoom(){
        return room;
    }

    public void setRoom(Room room){
        this.room = room;
    }

    public void buildDoorInScene(){
        DefaultDoorFactory defaultDoorFactory = new DefaultDoorFactory();
        defaultDoorFactory.generateDoor(this);
    }
    public string getPrefabName(){
        return prefabName;
    }

    public void setDoorId(string roomId, string doorName){
        this.roomId = roomId;
        this.doorName = doorName;
    }

    public string getDoorName(){
        return doorName;
    }
    
    public string getDoorId(){
        return roomId + "." + doorName;
    }

    public bool getEnteredThrough(){
        return enteredThrough;
    }
    public void setEnteredThrough(bool enteredThrough){
        this.enteredThrough = enteredThrough;
    }

    public void setManager(RoomManager manager){
        Debug.Log("manager set to " + manager + " on door " + getDoorId());
        this.manager = manager;
    }
}
