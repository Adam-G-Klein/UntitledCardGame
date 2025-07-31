using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EffectStepSO",
    menuName = "EffectStepSO")]
public class EffectStepSO : ScriptableObject {
    [SerializeReference]
    public EffectStep effectStep;
}