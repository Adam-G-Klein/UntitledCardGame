using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EffectTargetRequestEventListener))]
[RequireComponent(typeof(UIStateEventListener))]
public class TargettingArrowController : MonoBehaviour
{

    private List<Transform> children;
    private TargettingArrow arrow;
    [SerializeField]
    private UIColors colors;

    void Start(){
        arrow = GetComponentInChildren<TargettingArrow>();
        children = new List<Transform>();
        foreach(Transform child in transform){
            children.Add(child);
        }
        hideArrow();
    }

    public void effectTargetRequestEventHandler(EffectTargetRequestEventInfo info){
        showArrow();
        setArrowColor(info.validTargets);
        foreach(Transform child in transform){
            child.position = info.source.transform.position;
        }
    }

    public void uiStateChangeEventHandler(UIStateEventInfo info){
        if(info.newState != UIState.EFFECT_TARGETTING){
            hideArrow();
        }
    }

    private void showArrow(){
        foreach(Transform child in children){
            child.gameObject.SetActive(true);
        }
    }

    private void setArrowColor(List<EntityType> validTargets){
        if(validTargets.Contains(EntityType.Companion) && validTargets.Contains(EntityType.Enemy)){
            arrow.setColor(colors.neutralEffectColor);
        } else if(validTargets.Contains(EntityType.Enemy)){
            arrow.setColor(colors.enemyEffectColor);
        } else if(validTargets.Contains(EntityType.Companion)){
            arrow.setColor(colors.friendlyEffectColor);
        } else {
            arrow.setColor(colors.neutralEffectColor);
        }
    }

    private void hideArrow(){
        foreach(Transform child in children){
            child.gameObject.SetActive(false);
        }
    }
}