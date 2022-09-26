using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class CardCastArguments {

    //The target ID
    //On entering the scene, all valid targets should be given a targetID
    public int target;
    //Should also include a reference to the EventBus

    public CardCastArguments (int target = -1) { 
        this.target = target;
    }
}

/*
public abstract class CardEffect : ScriptableObject
{
    //virtual execute function each derived class has to implemented unique behaviors in 
    public abstract void Execute(CardEffectArguments args);
}
The effect is not a scriptable object, but a coded effect. 
    It should have a default scale
The CardInfo is a repeatedly instantiatable object with a list of effects
    It should be a scriptableObject so that its state is easily maintained
    between scenes
The CardEffect's target must be fed in by the monobehavior
    This requires arguments to the execute function. 
The monobehavior


*/