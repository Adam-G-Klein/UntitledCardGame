using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

/***
I believe this class is FULLY DEPRECATED, see EnemyInstance setting up the arrows controller directly
***/
/*
[RequireComponent(typeof(EnemyIntentArrowsController))]
public class EnemyIntentDisplay : MonoBehaviour
{
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

    public void Setup(EnemyInstance enemyInstance, float leftRightScreenPlacementPercent) {
        Debug.Log("Setting up enemy intent display for " + enemyInstance.name);
        this.enemyInstance = enemyInstance;
        this.arrowController = GetComponent<EnemyIntentArrowsController>();
        // arrowController.Setup(leftRightScreenPlacementPercent);
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
        arrowController.updateArrows(enemy.currentIntent);
        yield return null;
    }

    public void clearIntent() {
        arrowController.clearArrows();
    }
}
*/