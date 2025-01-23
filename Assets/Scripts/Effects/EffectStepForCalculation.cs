using System.Collections;
using UnityEngine;

public interface IEffectStepCalculation
{
    IEnumerator invokeForCalculation(EffectDocument document);
}