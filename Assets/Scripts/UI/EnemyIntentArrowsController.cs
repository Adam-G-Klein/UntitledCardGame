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
    [Space]
    [Header("Controls for how the targetting arrows behave")]
    [SerializeField]
    private float leftRightScreenPlacementPercent = 1;
    [Header("The amount of bend that's added to arrowBend1 if leftRightScreenPlacementPercent is 1.\nWill be ignored if leftRightScreenPlacementPercent is 0")]
    [SerializeField]
    private Vector3 leftRightArrowBendMod1 = new Vector3(0, 0.1f, 0.1f);
    [Header("The amount of bend that's added to arrowBend2 if leftRightScreenPlacementPercent is 1.\nWill be ignored if leftRightScreenPlacementPercent is 0")]
    [SerializeField]
    private Vector3 leftRightArrowBendMod2 = new Vector3(0, -0.1f, 0.1f);

    [SerializeField]
    private Vector3 arrowRootOffset = Vector3.zero;
    [SerializeField]
    [Header("The 1/3rd point of the arrow will pass through the point that is\nLerp(root, followMouse, 0.33) + arrowBend1\nThe value here changes based on leftRightScreenPlacementPercent")]
    private Vector3 arrowBend1 = Vector3.zero;
    [SerializeField]
    [Header("The 2/3rd point of the arrow will pass through the point that is\nLerp(root, followMouse, 0.66) + arrowBend2\nThe value here changes based on leftRightScreenPlacementPercent")]
    private Vector3 arrowBend2 = Vector3.zero;
    [SerializeField]
    private Vector3 targetPositionOffset = new Vector3(0, -1, 0);

    void Start(){
        enemyInstance = GetComponentInParent<EnemyInstance>();
    }

    public void Setup(float leftRightScreenPlacementPercent)
    {
        this.leftRightScreenPlacementPercent = leftRightScreenPlacementPercent;
        arrowBend1 = arrowBend1 + (leftRightArrowBendMod1 * leftRightScreenPlacementPercent);
        arrowBend2 = arrowBend2 + (leftRightArrowBendMod2 * leftRightScreenPlacementPercent);
    }

    public void updateArrows(EnemyIntent intent)
    {
        if (intent.targets.Count == 0 || intent.targets[0] == null)
        {
            return;
        }
        Debug.Log("Updating arrows, intent was: " + intent + " and targets count was: " + intent.targets.Count + " target pos: " + intent.targets[0].transform.position);
        TargettingArrow newArrow = createArrow(intent.targets[0].transform, intent.intentType);
        arrows.Add(newArrow);

    }
    private TargettingArrow createArrow(Transform target, EnemyIntentType intentType){
        TargettingArrow newArrow = Instantiate(arrowPrefab, transform).GetComponent<TargettingArrow>();
        // Todo: set of colors for enemy intent arrows
        setArrowColor(newArrow, new List<EntityType>(){EntityType.Enemy});
        newArrow.transform.position = enemyInstance.transform.position;
        newArrow.SetArrowBends(arrowBend1, arrowBend2);
        newArrow.setAllChildrenPosition(enemyInstance.transform.position + arrowRootOffset);
        newArrow.freeze(target, targetPositionOffset);
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