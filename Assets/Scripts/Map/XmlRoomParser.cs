using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor;
using System.Xml.Serialization;
using System.Xml;

public class XmlRoomParser 
{
    public XmlRoomParser(){}

    public Dictionary<string,Room> getRoomsById(XmlDocument xmlDoc){
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
            Manager.Quit();
        }
        Debug.Log("found type room from id string: " + newRoom);
        return newRoom;
    }
   
}