using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Room
{
    string getRoomSceneString();
    RoomType getRoomType();
    List<Encounter> getEncounters();
    void buildRoom();
}
