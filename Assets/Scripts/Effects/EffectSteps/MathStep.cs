using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
    The effect that takes in an int value and does some math operation on it

    Input: An int stored in the effect document
    Output: The int after applying the operation with the given scale
    Parameters:
        - Scale: The fixed scale to use in the other side of the operation
        - Operation: The math operation to do
*/
public class MathStep: EffectStep
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private Operation operation;
    [SerializeField]
    private int scale;
    [SerializeField]
    private string outputKey = "";

    public MathStep() {
        effectStepName = "MathStep";
    }

    public override IEnumerator invoke(EffectDocument document)
    {
        if (!document.intMap.ContainsKey(inputKey)) {
            EffectError("No entry for key " + inputKey);
            yield break;
        }

        int inputValue = document.intMap[inputKey];
        int returnValue = inputValue;
        switch (operation) {
            case Operation.Add:
                returnValue = inputValue + scale;
            break;

            case Operation.Subtract:
                returnValue = inputValue - scale;
            break;

            case Operation.Multiply:
                returnValue = inputValue * scale;
            break;

            case Operation.Divide:
                returnValue = inputValue / scale;
            break;
        }

        document.intMap[outputKey] = returnValue;

        yield return null;
    }

    public enum Operation {
        Add,
        Subtract,
        Multiply,
        Divide
    }
}