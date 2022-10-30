using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(EnemyInstantiatedEventListener))]
[RequireComponent(typeof(CompanionInstantiatedEventListener))]
public class TestIdProvider : MonoBehaviour
{
    private List<string> companionIds = new List<string>();
    private List<string> enemyIds = new List<string>();
    public string getCompanionId(){
        return "test";
    }

    public string getEnemyId(){
        return "test";
    }
}
