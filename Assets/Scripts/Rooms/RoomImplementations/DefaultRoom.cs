using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultRoom : Room
{
    private List<Encounter> encounters = new List<Encounter>()
    {
        // these will be encounters once they're created
        new DefaultEncounter(new Vector2(-7.5f, 4.2f)),
        new DefaultEncounter(new Vector2(2.3f, 4.5f)),
        new DefaultEncounter(new Vector2(8.6f, 0.35f))
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

    public List<Encounter> getEncounters()
    {
        return encounters;
    }

    public virtual void buildRoom()
    {
        DefaultRoomFactory roomFactory = new DefaultRoomFactory();
        roomFactory.generateRoom(this);
    }
}
