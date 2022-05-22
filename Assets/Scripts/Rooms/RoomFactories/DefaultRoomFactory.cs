using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultRoomFactory
{

    public DefaultRoomFactory(){}
    public void generateRoom(DefaultRoom room)
    {

        RoomManager manager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();
        manager.setActiveRoom(room);
        
        generateEncounters(room);
        generateDoors(room);
        generatePlayer();
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

    private void generateDoors(DefaultRoom room){
        GameObject doorInRoomPrefab = GameObject.Find("PrefabStore").GetComponent<PrefabStore>().getPrefabByName("DoorInRoom");
        GameObject obj;
        DoorInRoom doorInRoom;
        LocationStore encounterStore = GameObject.FindGameObjectWithTag("DoorStore").GetComponent<LocationStore>();
        List<Room> connectedRooms = room.getConnectedRooms();
        if(connectedRooms.Count > encounterStore.getTopLevelCount()){
            Debug.LogError("Not enough encounter locations in EncounterStore for the amount of encounters in scene: " + room.getSceneString());
        }
        for(int i = 0; i < connectedRooms.Count; i += 1)
        {
            //still weighing whether we should have a factory method to call here...
            obj = Object.Instantiate(doorInRoomPrefab, encounterStore.getLoc(i), Quaternion.identity);
            doorInRoom = obj.GetComponent<DoorInRoom>();
            doorInRoom.setConnectedRoom(connectedRooms[i]);
        }
    }
}
