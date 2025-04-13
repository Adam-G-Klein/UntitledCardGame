using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EncounterStage {
    None,
    Act1Normal,
    Act2Normal,
    Act3Normal,
    Act1Boss,
    Act2Boss,
    Act3Boss
}

public abstract class MapGeneratorSO: ScriptableObject {
    public abstract Map generateMap();
}