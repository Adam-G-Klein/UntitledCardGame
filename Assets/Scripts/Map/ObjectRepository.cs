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
    public static Dictionary<string,Room> roomsById = new Dictionary<string, Room>();
    public static Dictionary<string, Encounter> encountersById = new Dictionary<string, Encounter>();
    public static Dictionary<string, Enemy> enemiesById = new Dictionary<string, Enemy>();
    public static bool initialized = false;

    public static void initialize(XmlDocument xmlDoc){
        roomsById = XmlRoomParser.getRoomsById(xmlDoc);
        setRoomIds(roomsById);
        printDict(roomsById);

        initialized = true;
    }
    private static void setRoomIds(Dictionary<string,Room> roomsById){
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