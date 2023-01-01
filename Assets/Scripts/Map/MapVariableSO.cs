using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapReference : Reference<Map, MapVariableSO> {
    public MapReference(Map Value) : base(Value) { }
    public MapReference() { }
}

[CreateAssetMenu(
    fileName = "MapVariable",
    menuName = "Map/Map Variable")]
public class MapVariableSO: VariableSO<Map> { }