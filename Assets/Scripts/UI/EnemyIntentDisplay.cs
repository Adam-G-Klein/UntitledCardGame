using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIntentDisplay : MonoBehaviour
{
    private List<EnemyIntentImage> intentImages = new List<EnemyIntentImage>();
    private EnemyIntentArrowsController arrowController;

    private EnemyInstance enemyInstance;
    private TurnManager turnManager;
    // public so that enemyinstance can remove it on death
    // can figure out a better way to do it later
    public TurnPhaseTrigger displayIntentTrigger;
    public TurnPhaseTriggerEvent registerTurnPhaseTriggerEvent;

    void Start() {
        enemyInstance = GetComponentInParent<EnemyInstance>();
        arrowController = GetComponent<EnemyIntentArrowsController>();
        intentImages.AddRange(GetComponentsInChildren<EnemyIntentImage>());
        GameObject turnManagerGO = GameObject.Find("TurnManager");
        if(turnManagerGO != null)
            turnManager = turnManagerGO.GetComponent<TurnManager>();
        else Debug.LogError("TurnManager not found in scene, won't be able to update enemy intent display");
        displayIntentTrigger = new TurnPhaseTrigger(TurnPhase.START_PLAYER_TURN, displayIntent(enemyInstance));
        registerTurnPhaseTriggerEvent.Raise(new TurnPhaseTriggerEventInfo(displayIntentTrigger));
        transform.SetAsFirstSibling(); // Want the arrows to be on top of the enemies so that we can see them buffing each other

    }
    
    void Update() {

    }

    public IEnumerable displayIntent(EnemyInstance enemy)  {
        clearIntent();
        updateIntentImages(enemy.currentIntent);
        arrowController.updateArrows(enemy.currentIntent);
        yield return null;
    }

    public void clearIntent() {
        clearIntentImages();
        arrowController.clearArrows();
    }

    private void updateIntentImages(EnemyIntent intent) {
        for(int i = 0; i < intentImages.Count; i++) {
            EnemyIntentImage img = intentImages[i];
            if(img.intentType == intent.intentType) {
                img.gameObject.SetActive(true);
            } else {
                img.gameObject.SetActive(false);
            }
        }
    }

    private void clearIntentImages() {
        for(int i = 0; i < intentImages.Count; i++) {
            intentImages[i].gameObject.SetActive(false);
        }
    }
    
}