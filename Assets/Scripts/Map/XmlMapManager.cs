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
    public Dictionary<string,Room> roomsById = new Dictionary<string, Room>();

    void Start(){
        roomManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();
        generateMap();
    }

    public void generateMap(){
        //does three passes through the xml document
        // pass 1: in getRoomsById, just grab the roomIds and associate them with new room objects
        // pass 2: in connectRooms, get the doorNames 

        XmlDocument xmlDoc = new XmlDocument();
        XmlRoomParser roomParser = new XmlRoomParser();
        XmlDoorConnector doorConnector = new XmlDoorConnector();
        TextAsset f = Resources.Load<TextAsset>(mapFileName);
        xmlDoc.LoadXml(f.text);
        //Create a map of fresh Room objects, associate with their ids
        roomsById = roomParser.getRoomsById(xmlDoc);
        //set the id field on all the room objects
        setRoomIds(roomsById);
        printDict(roomsById);
        doorConnector.connectRooms(xmlDoc, roomsById);
        Room startRoom = roomsById[startRoomId];
        roomManager.loadScene(startRoom);


    }
    private void setRoomIds(Dictionary<string,Room> roomsById){
        foreach(KeyValuePair<string,Room> kvp in roomsById){
            kvp.Value.setId(kvp.Key);
        }
    }
    
    void printDict(Dictionary<string,Room>  dict){
        print("roomsById: ");
        foreach(KeyValuePair<string, Room> kvp in dict){
            print("\troom id: " + kvp.Key + " has value " + kvp.Value);
        }
    }
    
}