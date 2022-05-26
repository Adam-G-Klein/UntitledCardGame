using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Room : GameScene
{
    string getId();
    void setId(string id);
    RoomType getRoomType();
    List<Encounter> getEncounters();

    List<Room> getConnectedRooms();

    //for use in worldgen
    void setEncounters(List<Encounter> encounters);
    //for use in worldgen
    void setConnectedRooms(List<Room> connectedRooms);
}
