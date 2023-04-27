using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EffectTargetRequestEventListener))]
[RequireComponent(typeof(EffectTargetSuppliedEventListener))]
[RequireComponent(typeof(UIStateEventListener))]
public class TargettingArrowsController : GenericSingleton<TargettingArrowsController>
{

    [SerializeField]
    private UIColors colors;
    public GameObject arrowPrefab;
    private List<TargettingArrow> arrows = new List<TargettingArrow>();
    private TargettingArrow currentArrow;

    void Start(){
    }

    public void effectTargetRequestEventHandler(EffectTargetRequestEventInfo info){
        if(info.source == null){
            // a null source means that the requestor didn't want an arrow displayed
            return;
        }
        currentArrow = createArrow(info.validTargets, info.source);
        arrows.Add(currentArrow);
    }

    public void effectTargetRequestSuppliedHandler(EffectTargetSuppliedEventInfo info){
        if(currentArrow != null) {
            currentArrow.freeze(info.target.transform);
        }
    }

    public void uiStateChangeEventHandler(UIStateEventInfo info){
        // might have to update this if we want targetting arrows
        // to stay during card UI selection
        if(info.newState != UIState.EFFECT_TARGETTING){
            clearArrows();
        }
    }

    public void createTargettingArrow(
            List<EntityType> validTargets,
            Entity entity) {
        currentArrow = createArrow(validTargets, entity);
        arrows.Add(currentArrow);
    }

    private TargettingArrow createArrow(
            List<EntityType> validTargets,
            Entity entity) {
        TargettingArrow newArrow = Instantiate(arrowPrefab, transform).GetComponent<TargettingArrow>();
        setArrowColor(newArrow, validTargets);
        newArrow.transform.position = entity.transform.position;
        newArrow.setAllChildrenPosition(entity.transform.position);
        return newArrow;
    }

    private void setArrowColor(TargettingArrow arrow, List<EntityType> validTargets){
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

    private void clearArrows(){
        for(int i = 0; i < arrows.Count; i++){
            Destroy(arrows[i].gameObject);
        }
        arrows.Clear();
    }
}