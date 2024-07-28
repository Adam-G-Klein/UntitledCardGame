using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIStateEventListener))]
public class EnemyIntentArrowsController : MonoBehaviour
{

    private TargettingArrow arrow;
    [SerializeField]
    private UIColors colors;
    public GameObject arrowPrefab;
    private List<TargettingArrow> arrows = new List<TargettingArrow>();
    private TargettingArrow currentArrow;
    private EnemyInstance enemyInstance;


    void Start(){
        enemyInstance = GetComponentInParent<EnemyInstance>();
    }

    public void updateArrows(EnemyIntent intent) {
        Debug.Log("Updating arrows, intent was: " + intent + " and targets count was: " + intent.targets.Count + " target pos: " + intent.targets[0].transform.position);
        if (intent.targets.Count == 0 || intent.targets[0] == null) {
            return;
        }
        TargettingArrow newArrow = createArrow(intent.targets[0].transform, intent.intentType);
        arrows.Add(newArrow);

    }
    private TargettingArrow createArrow(Transform target, EnemyIntentType intentType){
        TargettingArrow newArrow = Instantiate(arrowPrefab, transform).GetComponent<TargettingArrow>();
        // Todo: set of colors for enemy intent arrows
        setArrowColor(newArrow, new List<EntityType>(){EntityType.Enemy});
        newArrow.transform.position = enemyInstance.transform.position;
        newArrow.setAllChildrenPosition(enemyInstance.transform.position);
        newArrow.freeze(target);
        return newArrow;
    }

    
    private void setArrowColor(TargettingArrow arrow, List<EntityType> validTargets){
        if(validTargets.Contains(EntityType.CompanionInstance) && validTargets.Contains(EntityType.Enemy)){
            arrow.setColor(colors.neutralEffectColor);
        } else if(validTargets.Contains(EntityType.Enemy)){
            arrow.setColor(colors.enemyEffectColor);
        } else if(validTargets.Contains(EntityType.CompanionInstance)){
            arrow.setColor(colors.friendlyEffectColor);
        } else {
            arrow.setColor(colors.neutralEffectColor);
        }
    }

    public void clearArrows(){
        for(int i = 0; i < arrows.Count; i++){
            Destroy(arrows[i].gameObject);
        }
        arrows.Clear();
    }

    public void uiStateChangeEventHandler(UIStateEventInfo info){
        if(info.newState == UIState.END_ENCOUNTER){
            clearArrows();
        }
    }
}