using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor;
using System.Xml.Serialization;
using System.Xml;

public static class ObjectRepository
{
    public static Dictionary<string, Room> roomsById = new Dictionary<string, Room>();
    public static Dictionary<string, Encounter> encountersById = new Dictionary<string, Encounter>();
    public static Dictionary<string, Enemy> enemiesById = new Dictionary<string, Enemy>();
    public static bool initialized = false;
    public const string PATH_TO_RESOURCES = "Assets/Resources/";
    public const string ROOMS_FILEPATH  = PATH_TO_RESOURCES + "Maps/Rooms/";
    // The path down the xml hierarchy within room files to the individual elements
    public const string ROOMFILES_TAG_PATH  = "Rooms/Room";
    public const string ENCOUNTERS_PATH = PATH_TO_RESOURCES + "Maps/Encounters/";
    public const string ENCOUNTERFILES_TAG_PATH = "Encounters/Encounter";
    public const string ENEMIES_PATH = PATH_TO_RESOURCES + "Maps/Enemies/";
    public const string ENEMYFILES_TAG_PATH = "Enemies/Enemy";

    public static void initializeForMap(XmlDocument mapXml){
        // populate all of the maps with the map objects in the Resources folder
        roomsById = XmlParser<Room>.getMapObjectsFromDirectory(ROOMS_FILEPATH, ROOMFILES_TAG_PATH);
        // get any rooms defined in the map
        Dictionary<string,Room> mapRoomsById = XmlParser<Room>.getMapObjectsFromXmlDoc(mapXml, "Map/Room");
        //add the map rooms to the roomsById dictionary
        foreach(KeyValuePair<string,Room> roomPair in mapRoomsById){
            roomsById.Add(roomPair.Key, roomPair.Value);
        }
        setMapRoomIds(roomsById);
        printDict(roomsById);
        XmlDoorConnector.connectRoomsInXmlDocument(mapXml, roomsById);
        XmlDoorConnector.connectRoomsInDirectory(ROOMS_FILEPATH, roomsById);
        initialized = true;
    }

    private static Dictionary<string, MapObject> setIds(){
        Dictionary<string, MapObject> mo = new Dictionary<string, MapObject>();
        foreach(KeyValuePair<string, Room> kvp in roomsById){
            if(kvp.Value == null) {
                Debug.LogError("No room object found corresponding to id " + kvp.Key);
            }
            kvp.Value.setId(kvp.Key);
        }
        foreach(KeyValuePair<string, Encounter> kvp in encountersById){
            if(kvp.Value == null) {
                Debug.LogError("No encounter object found corresponding to id " + kvp.Key);
            }
            kvp.Value.setId(kvp.Key);
        }
        foreach(KeyValuePair<string, Enemy> kvp in enemiesById){
            if(kvp.Value == null) {
                Debug.LogError("No enemy object found corresponding to id " + kvp.Key);
            }
            kvp.Value.setId(kvp.Key);
        }
        return mo;
    }
    private static void setMapRoomIds(Dictionary<string,Room> roomsById){
        // tell the room objects what their id was declared as in the xml doc
        foreach(KeyValuePair<string,Room> kvp in roomsById){
            if(kvp.Value == null) {
                Debug.LogError("No room object found corresponding to id " + kvp.Key);
            }
            kvp.Value.setId(kvp.Key);
        }
    }
    
    static void printDict(Dictionary<string,Room>  dict){
        Debug.Log("roomsById: ");
        foreach(KeyValuePair<string, Room> kvp in dict){
            Debug.Log("\troom id: " + kvp.Key + " has value " + kvp.Value);
        }
    }

    public static Room getRoom(string key){
        if(!initialized) { throwNotInitialized(); }
        return roomsById[key];
    }

    public static Dictionary<string, Room> getRoomsById(){
        if(!initialized) { throwNotInitialized(); }
        return roomsById;
    }

    public static void throwNotInitialized(){
        Debug.LogError("ObjectRepository not initialized, "+
                "call ObjectRepository.initialize before attempting"+
                "to access anything from it");
    }
   
}