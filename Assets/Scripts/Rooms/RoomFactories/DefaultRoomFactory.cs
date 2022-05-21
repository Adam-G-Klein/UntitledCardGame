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
        GameObject encounterInRoomPrefab = GameObject.Find("PrefabStore").GetComponent<PrefabStore>().getPrefabByName("EncounterInRoom");
        GameObject obj;
        EncounterInRoom encounterInRoom;
        foreach(Encounter encounter in room.getEncounters())
        {
            obj = Object.Instantiate(encounterInRoomPrefab, encounter.getLocationInRoom(), Quaternion.identity);
            encounterInRoom = obj.GetComponent<EncounterInRoom>();
            encounterInRoom.setEncounter(encounter);
        }
    }

    private void generatePlayer(){
        PlayerInRoomFactory playerInRoomFactory = new PlayerInRoomFactory();
        playerInRoomFactory.generatePlayer();
    }
}
