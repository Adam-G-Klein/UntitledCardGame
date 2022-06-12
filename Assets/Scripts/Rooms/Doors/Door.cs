using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Door 
{
    //get the door this door is connected to in another room
    Door getConnectedDoor();
    //set the door this door is connected to in another room
    void setConnectedDoor(Door door);
    void loadConnectedRoom();
    void loadRoom(RoomManager manager);
    Room getRoom();
    void setDoorId(string roomId, string doorName);
    void buildDoorInScene();
    string getPrefabName();
    string getDoorName();
    string getDoorId();
    bool getEnteredThrough();
    void setEnteredThrough(bool enteredThrough);
    void setManager(RoomManager manager);

}
