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
    private TargettableEntityOutlineControl outlineControl = null;

    public virtual bool isTargetableByChildImpl(EffectTargetRequestEventInfo eventInfo) { return true; }
    public virtual void onPointerClickChildImpl(PointerEventData eventData) {}
    public virtual void uiStageChangeEventHandlerChildImpl(UIStateEventInfo eventInfo) {}

    protected virtual void Start() {
        outlineControl = GetComponentInChildren<TargettableEntityOutlineControl>();
    }

    public bool isTargetableBy(EffectTargetRequestEventInfo eventInfo){
        return eventInfo.validTargets.Contains(entityType) 
            && (eventInfo.disallowedTargets == null || !eventInfo.disallowedTargets.Contains(this))
            && isTargetableByChildImpl(eventInfo) ;
    }

    public void effectTargetRequestEventHandler(EffectTargetRequestEventInfo info){
        if(isTargetableBy(info)){
            setTargettable(true);
        }
    }

    public void uIStateEventListener(UIStateEventInfo info){
        // For now we don't need any information besides the fact 
        // that we're not targeting anymore
        uiStageChangeEventHandlerChildImpl(info);
        // Tried to boolean optimize this logic, but it reduced the readability. not worth it
        if(info.newState != UIState.EFFECT_TARGETTING) {
            setTargettable(false);
        }
        else if(entityType == EntityType.UICard && info.newState == UIState.CARD_SELECTION_DISPLAY) {
            setTargettable(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData){
        if (UIStateManager.Instance.currentState == UIState.EFFECT_TARGETTING) {
            TargettingManager.Instance.attemptToTarget(this);
        }
        onPointerClickChildImpl(eventData);
        if(isTargetable){
            print("Entity" + id + " was clicked and is targetable");
            // can assume we have an effect set if we're targetable
            StartCoroutine(effectTargetSuppliedEvent.RaiseAtEndOfFrameCoroutine(new EffectTargetSuppliedEventInfo(this)));
            isTargetable = false;
        } 
    }

    protected void setTargettable(bool val){
        isTargetable = val;
        if(val && outlineControl != null) {
            outlineControl.setOutlineState(OutlineState.Targettable);
        } else if(outlineControl != null) {
            outlineControl.setOutlineState(OutlineState.Idle);
        }
    }

    

}
