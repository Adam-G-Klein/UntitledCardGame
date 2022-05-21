using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Encounter :  GameScene 
{

    void setLocationInRoom(Vector2 loc);
    EncounterType getEncounterType();
    List<Enemy> getEnemies();
}
