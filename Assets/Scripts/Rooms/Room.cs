using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Room : GameScene
{
    RoomType getRoomType();
    List<Encounter> getEncounters();

    List<Vector2> getDoorLocations();
    //for use in worldgen
    void setEncounters(List<Encounter> encounters);
    //for use in worldgen
    void setConnectedRooms(List<Room> connectedRooms);
}
