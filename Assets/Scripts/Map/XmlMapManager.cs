using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

public class XmlMapManager : Manager, MapManager
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
    private string mapFileName = "Maps/testMap";
    private string startRoomId = "StartRoom";
    private DeserializedMap deserializedMap;
    public Dictionary<string,Room> roomsById = new Dictionary<string, Room>();

    void Start(){
        roomManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();
        generateMap();
    }

    public void generateMap(){
        XmlDocument xmlDoc = new XmlDocument();
        TextAsset f = Resources.Load<TextAsset>(mapFileName);
        print("textasset null? " + (f  == null).ToString());
        xmlDoc.LoadXml(f.text);
        roomsById = getRoomsById(xmlDoc);
        setRoomIds(roomsById);
        printDict(roomsById);
        connectRooms(xmlDoc, roomsById);
        Room startRoom = roomsById[startRoomId];
        roomManager.loadScene(startRoom);


        /*
        leftRoom.setConnectedRooms(new List<Room>(){startRoom});
        rightRoom.setConnectedRooms(new List<Room>(){startRoom});
        startRoom.setConnectedRooms(new List<Room>(){
            leftRoom, rightRoom
        });
        //generate the map and then
        roomManager.loadScene(startRoom);
        */

    }
    private void setRoomIds(Dictionary<string,Room> roomsById){
        foreach(KeyValuePair<string,Room> kvp in roomsById){
            kvp.Value.setId(kvp.Key);
        }
    }
    private void connectRooms(XmlDocument xmlDoc, Dictionary<string, Room> roomsById){
        string id;
        List<Room> connectedRooms;
        Room room;
        foreach(XmlElement node in xmlDoc.SelectNodes("Rooms/Room"))
        {
            id = node.GetAttribute("id");
            connectedRooms = getConnectedRooms(roomsById, node);
            room = roomsById[id];
            room.setConnectedRooms(connectedRooms);
        }

    }

    private Dictionary<string,Room> getRoomsById(XmlDocument xmlDoc){
        string id;
        Room room;
        Dictionary<string,Room> newRoomsById = new Dictionary<string, Room>();
        foreach(XmlElement node in xmlDoc.SelectNodes("Rooms/Room"))
        {
            id = node.GetAttribute("id");
            room = getRoomWithExceptions(node);
            newRoomsById.Add(id, room);
        }
        return newRoomsById;
    }
    private Room getRoomWithExceptions(XmlElement roomElement){
        Room newRoom;
        try {
            Type roomType = Type.GetType(roomElement.GetAttribute("type"));
            newRoom = (Room) System.Activator.CreateInstance(roomType);
        } catch {
            Debug.LogError("Couldn't convert room type " 
                + roomElement.GetAttribute("type") 
                + " to a valid room implementation during map generation. "
                + "Use one of the classnames in Assets/Rooms/RoomImplementations for the type field. ");
            newRoom = null;
            Quit();
        }
        print("found type room from id string: " + newRoom);
        return newRoom;
    }

    private List<Room> getConnectedRooms(Dictionary<string,Room> roomsById, XmlNode roomsNode){
        List<Room> connectedRooms = new List<Room>();
        string id;
        foreach(XmlElement node in roomsNode.SelectNodes("ConnectedRooms/Room"))
        {
            id = node.GetAttribute("id");
            connectedRooms.Add(roomsById[id]);
        }
        return connectedRooms;

    }
    void printDict(Dictionary<string,Room>  dict){
        print("roomsById: ");
        foreach(KeyValuePair<string, Room> kvp in dict){
            print("\troom id: " + kvp.Key + " has value " + kvp.Value);
        }
    }
    
}