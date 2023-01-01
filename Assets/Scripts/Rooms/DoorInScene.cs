using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DoorInScene : MonoBehaviour
{
    public Door door;
    public StringGameEvent roomChangeEvent;

    void OnCollisionEnter2D(Collision2D col) {
        roomChangeEvent.Raise(door.connectedRoomId);
    }
}
