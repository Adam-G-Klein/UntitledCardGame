using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    //taking a first crack at this here, comments will be verbose
    //extending monobehavior because we'll kick off world generation in the start
    //method if it hasn't happened yet.

    //I really wanna parse this Dictionary from a YAML file, I think it'll be really nice
    // for our test setup

    // for now, we do this the hard way
    
    // this script will be the only thing in the loading scene before the start room
    // will finish by calling the startRoom's factory method


    private Room startRoom = new StartRoom();
    private Room leftRoom = new DefaultRoom();
    private Room rightRoom = new DefaultRoom();

    private RoomManager roomManager;

    void Start(){
        roomManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();
        generateMap();
    }

    void generateMap(){

        leftRoom.setConnectedRooms(new List<Room>(){startRoom});
        rightRoom.setConnectedRooms(new List<Room>(){startRoom});
        startRoom.setConnectedRooms(new List<Room>(){
            leftRoom, rightRoom
        });
        //generate the map and then
        roomManager.loadScene(startRoom);

    }
    
}