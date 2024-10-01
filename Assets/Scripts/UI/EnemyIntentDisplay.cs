using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(EnemyIntentArrowsController))]
public class EnemyIntentDisplay : MonoBehaviour
{
    // private List<EnemyIntentImage> intentImages = new List<EnemyIntentImage>();
    public List<IntentImage> intentImages = new List<IntentImage>();
    private EnemyIntentArrowsController arrowController;

    private EnemyInstance enemyInstance;
    private TurnManager turnManager;
    private CombatEntityManager combatEntityManager;
    // public so that enemyinstance can remove it on death
    // can figure out a better way to do it later
    public TurnPhaseTrigger displayIntentTrigger;
    public TurnPhaseTriggerEvent registerTurnPhaseTriggerEvent;
    public TurnPhaseTriggerEvent removeTurnPhaseTriggerEvent;
    public TextMeshProUGUI valueText;

    private CombatEntityTrigger onCompanionDeathTrigger;

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
        this.arrowController = GetComponent<EnemyIntentArrowsController>();
        turnManager = TurnManager.Instance;
        combatEntityManager = CombatEntityManager.Instance;
        displayIntentTrigger = new TurnPhaseTrigger(TurnPhase.START_PLAYER_TURN, displayIntent(enemyInstance));
        turnManager.registerTurnPhaseTriggerEventHandler(new TurnPhaseTriggerEventInfo(displayIntentTrigger));
        onCompanionDeathTrigger = new CombatEntityTrigger(CombatEntityTriggerType.COMPANION_DIED, UpdateDisplayAfterCompanionDies());
        combatEntityManager.registerTrigger(onCompanionDeathTrigger);
        enemyInstance.combatInstance.onDeathHandler += OnDeath;
    }

    void OnDestroy() {
        removeTurnPhaseTriggerEvent.Raise(new TurnPhaseTriggerEventInfo(displayIntentTrigger));
    }

    public IEnumerator OnDeath(CombatInstance killer) {
        clearIntent();
        removeTurnPhaseTriggerEvent.Raise(new TurnPhaseTriggerEventInfo(displayIntentTrigger));
        combatEntityManager.unregisterTrigger(onCompanionDeathTrigger);
        yield return null;
    }

    public IEnumerable displayIntent(EnemyInstance enemy)  {
        clearIntent();
        StartCoroutine(displayIntentAfterDelay(enemy));
        yield return null;
    }

    public IEnumerable UpdateDisplayAfterCompanionDies() {
        List<CompanionInstance> targets = enemyInstance.currentIntent.targets;
        List<CompanionInstance> newTargets = new List<CompanionInstance>();
        foreach (CompanionInstance companion in targets) {
            if (combatEntityManager.getCompanions().Contains(companion)) {
                newTargets.Add(companion);
            }
        }
        enemyInstance.currentIntent.targets = newTargets;
        StartCoroutine(displayIntentAfterDelay(enemyInstance));
        yield return null;
    }

    // Todo, needn't be waiting
    private IEnumerator displayIntentAfterDelay(EnemyInstance enemy) {
        yield return new WaitForSeconds(1);
        clearIntent();
        if (enemy.currentIntent == null) {
            StartCoroutine(displayIntentAfterDelay(enemy));
            yield break;
        }
        updateIntentImages(enemy.currentIntent);
        arrowController.updateArrows(enemy.currentIntent);
        yield return null;
    }

    public void clearIntent() {
        //clearIntentImages();
        arrowController.clearArrows();
    }

    private void updateIntentImages(EnemyIntent intent) {
        /*
        foreach (IntentImage image in intentImages) {
            if (image.intent == intent.intentType) {
                image.gameObject.SetActive(true);
            } else {
                image.gameObject.SetActive(false);
            }
        }
    }

    private void clearIntentImages() {
    /*
        for(int i = 0; i < intentImages.Count; i++) {
            intentImages[i].gameObject.SetActive(false);
        }
        */
    }
}

[System.Serializable]
public class IntentImage {
    public EnemyIntentType intent;
    public Image image;
}