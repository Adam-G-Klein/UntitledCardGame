using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Room : GameScene, MapObject
{
    RoomType getRoomType();
    List<Encounter> getEncounters();

    //for use in worldgen
    void setEncounters(List<Encounter> encounters);
    //for use in worldgen
    void addOutgoingDoor(Door door, string roomId);
    void addIncomingDoor(Door door); //doorId = RoomId.DoorName
    Dictionary<string, Door> getOutgoingDoorsMapGen(); 
    List<Door> getOutgoingDoors(); 
    List<Door> getIncomingDoors(); 
    void cleanupRoom(); //to be called when leaving a room
}
