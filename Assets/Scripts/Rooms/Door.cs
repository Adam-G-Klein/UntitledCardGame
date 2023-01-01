using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Door
{
    public string id = Id.newGuid();
    public string connectedRoomId;
}
