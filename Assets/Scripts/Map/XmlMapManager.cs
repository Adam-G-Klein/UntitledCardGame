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


    public Boolean loadDefaultMap = false;
    public static string MAP_BASE_TAG = "Map";
    public static string DEFAULT_MAP_NAME = "DefaultTestMap";
    private Room startRoom = new StartRoom();
    private Room leftRoom = new DefaultRoom();
    private Room rightRoom = new DefaultRoom();

    private RoomManager roomManager;
    private string PATH_TO_MAP = "Maps/";
    private string startRoomId = "StartRoom";
    private DeserializedMap deserializedMap;

    void Start(){
        if(loadDefaultMap){
            Room startRoom = generateMap(DEFAULT_MAP_NAME);
            roomManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();
            roomManager.loadScene(startRoom);
        }
    }

    public Room generateMap(string mapName){
        //does three passes through the xml document
        // pass 1: in ObjRepository, build all of the object maps
        // pass 2: in DoorConnector.connectRooms, get the doorNames 
        // pass 3: in DoorConnector.connectRooms, connect the doors
        // pass 2 and 3 could be reduced to a DFS if we want to optimize
        // see https://github.com/Adam-G-Klein/UntitledCardGame/commit/9d497eb425cfd400c95797d91e7ade4e2310c2e1#r77334961

        XmlDocument xmlDoc = new XmlDocument();
        TextAsset f = Resources.Load<TextAsset>(PATH_TO_MAP + mapName);
        if(!f) {
            throw new Exception("No map by name " + mapName + ", please enter another map name");
        }
        xmlDoc.LoadXml(f.text);
        ObjectRepository.initialize(xmlDoc);
        XmlDoorConnector.connectRooms(xmlDoc, ObjectRepository.getRoomsById());
        return ObjectRepository.getRoom(startRoomId);


    }

    
}