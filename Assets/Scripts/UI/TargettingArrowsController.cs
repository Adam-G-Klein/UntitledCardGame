using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIStateEventListener))]
public class TargettingArrowsController : GenericSingleton<TargettingArrowsController>
{

    [SerializeField]
    private UIColors colors;
    public GameObject arrowPrefab;
    private List<TargettingArrow> arrows = new List<TargettingArrow>();
    private TargettingArrow currentArrow;

    void Start() {
    }

    public void freezeArrow(GameObject target) {
        if(currentArrow != null) {
            currentArrow.freeze(target.transform);
        }
    }

    public void createTargettingArrow(
            List<Targetable.TargetType> validTypes,
            GameObject origin) {
        currentArrow = createArrow(validTypes, origin);
        arrows.Add(currentArrow);
    }

    private TargettingArrow createArrow(
            List<Targetable.TargetType> validTypes,
            GameObject origin) {
                //test
        Debug.Log("TargettingArrows: Creating arrow with origin: " + origin.name);
        Vector3 rootPosition = origin.transform.position;
        TargettingArrow newArrow = Instantiate(arrowPrefab, new Vector3(100,100,100), Quaternion.identity, transform).GetComponent<TargettingArrow>();
        setArrowColor(newArrow, validTypes);
        newArrow.transform.position = rootPosition;
        newArrow.setAllChildrenPosition(rootPosition);
        return newArrow;
    }

    private void setArrowColor(TargettingArrow arrow, List<Targetable.TargetType> validTypes) {
        if(validTypes.Contains(Targetable.TargetType.Enemy)){
            arrow.setColor(colors.enemyEffectColor);
        } else if(validTypes.Contains(Targetable.TargetType.Companion)){
            arrow.setColor(colors.friendlyEffectColor);
        } else {
            arrow.setColor(colors.neutralEffectColor);
        }
    }

    public void uiStateChangeEventHandler(UIStateEventInfo info) {
        // might have to update this if we want targetting arrows
        // to stay during card UI selection
        if(info.newState != UIState.EFFECT_TARGETTING){
            clearArrows();
        }
    }

    private void clearArrows() {
        for(int i = 0; i < arrows.Count; i++){
            Destroy(arrows[i].gameObject);
        }
        arrows.Clear();
    }
}