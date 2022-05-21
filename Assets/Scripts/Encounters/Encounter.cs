using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Encounter :  GameScene 
{

    /*
        This function should probably return an EncounterInRoom interface in the future
        so we can make Encounters look different based on their contents 
    */
    Vector2 getLocationInRoom();
    void setLocationInRoom(Vector2 loc);
    EncounterType getEncounterType();
    List<Enemy> getEnemies();
}
