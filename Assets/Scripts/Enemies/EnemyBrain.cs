using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyBrain
{
    public List<EnemyBehavior> behaviors;
    [SerializeField]
    public EnemyBehaviorPattern behaviorType;

    public enum EnemyBehaviorPattern {
        // Cycle through the behaviors in sequence.
        SequentialCycling,
        // Choose a behavior at random.
        Random,
        // Terminate on the last behavior and repeat until combat is over.
        SequentialWithSinkAtLastElement,
    }
}