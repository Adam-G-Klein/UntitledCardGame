using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultEncounter : Encounter  
{
    

    private List<Enemy> enemies = new List<Enemy>() {
        new RedEnemy(),
        new BlueEnemy(),
        new GreenEnemy()
    };
    private string id = "unsetID!";
    private string encounterSceneString = "Scenes/Encounters/DefaultEncounter";
    private EncounterType encounterType = EncounterType.DefaultEncounter;
    private Vector2 locationInRoom;
    public DefaultEncounter(Vector2 locationInRoom){
        this.locationInRoom = locationInRoom;
    }

    public EncounterType getEncounterType(){
        return encounterType;
    }

    public string getSceneString(){
        return encounterSceneString; 
    }

    public List<Enemy> getEnemies(){
        return enemies;
    }

    public void build(){
        DefaultEncounterFactory encounterFactory = new DefaultEncounterFactory();
        encounterFactory.generateEncounter(this);
    }

    public Vector2 getLocationInRoom(){
        return locationInRoom;
    }
    public void setLocationInRoom(Vector2 loc){
        locationInRoom = loc;
    }
    public string getId(){
        return id;
    }

    public void setId(string id){
        this.id = id;
    }

}
