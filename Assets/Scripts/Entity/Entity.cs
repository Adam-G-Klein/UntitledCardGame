using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 

public abstract class Entity : MonoBehaviour {
    public string id = Id.newGuid();

    public EntityType entityType;
}
