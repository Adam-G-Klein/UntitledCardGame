using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 

[RequireComponent(typeof(EffectTargetSuppliedEventListener))]
public abstract class TargetProvider: MonoBehaviour {

    protected Entity providingEntity;
    protected TargettableEntity requestedTarget;

    [SerializeField]
    private EffectTargetRequestEvent effectTargetRequestEvent;
    protected IEnumerator targettingCoroutine;

    public virtual void effectTargetSuppliedHandler(EffectTargetSuppliedEventInfo eventInfo){
        requestedTarget = eventInfo.target;
    }
    public void requestTarget(List<EntityType> validTargets, TargetRequester requester, List<TargettableEntity> disallowedTargets = null){
        this.targettingCoroutine = getTargetCoroutine(validTargets, requester, disallowedTargets);
        StartCoroutine(targettingCoroutine);
    }
    protected IEnumerator getTargetCoroutine(List<EntityType> validTargets, TargetRequester requester, List<TargettableEntity> disallowedTargets = null) {
        requestedTarget = null;
        StartCoroutine(effectTargetRequestEvent.RaiseAtEndOfFrameCoroutine(
                new EffectTargetRequestEventInfo(validTargets, providingEntity, disallowedTargets)));
        // Waits until the effectTargetSuppliedHandler is called
        yield return new WaitUntil(() => requestedTarget != null);
        requester.targetsSupplied(new List<TargettableEntity>() { requestedTarget });
    }

    public void resetTargettingState() {
        requestedTarget = null;
        if(targettingCoroutine != null)
            StopCoroutine(targettingCoroutine);
    }

}
