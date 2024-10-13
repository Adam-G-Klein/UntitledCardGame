using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(EnemyIntentArrowsController))]
public class EnemyIntentDisplay : MonoBehaviour
{
    // private List<EnemyIntentImage> intentImages = new List<EnemyIntentImage>();
    public List<IntentImage> intentImages = new List<IntentImage>();
    private EnemyIntentArrowsController arrowController;

    private EnemyInstance enemyInstance;
    private TurnManager turnManager;
    private Label intentText;
    private VisualElement pillarBox;
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
        StartCoroutine(SetupUIDocumentElementsWhenReady());
        combatEntityManager = CombatEntityManager.Instance;
        displayIntentTrigger = new TurnPhaseTrigger(TurnPhase.START_PLAYER_TURN, displayIntent(enemyInstance));
        turnManager.registerTurnPhaseTriggerEventHandler(new TurnPhaseTriggerEventInfo(displayIntentTrigger));
        onCompanionDeathTrigger = new CombatEntityTrigger(CombatEntityTriggerType.COMPANION_DIED, UpdateDisplayAfterCompanionDies());
        combatEntityManager.registerTrigger(onCompanionDeathTrigger);
        enemyInstance.combatInstance.onDeathHandler += OnDeath;
    }

    private IEnumerator SetupUIDocumentElementsWhenReady() {
        yield return new WaitUntil(() => UIDocumentGameObjectPlacer.Instance.IsReady());
        VisualElement root = UIDocumentUtils.GetRootElement(enemyInstance.placement.ve);
        pillarBox = root.Q<VisualElement>(className: enemyInstance.placement.ve.name + CombatEncounterView.DETAILS_CONTAINER_SUFFIX);
        if(pillarBox == null) {
            Debug.LogError("No pillar box found on " + enemyInstance.placement.ve.name);
        }
        
        intentText = root.Q<Label>(className: enemyInstance.placement.ve.name + CombatEncounterView.DETAILS_DESCRIPTION_SUFFIX);
        if(intentText == null) {
            Debug.LogError("No intent text found on " + enemyInstance.placement.ve.name);
        }
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
        updateIntentText(enemy.currentIntent);
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
        */
    }

    private void updateIntentText(EnemyIntent intent) {
        intentText.text = intent.intentType.ToString() + "\n" + intent.displayValue.ToString();
        UIStateManager.Instance.SetUIDocDirty();
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
    public UnityEngine.UIElements.Image image;
}