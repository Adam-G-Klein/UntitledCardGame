using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyIntentDisplay : MonoBehaviour
{
    // private List<EnemyIntentImage> intentImages = new List<EnemyIntentImage>();
    public List<IntentImage> intentImages = new List<IntentImage>();
    private EnemyIntentArrowsController arrowController;

    private EnemyInstance enemyInstance;
    private TurnManager turnManager;
    // public so that enemyinstance can remove it on death
    // can figure out a better way to do it later
    public TurnPhaseTrigger displayIntentTrigger;
    public TurnPhaseTriggerEvent registerTurnPhaseTriggerEvent;
    public TurnPhaseTriggerEvent removeTurnPhaseTriggerEvent;
    public TextMeshProUGUI valueText;

    void Start() {
        enemyInstance = GetComponentInParent<EnemyInstance>();
        arrowController = GetComponent<EnemyIntentArrowsController>();
        // intentImages.AddRange(GetComponentsInChildren<EnemyIntentImage>());
        turnManager = TurnManager.Instance;
        displayIntentTrigger = new TurnPhaseTrigger(TurnPhase.START_PLAYER_TURN, displayIntent(enemyInstance));
        registerTurnPhaseTriggerEvent.Raise(new TurnPhaseTriggerEventInfo(displayIntentTrigger));
        transform.SetAsFirstSibling(); // Want the arrows to be on top of the enemies so that we can see them buffing each other

    }

    void OnDestroy() {
        removeTurnPhaseTriggerEvent.Raise(new TurnPhaseTriggerEventInfo(displayIntentTrigger));
    }

    public IEnumerable displayIntent(EnemyInstance enemy)  {
        clearIntent();
        if(enemy.currentIntent == null) {
            yield break;
        }
        updateIntentImages(enemy.currentIntent);
        arrowController.updateArrows(enemy.currentIntent);
        valueText.gameObject.SetActive(true);
        valueText.text = enemy.currentIntent.displayValue.ToString();
        yield return null;
    }

    public void clearIntent() {
        clearIntentImages();
        arrowController.clearArrows();
        valueText.gameObject.SetActive(false);
    }

    private void updateIntentImages(EnemyIntent intent) {
        foreach (IntentImage image in intentImages) {
            if (image.intent == intent.intentType) {
                image.gameObject.SetActive(true);
            } else {
                image.gameObject.SetActive(false);
            }
        }
    }

    private void clearIntentImages() {
        for(int i = 0; i < intentImages.Count; i++) {
            intentImages[i].gameObject.SetActive(false);
        }
    }
}

[System.Serializable]
public class IntentImage {
    public EnemyIntentType intent;
    public GameObject gameObject;
}