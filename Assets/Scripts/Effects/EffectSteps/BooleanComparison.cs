using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    This effect step takes two ints, does the boolean comparison on them,
    and outputs true or false given the operation.
*/
public class BooleanComparison : EffectStep, IEffectStepCalculation
{
    [SerializeField]
    private int hardCodedInputKey1 = 0;
    [SerializeField]
    private bool useHardCodedInputKey1 = false;
    [SerializeField]
    private string inputKey1 = "";
    [SerializeField]
    private Operation operation;
    [SerializeField]
    private string inputKey2 = "";
    [SerializeField]
    private string outputKey = "";
    
    public BooleanComparison() {
        effectStepName = "BooleanComparison";
    }

    public override IEnumerator invoke(EffectDocument document) {
        if ((!useHardCodedInputKey1 && !document.intMap.ContainsKey(inputKey1)) || !document.intMap.ContainsKey(inputKey2)) {
            EffectError("Missing either key " + inputKey1 + " or key " + inputKey2 + "in int map!");
            yield return null;
        }
        int num1;
        if (useHardCodedInputKey1) {
            num1 = hardCodedInputKey1;
        } else {
            num1 = document.intMap[inputKey1];
        }
        int num2 = document.intMap[inputKey2];
        bool result = false;
        switch (operation) {
            case Operation.Equal:
                result = num1 == num2;
            break;

            case Operation.LessThan:
                result = num1 < num2;
            break;

            case Operation.LessThanOrEqual:
                result = num1 <= num2;
            break;

            case Operation.GreaterThan:
                result = num1 > num2;
            break;

            case Operation.GreaterThanOrEqual:
                result = num1 >= num2;
            break;
        }
        
        document.boolMap[outputKey] = result;
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }

    public enum Operation {
        Equal,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual
    }
}
