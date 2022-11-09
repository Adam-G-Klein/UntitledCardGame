using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterBuilder : MonoBehaviour {

    public EnemyEncounterSO encounter;

    void Awake() {
        encounter.enemyEncounter.build();

    }


}
