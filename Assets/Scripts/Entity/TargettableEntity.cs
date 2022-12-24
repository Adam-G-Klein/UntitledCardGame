using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 

[RequireComponent(typeof(EffectTargetRequestEventListener))]
[RequireComponent(typeof(UIStateEventListener))]
public class TargettableEntity : Entity,
    IPointerClickHandler {

    public bool isTargetable = false;

    [SerializeField]
    private EffectTargetSuppliedEvent effectTargetSuppliedEvent;

    void Start() {
    }

    public void effectTargetRequestEventHandler(EffectTargetRequestEventInfo info){
        if(info.validTargets.Contains(entityType)){
            print("Entity" + id + " is a valid target");
            isTargetable = true;
        }
    }

    public void uIStateEventListener(UIStateEventInfo info){
        // For now we don't need any information besides the fact 
        // that we're not targeting anymore
        if(info.newState != UIState.EFFECT_TARGETTING) {
            isTargetable = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData){
        if(isTargetable){
            print("Entity" + id + " was clicked and is targetable");
            // can assume we have an effect set if we're targetable
            StartCoroutine(effectTargetSuppliedEvent.RaiseAtEndOfFrameCoroutine(new EffectTargetSuppliedEventInfo(this)));
            isTargetable = false;
        }
    }

}
