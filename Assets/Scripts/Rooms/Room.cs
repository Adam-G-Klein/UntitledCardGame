using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Room
{
    string getRoomSceneString();
    RoomType getRoomType();
    List<Vector2> getEncounters();
    void buildRoom();
}
