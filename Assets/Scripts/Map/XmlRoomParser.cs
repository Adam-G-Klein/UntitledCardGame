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
    public const string IMPORTED_KEYWORD = "IMPORTED";
    public const string ROOMS_FILE_PREFIX = "Maps/Rooms/";
    public XmlRoomParser(){}

    public static Dictionary<string,Room> getRoomsById(XmlDocument xmlDoc){
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

    public static Room getRoomWithExceptions(XmlElement roomElement){
        Room newRoom = null;
        string id = roomElement.GetAttribute("id");
        if(roomElement.GetAttribute("type").Trim().Equals(IMPORTED_KEYWORD)){
            roomElement = getRoomElementFromFileName(id);
        }
        newRoom = getRoomFromElement(roomElement);
        return newRoom;
    }

    public static XmlElement getRoomElementFromFileName(string filename){
        XmlDocument xmlDoc = new XmlDocument();
        TextAsset f = Resources.Load<TextAsset>(ROOMS_FILE_PREFIX + filename);
        xmlDoc.LoadXml(f.text);
        XmlElement roomElement = xmlDoc.DocumentElement;
        Debug.Log("Found element with id " + roomElement.GetAttribute("id") + " from file");
        return roomElement;
    }

    private static Room getRoomFromElement(XmlElement roomElement){
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
        return newRoom;
    }
   
}