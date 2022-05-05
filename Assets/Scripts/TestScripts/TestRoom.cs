using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRoom : DefaultRoom 
{
    public override void buildRoom()
    {
        TestRoomFactory roomFactory = new TestRoomFactory();
        roomFactory.generateRoom(this);
    }
}
