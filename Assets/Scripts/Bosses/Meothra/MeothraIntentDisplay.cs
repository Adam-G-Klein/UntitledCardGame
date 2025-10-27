using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MeothraIntentDisplay: MonoBehaviour
{
    [SerializeField] private EnemyInstance enemyInstance;

    public void Setup()
    {
        if (enemyInstance == null) enemyInstance = GetComponent<EnemyInstance>();

        enemyInstance.onIntentDeclared += UpdateIntent;
    }

    private void UpdateIntent() {
        // Do the thing
    }
}
