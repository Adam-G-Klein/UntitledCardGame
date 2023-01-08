using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntReference : Reference<int, IntVariableSO> {
    public IntReference(int Value) : base(Value) { }
    public IntReference() { }
}

[CreateAssetMenu(
    fileName = "IntVariable",
    menuName = "Variables/Int Variable")]
public class IntVariableSO : VariableSO<int> { }
