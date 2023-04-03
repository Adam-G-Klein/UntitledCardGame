using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Room
{
    // public string id = Id.newGuid();
    // public List<EncounterReference> encounters = new List<EncounterReference>();
    // public List<Door> doors = new List<Door>();

    // private RoomConstants roomConstants;

    // public void build(RoomConstants roomConstants) {
    //     this.roomConstants = roomConstants;
    //     setupDoors();
    //     setupEncounters();
    // }

    // private void setupDoors() {
    //     // So for now, each room is actually a hallway
    //     // There will only be two doors (you can add more but they won't do anything)
    //     // The first door is the left door. If the first door's id is an empty string then we're pretending it isn't there
    //     // The second door is the right door. If there's only one door, then don't create a right door
    //     int i = 0;
    //     foreach (Door door in doors) {
    //         if (door.id != "") {
    //             Vector3 location = Vector3.zero;
    //             if (i == 0) {
    //                 location = roomConstants.FIRST_DOOR_LOCATION;
    //             } else {
    //                 location = roomConstants.SECOND_DOOR_LOCATION;
    //             }

    //             GameObject newDoor = GameObject.Instantiate(
    //                 roomConstants.doorPrefab, 
    //                 location,
    //                 Quaternion.identity);
    //             DoorInScene doorInScene = newDoor.GetComponent<DoorInScene>();
    //             doorInScene.door = door;
    //         }
    //         i++;
    //     }
    // }

    // private void setupEncounters() {
    //     foreach(EncounterReference encounterReference in encounters) {
    //         Encounter encounter = encounterReference.Value;
    //         switch(encounter.getEncounterType()) {
    //             case EncounterType.Enemy:
    //                 if (encounter.isCompleted) {
    //                     continue;
    //                 }
    //                 GameObject newEnemyEncounter = GameObject.Instantiate(
    //                     roomConstants.encounterPrefab, 
    //                     roomConstants.ENCOUNTER_LOCATION,
    //                     Quaternion.identity);
    //                 newEnemyEncounter.GetComponent<EncounterInScene>().encounterId = encounter.id;
    //             break;

    //             case EncounterType.Shop:
    //                 GameObject prefab = encounter.isCompleted ? roomConstants.closedShopPrefab : roomConstants.shopPrefab;
    //                 GameObject newShop = GameObject.Instantiate(
    //                     prefab, 
    //                     roomConstants.SHOPKEEP_POSITION,
    //                     Quaternion.identity);
    //                 if (!encounter.isCompleted) {
    //                     newShop.GetComponent<EncounterInScene>().encounterId = encounter.id;
    //                 }
    //             break;
    //         }
    //     }
    // }
}
