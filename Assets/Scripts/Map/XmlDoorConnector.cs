using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor;
using System.Xml.Serialization;
using System.Xml;

public class XmlDoorConnector
{
    public XmlDoorConnector(){}

    public void connectRooms(XmlDocument xmlDoc, Dictionary<string, Room> roomsById){
        string id;
        foreach(XmlElement node in xmlDoc.SelectNodes("Rooms/Room"))
        {
            id = node.GetAttribute("id");
            addDoors(roomsById, node, id);
        }
        foreach(XmlElement node in xmlDoc.SelectNodes("Rooms/Room"))
        {
            id = node.GetAttribute("id");
            connectDoors(roomsById, node, id);
        }
    }

    private void connectDoors(Dictionary<string,Room> roomsById, XmlNode roomsNode, string thisRoomId){
        Room thisRoom = roomsById[thisRoomId];
        Room connectedRoom;
        foreach(KeyValuePair<string, Door> outgoingDoor in thisRoom.getOutgoingDoorsMapGen()){
            connectedRoom = roomsById[outgoingDoor.Key];
            foreach(KeyValuePair<string,Door> incomingDoor in connectedRoom.getOutgoingDoorsMapGen()){
                if(incomingDoor.Key.Equals(thisRoomId)){
                    incomingDoor.Value.setConnectedDoor(outgoingDoor.Value);
                    outgoingDoor.Value.setConnectedDoor(incomingDoor.Value);
                    Debug.Log("connected outgoing door " + outgoingDoor.Value.getDoorId() + " to incoming door " + incomingDoor.Value.getDoorId());
                }
            }
            if(outgoingDoor.Value.getConnectedDoor() == null) {
                Debug.LogError("No door on the other side of door " + outgoingDoor.Value.getDoorId());
            } 
        }
    }

    private void addDoors(Dictionary<string,Room> roomsById, XmlNode roomsNode, string thisRoomId){
        string connectedRoomId;
        Room connectedRoom;
        Room thisRoom = roomsById[thisRoomId];
        Door thisRoomDoor;
        foreach(XmlElement node in roomsNode.SelectNodes("ConnectedRooms/Room"))
        {
            connectedRoomId = node.GetAttribute("id");
            connectedRoom = roomsById[connectedRoomId];

            thisRoomDoor = new DefaultDoor(thisRoom);
            thisRoomDoor.setDoorId(thisRoomId, node.GetAttribute("door"));

            Debug.Log("added outgoing door: " + thisRoomDoor.getDoorId());
            thisRoom.addOutgoingDoor(thisRoomDoor, connectedRoomId);
            connectedRoom.addIncomingDoor(thisRoomDoor);
        }
    }   
   
}