using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Encounter 
{
    Vector2 getLocationInRoom();
    void setLocationInRoom(Vector2 loc);
    EncounterType getEncounterType();
    string getEncounterSceneString();
    List<Enemy> getEnemies();
    void buildEncounter();
    
}
