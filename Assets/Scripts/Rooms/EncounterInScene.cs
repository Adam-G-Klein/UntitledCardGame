using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EncounterInScene : MonoBehaviour
{
    public string id = Id.newGuid();
    public string encounterId;
    public StringGameEvent encounterInitiateEvent;

    void OnCollisionEnter2D(Collision2D col) {
        encounterInitiateEvent.Raise(encounterId);
    }
}
