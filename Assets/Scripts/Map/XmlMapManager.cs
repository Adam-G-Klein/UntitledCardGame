using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor;
using System.Xml.Serialization;
using System.Xml;

public class XmlMapManager : Manager, MapManager
{
    // this script will be the only thing in the loading scene before the start room
    // will finish by calling the startRoom's factory method


    private Room startRoom = new StartRoom();
    private Room leftRoom = new DefaultRoom();
    private Room rightRoom = new DefaultRoom();

    private RoomManager roomManager;
    private string mapFileName = "Maps/testMap";
    private string startRoomId = "StartRoom";
    private DeserializedMap deserializedMap;

    void Start(){
        roomManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();
        generateMap();
    }

    public void generateMap(){
        //does three passes through the xml document
        // pass 1: in ObjRepository, build all of the object maps
        // pass 2: in DoorConnector.connectRooms, get the doorNames 
        // pass 3: in DoorConnector.connectRooms, connect the doors
        // pass 2 and 3 could be reduced to a DFS if we want to optimize
        // see https://github.com/Adam-G-Klein/UntitledCardGame/commit/9d497eb425cfd400c95797d91e7ade4e2310c2e1#r77334961

        XmlDocument xmlDoc = new XmlDocument();
        TextAsset f = Resources.Load<TextAsset>(mapFileName);
        xmlDoc.LoadXml(f.text);
        //Create a map of fresh Room objects, associate with their ids
        ObjectRepository.initialize(xmlDoc);
        //set the id field on all the room objects
        XmlDoorConnector.connectRooms(xmlDoc, ObjectRepository.getRoomsById());
        Room startRoom = ObjectRepository.getRoom(startRoomId);
        roomManager.loadScene(startRoom);


    }

    
}