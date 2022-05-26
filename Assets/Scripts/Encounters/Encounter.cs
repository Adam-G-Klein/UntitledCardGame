using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Encounter :  GameScene 
{

    string getId();
    void setId(string id);

    void setLocationInRoom(Vector2 loc);
    EncounterType getEncounterType();
    List<Enemy> getEnemies();
}
