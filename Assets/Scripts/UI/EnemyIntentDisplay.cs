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
        // enemyInstance = GetComponentInParent<EnemyInstance>();
        // arrowController = GetComponent<EnemyIntentArrowsController>();
        // turnManager = TurnManager.Instance;
        // displayIntentTrigger = new TurnPhaseTrigger(TurnPhase.START_PLAYER_TURN, displayIntent(enemyInstance));
        // registerTurnPhaseTriggerEvent.Raise(new TurnPhaseTriggerEventInfo(displayIntentTrigger));
        // transform.SetAsFirstSibling(); // Want the arrows to be on top of the enemies so that we can see them buffing each other
    }

    public void Setup(EnemyInstance enemyInstance) {
        Debug.Log("Setting up enemy intent display for " + enemyInstance.name);
        this.enemyInstance = enemyInstance;
        this.arrowController = enemyInstance.GetComponentInChildren<EnemyIntentArrowsController>();
        turnManager = TurnManager.Instance;
        displayIntentTrigger = new TurnPhaseTrigger(TurnPhase.START_PLAYER_TURN, displayIntent(enemyInstance));
        turnManager.registerTurnPhaseTriggerEventHandler(new TurnPhaseTriggerEventInfo(displayIntentTrigger));
        enemyInstance.combatInstance.onDeathHandler += OnDeath;
    }

    void OnDestroy() {
        removeTurnPhaseTriggerEvent.Raise(new TurnPhaseTriggerEventInfo(displayIntentTrigger));
    }

    public IEnumerator OnDeath(CombatInstance killer) {
        clearIntent();
        removeTurnPhaseTriggerEvent.Raise(new TurnPhaseTriggerEventInfo(displayIntentTrigger));
        yield return null;
    }

    public IEnumerable displayIntent(EnemyInstance enemy)  {
        Debug.Log("here im here im here");
        clearIntent();
        if (enemy.currentIntent == null) {
            Debug.Log("Enemy " + enemy.name + " has no current intent");
            StartCoroutine(displayIntentAfterDelay(enemy));
            yield break;
        }
        Debug.Log("Displaying intent for " + enemy.name + " with intent " + enemy.currentIntent.intentType);
        updateIntentImages(enemy.currentIntent);
        arrowController.updateArrows(enemy.currentIntent);
        valueText.gameObject.SetActive(true);
        valueText.text = enemy.currentIntent.displayValue.ToString();
        yield return null;
    }

    private IEnumerator displayIntentAfterDelay(EnemyInstance enemy) {
        yield return new WaitForEndOfFrame();
        clearIntent();
        if (enemy.currentIntent == null) {
            StartCoroutine(displayIntentAfterDelay(enemy));
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