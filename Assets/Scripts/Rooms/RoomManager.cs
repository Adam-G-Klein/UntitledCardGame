using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(StringEventListener))]
[RequireComponent(typeof(StringEventListener))]
public class RoomManager : MonoBehaviour
{
    public EncounterReference activeEncounter;
    public RoomVariableSO activeRoom;
    public MapReference activeMap;

    public RoomConstants roomConstants;

    void Awake()
    {
        Debug.Log(activeRoom);
        Debug.Log(activeRoom.GetValue().id);
        activeRoom.GetValue().build(roomConstants);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

/*
    public void processRoomChangeEvent(string roomId) {
        Debug.Log("Processing room change event in room manager");
        Map currentMap = activeMap.Value;
        List<RoomReference> roomReferences = currentMap.rooms;
        foreach(RoomReference roomReference in roomReferences) {
            if (roomReference.Value.id == roomId) {
                activeRoom.SetValue(roomReference.Value);
                SceneManager.LoadScene("PlaceholderRoom");
                return;
            }
        }
    }
    */

    public void processEncounterInitiateEvent(string encounterId) {
        Debug.Log("Processing encounter initiate event in room manager");
        Room currentRoom = activeRoom.GetValue();
        List<EncounterReference> encounterReferences = currentRoom.encounters;
        foreach (EncounterReference encounterReference in encounterReferences) {
            if (encounterReference.Value.id == encounterId) {
                activeEncounter.Value = encounterReference.Value;
                switch(encounterReference.Value.getEncounterType()) {
                    case EncounterType.Enemy:
                        SceneManager.LoadScene("PlaceholderEnemyEncounter");
                    break;

                    case EncounterType.Shop:
                        SceneManager.LoadScene("PlaceholderShopEncounter");
                    break;
                }
                return;
            }
        }
    }
}
