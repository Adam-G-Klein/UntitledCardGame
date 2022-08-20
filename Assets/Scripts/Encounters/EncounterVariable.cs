using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EncounterVariable", 
    menuName = "Encounters/Encounter Variable")]
public class EncounterVariable : ScriptableObject
{
    public Encounter encounter;
}
