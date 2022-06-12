using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DefaultRoomFactory
{

    public DefaultRoomFactory(){}
    public void generateRoom(DefaultRoom room)
    {

        RoomManager manager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();
        manager.setActiveRoom(room);
        
        generateEncounters(room);
        generateDoors(room, manager);
        generatePlayer();
        setRoomText(room);
    }

    private void setRoomText(DefaultRoom room){
        GameObject.Find("RoomId").GetComponent<TextMeshProUGUI>().SetText(room.getId());
    }

    private void generatePlayer(){
        PlayerInRoomFactory playerInRoomFactory = new PlayerInRoomFactory();
        playerInRoomFactory.generatePlayer();
    }

    private void generateEncounters(DefaultRoom room){
        GameObject encounterInRoomPrefab = GameObject.Find("PrefabStore").GetComponent<PrefabStore>().getPrefabByName("EncounterInRoom");
        GameObject obj;
        EncounterInRoom encounterInRoom;
        LocationStore encounterStore = GameObject.FindGameObjectWithTag("EncounterStore").GetComponent<LocationStore>();
        List<Encounter> encounters = room.getEncounters();
        if(encounters.Count > encounterStore.getTopLevelCount()){
            Debug.LogError("Not enough encounter locations in EncounterStore for the amount of encounters in scene: " + room.getSceneString());
        }
        for(int i = 0; i < encounters.Count; i += 1)
        {
            //still weighing whether we should have a factory method to call here...
            obj = Object.Instantiate(encounterInRoomPrefab, encounterStore.getLoc(i), Quaternion.identity);
            encounterInRoom = obj.GetComponent<EncounterInRoom>();
            encounterInRoom.setEncounter(encounters[i]);
        }
    }

    private void generateDoors(DefaultRoom room, RoomManager manager){
        GameObject[] doorGameObjects = GameObject.FindGameObjectsWithTag("Door");
        DoorInRoom doorScript;
        foreach(Door door in room.getOutgoingDoors()){
            foreach(GameObject go in doorGameObjects){
                if(door.getDoorName().Equals(go.name)){
                    doorScript = go.GetComponent<DoorInRoom>();
                    doorScript.door = door;
                }
            }
            door.setManager(manager); //won't be done by another room if we're in the start room
        }
        // will be doors in the room that aren't in the door list because they don't have anything connected
        foreach(GameObject go in doorGameObjects){
            go.GetComponent<DoorInRoom>().initColor();
        }
        
    }

}
