using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 

public abstract class TargetRequester {
    protected List<TargettableEntity> currentTargets;
    public virtual void resetTargets() {
        currentTargets.Clear();
    }
    public virtual void targetsSupplied(List<TargettableEntity> targets)
    {
        this.currentTargets = targets;
    }
}
