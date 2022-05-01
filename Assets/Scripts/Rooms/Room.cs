using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Room
{
    public string getRoomSceneString();
    public RoomType getRoomType();
    public Dictionary<string, Vector2> getEncounterPoints();
}
