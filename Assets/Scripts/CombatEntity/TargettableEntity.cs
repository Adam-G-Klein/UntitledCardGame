using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 

// Setting up for when we want to display this

[RequireComponent(typeof(EffectTargetRequestEventListener))]
[RequireComponent(typeof(UIStateEventListener))]
public class TargettableEntity : MonoBehaviour,
    IPointerClickHandler {

    private CombatEntityInstance entityInstance;
    public bool isTargetable = false;
    private CardEffectData effect;

    [SerializeField]
    private EffectTargetSuppliedEvent effectTargetSuppliedEvent;

    void Start() {
        entityInstance = GetComponent<CombatEntityInstance>();
    }

    public void effectTargetRequestEventHandler(EffectTargetRequestEventInfo info){
        if(info.effect.validTargets.Contains(entityInstance.baseStats.getEntityType())){
            print("Entity" + entityInstance.baseStats.getId() + " is a valid target");
            isTargetable = true;
            effect = info.effect;
        }
    }

    public void uIStateEventListener(UIStateEventInfo info){
        // For now we don't need any information besides the fact 
        // that we're not targeting anymore
        if(info.newState != UIState.EFFECT_TARGETTING) {
            isTargetable = false;
            effect = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData){
        if(isTargetable){
            print("Entity" + entityInstance.baseStats.getId() + " was clicked and is targetable");
            // can assume we have an effect set if we're targetable
            StartCoroutine(effectTargetSuppliedEvent.RaiseAtEndOfFrameCoroutine(new EffectTargetSuppliedEventInfo(effect, entityInstance)));
            isTargetable = false;
        }
    }

}
