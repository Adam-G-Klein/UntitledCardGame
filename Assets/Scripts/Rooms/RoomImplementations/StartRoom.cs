using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartRoom : DefaultRoom
{
    //maybe this room doesn't have any encounters in it
    // either way, separating it out here because I feel it'll be
    // necessary
    public override void build(){
        StartRoomFactory roomFactory = new StartRoomFactory();
        roomFactory.generateRoom(this);
    }
}
