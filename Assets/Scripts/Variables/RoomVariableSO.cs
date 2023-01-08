using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomReference : Reference<Room, RoomVariableSO> {
    public RoomReference(Room Value) : base(Value) {}
    public RoomReference() { }
}

[CreateAssetMenu(
    fileName = "RoomVariable",
    menuName = "Rooms/Room Variable")]
public class RoomVariableSO : VariableSO<Room> { }
