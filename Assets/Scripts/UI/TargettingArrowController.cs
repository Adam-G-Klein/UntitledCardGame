using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EffectTargetRequestEventListener))]
[RequireComponent(typeof(UIStateEventListener))]
public class TargettingArrowController : MonoBehaviour
{

    private TargettingArrow arrow;
    [SerializeField]
    private UIColors colors;
    public GameObject arrowPrefab;
    private List<TargettingArrow> arrows = new List<TargettingArrow>();
    private TargettingArrow currentArrow;

    void Start(){
    }

    public void effectTargetRequestEventHandler(EffectTargetRequestEventInfo info){
        if(currentArrow != null) {
            currentArrow.frozen = true;
        }
        currentArrow = createArrow(info);
        arrows.Add(currentArrow);
    }

    public void uiStateChangeEventHandler(UIStateEventInfo info){
        if(info.newState != UIState.EFFECT_TARGETTING){
            clearArrows();
        }
    }

    private TargettingArrow createArrow(EffectTargetRequestEventInfo info){
        TargettingArrow newArrow = Instantiate(arrowPrefab, transform).GetComponent<TargettingArrow>();
        setArrowColor(newArrow, info.validTargets);
        newArrow.transform.position = info.source.transform.position;
        newArrow.setAllChildrenPosition(info.source.transform.position);
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