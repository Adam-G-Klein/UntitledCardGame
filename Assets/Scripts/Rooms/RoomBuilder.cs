using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBuilder
{
    public void buildRoom(Room room) 
    {
        RoomFactory roomFactory;

        switch(room.getRoomType())
        {
            case RoomType.DefaultRoom:
                roomFactory = new DefaultRoomFactory();
                roomFactory.generateRoom(room);
                break;
        }
    }
}
