using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardEffect : ScriptableObject
{
    //virtual execute function each derived class has to implemented unique behaviors in 
    public abstract void Execute();
}
