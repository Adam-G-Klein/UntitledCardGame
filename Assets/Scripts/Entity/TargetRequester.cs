using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 

public abstract class TargetRequester {
    protected List<TargettableEntity> currentTargets = new List<TargettableEntity>();
    protected List<Card> currentSelectedCardTargets = new List<Card>();
    protected List<Card> currentUnselectedCardTargets = new List<Card>();
    public virtual void resetTargets() {
        currentTargets.Clear();
        currentSelectedCardTargets.Clear();
        currentUnselectedCardTargets.Clear();
    }
    public virtual void effectTargetsSupplied(List<TargettableEntity> targets)
    {
        Debug.Log("effectTargetsSupplied to " + this);
        this.currentTargets = targets;
    }

    public virtual void cardTargetsSupplied(List<Card> selected, List<Card> unselected)
    {
        Debug.Log("cardTargetsSupplied to " + this);
        this.currentSelectedCardTargets = selected;
        this.currentUnselectedCardTargets = unselected;
    }
}
