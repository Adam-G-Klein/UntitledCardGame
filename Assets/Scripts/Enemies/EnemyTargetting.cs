using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 

// Setting up for when we want to display this

public class EnemyTargetting : MonoBehaviour,
    IPointerClickHandler {

    private EnemyInstance enemyInstance;
    public bool isTargetable = true;
    private CardEffectData effect;

    [SerializeField]
    private EffectTargetSuppliedEvent effectTargetSuppliedEvent;

    void Start() {
        enemyInstance = GetComponent<EnemyInstance>();
    }

    public void effectTargetRequestEventHandler(EffectTargetRequestEventInfo info){
        if(info.effect.validTargets.Contains(EntityType.Enemy)){
            print("Enemy " + enemyInstance.enemy.id + " is a valid target");
            isTargetable = true;
            effect = info.effect;
        }
    }

    public void OnPointerClick(PointerEventData eventData){
        if(isTargetable){
            print("Enemy " + enemyInstance.enemy.id + " was supplied as a target");
            // can assume we have an effect set if we're targetable
            StartCoroutine(effectTargetSuppliedEvent.RaiseAtEndOfFrameCoroutine(new EffectTargetSuppliedEventInfo(effect, enemyInstance)));
        }
    }

}
