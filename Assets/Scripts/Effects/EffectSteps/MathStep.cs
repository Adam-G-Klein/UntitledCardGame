using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
    The effect that takes in up to two values and does some math operation on it

    Input: An int stored in the effect document
    Output: The int after applying the operation with the given scale
    Parameters:
        - Scale: The fixed scale to use in the other side of the operation
        - (optional) Operand2: another input to use instead of using a fixed scale
        - Operation: The math operation to do
*/
public class MathStep: EffectStep, IEffectStepCalculation
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private string operand2InputKey = "UNUSED";
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

        bool useOperand2 = document.intMap.ContainsKey(operand2InputKey);
        int operand2 = 0;
        if(useOperand2) operand2 = document.intMap[operand2InputKey];
        int inputValue = document.intMap[inputKey];
        int returnValue = inputValue;
        switch (operation)
        {
            case Operation.Add:
                returnValue = useOperand2 ? inputValue + operand2 : inputValue + scale;
                break;

            case Operation.Subtract:
                returnValue = useOperand2 ? inputValue - operand2 : inputValue - scale;
                break;

            case Operation.Multiply:
                returnValue = useOperand2 ? inputValue * operand2 : inputValue * scale;
                break;

            case Operation.Divide:
                returnValue = useOperand2 ? inputValue / operand2 : inputValue / scale;
                break;
            case Operation.RELU:
                returnValue = inputValue > 0 ? inputValue : 0;
            break;
        }

        document.intMap[outputKey] = returnValue;

        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }

    public enum Operation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        RELU,  // rectified linear unit that zeroes out negative numbers and leaves positive numbers be.
    }
}