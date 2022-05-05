using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultRoomFactory
{
    public void generateRoom(DefaultRoom room)
    {
        // This is all just random test things room generation
        // to test the room generation process
        GameObject prefab = GameObject.Find("TestHoldPrefab").GetComponent<TestHoldThings>().getTestSprite();
        foreach(Vector2 entry in room.getEncounters())
        {
            Object.Instantiate(prefab, entry, Quaternion.identity);
        }
    }
}
