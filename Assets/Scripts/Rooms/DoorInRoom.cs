using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DoorInRoom : MonoBehaviour
{
    // will be set by the room factory
    public Door door = null;
    private RoomManager roomManager;
    private SpriteRenderer spriteRenderer;
    public Color enteredThroughColor;
    public Color noConnectedDoorColor;
    // Start is called before the first frame update
    void Start()
    {
        roomManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void initColor(){
        if(door == null){
            spriteRenderer.color = noConnectedDoorColor;
        } else if (door.getEnteredThrough()){
            spriteRenderer.color = enteredThroughColor;
        }
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        if(door != null){ 
            Debug.Log("door collision, going to " + door.getConnectedDoor().getRoom().getId());
            door.loadConnectedRoom();
        }
    }

}
