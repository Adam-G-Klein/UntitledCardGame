using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultRoomFactory : RoomFactory
{
    public void generateRoom(Room room)
    {
        DefaultRoom newRoom = room as DefaultRoom;

        // This is all just random test things room generation
        // to test the room generation process
        GameObject prefab = GameObject.Find("TestHoldPrefab").GetComponent<TestHoldThings>().getTestSprite();
        foreach(KeyValuePair<string, Vector2> entry in newRoom.getEncounterPoints())
        {
            Object.Instantiate(prefab, entry.Value, Quaternion.identity);
        }
    }
}
