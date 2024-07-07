using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 

public abstract class Entity {
    public string id;
    public EntityType entityType;
}

public enum EntityType {
    CompanionInstance,
    Enemy, 
    Minion,
    Card,
    Unknown,
    Companion
}