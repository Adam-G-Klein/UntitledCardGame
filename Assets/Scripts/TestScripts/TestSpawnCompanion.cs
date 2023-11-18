using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawnCompanion : MonoBehaviour
{
    public GameObject prefab;
    public CompanionTypeSO companionType;

    void Start() {
        GameObject companionGO = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        CompanionInstance companionInstance = companionGO.GetComponent<CompanionInstance>();
        companionInstance.companion = new Companion(companionType);
    }
}
