using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "RoomVariable",
    menuName = "Rooms/Room Variable")]
public class RoomVariableSO : ScriptableObject
{
    public Room room;
}
