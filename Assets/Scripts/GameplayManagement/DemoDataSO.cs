using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "DemoDataSO",
    menuName = "ScriptableObjects/Demo Data SO")]
public class DemoDataSO : ScriptableObject {
    public Dictionary<DemoStepName, bool> stepCompletion;
}