using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 

[RequireComponent(typeof(EffectTargetRequestEventListener))]
[RequireComponent(typeof(UIStateEventListener))]
public abstract class TargettableEntity : Entity,
    IPointerClickHandler {


    public bool isTargetable = false;

    [SerializeField]
    private EffectTargetSuppliedEvent effectTargetSuppliedEvent;

    void Start() {
    }

    public virtual bool isTargetableByChildImpl(EffectTargetRequestEventInfo eventInfo) { return true; }
    public virtual void onPointerClickChildImpl(PointerEventData eventData) {}
    public virtual void uiStageChangeEventHandlerChildImpl(UIStateEventInfo eventInfo) {}

    public bool isTargetableBy(EffectTargetRequestEventInfo eventInfo){
        return eventInfo.validTargets.Contains(entityType) 
            && (eventInfo.disallowedTargets == null || !eventInfo.disallowedTargets.Contains(this))
            && isTargetableByChildImpl(eventInfo) ;
    }

    public void effectTargetRequestEventHandler(EffectTargetRequestEventInfo info){
        if(isTargetableBy(info)){
            isTargetable = true;
        }
    }

    public void uIStateEventListener(UIStateEventInfo info){
        // For now we don't need any information besides the fact 
        // that we're not targeting anymore
        uiStageChangeEventHandlerChildImpl(info);
        if(info.newState != UIState.EFFECT_TARGETTING) {
            isTargetable = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData){
        onPointerClickChildImpl(eventData);
        if(isTargetable){
            print("Entity" + id + " was clicked and is targetable");
            // can assume we have an effect set if we're targetable
            StartCoroutine(effectTargetSuppliedEvent.RaiseAtEndOfFrameCoroutine(new EffectTargetSuppliedEventInfo(this)));
            isTargetable = false;
        } 
    }

    

}