using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartRoom : DefaultRoom
{
    //maybe this room doesn't have any encounters in it

    public override void buildRoom(){
        StartRoomFactory roomFactory = new StartRoomFactory();
        roomFactory.generateRoom(this);
    }
}
