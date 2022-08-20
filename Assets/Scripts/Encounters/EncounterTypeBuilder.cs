using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterTypeBuilder : MonoBehaviour
{
    public Encounter encounter;

    void Start() 
    {
        if (encounter != null)
        {
            encounter.Build();
        }
    }
}
