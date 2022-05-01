using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultRoom : Room
{
    private Dictionary<string, string> encounterDict = new Dictionary<string, string>()
    {
        {"1", "First Encounter Here"},
        {"2", "Second Encounter Here"},
        {"3", "Third Encounter Here"}
    };

    private Dictionary<string, Vector2> encounterPoints = new Dictionary<string, Vector2>()
    {
        {"1", new Vector2(-7.5f, 4.2f)},
        {"2", new Vector2(2.3f, 4.5f)},
        {"3", new Vector2(8.6f, 0.35f)}
    };

    private string roomSceneString = "Scenes/Rooms/DefaultRoom";
    private RoomType roomType = RoomType.DefaultRoom;

    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update() {}

    public string getRoomSceneString()
    {
        return roomSceneString;
    }

    public RoomType getRoomType()
    {
        return roomType;
    }

    public Dictionary<string, Vector2> getEncounterPoints()
    {
        return encounterPoints;
    }
}
