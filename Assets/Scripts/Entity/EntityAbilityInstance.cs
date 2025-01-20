using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
public abstract class EntityAbilityInstance
{
    protected EntityAbility ability;

    private List<TurnPhaseTrigger> turnPhaseTriggers = new List<TurnPhaseTrigger>();
    private List<CombatEntityTrigger> combatEntityTriggers = new List<CombatEntityTrigger>();

    // Provide access to the underlying combat instance for different subclasses.
    protected abstract CombatInstance getCombatInstance();

    public void Setup() {
        getCombatInstance().onDeathHandler += OnDeath;
        registerTrigger();
    }

    private void registerTrigger() {

    }

    private void setupForTurnPhaseTrigger(TurnPhase turnPhase) {
        TurnPhaseTrigger newTrigger = new TurnPhaseTrigger(turnPhase, setupAndInvokeAbility());
        turnPhaseTriggers.Add(newTrigger);
        TurnManager.Instance.addTurnPhaseTrigger(newTrigger);
    }

    protected abstract IEnumerable setupAndInvokeAbility();


    private IEnumerator OnDeath(CombatInstance killer) {
        yield return null;
    }
}

public class CompanionInstanceAbilityInstance : EntityAbilityInstance
{
    private CompanionInstance companionInstance;

    protected override CombatInstance getCombatInstance() { return companionInstance.combatInstance; }

    protected override IEnumerable setupAndInvokeAbility()
    {
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this.companionInstance);
        document.originEntityType = EntityType.CompanionInstance;
        yield return EffectManager.Instance.invokeEffectWorkflowCoroutine(document, ability.effectSteps, null);
    }
}

public class EnemyInstanceAbilitInstance : EntityAbilityInstance
{
    private EnemyInstance enemyInstance;

    protected override CombatInstance getCombatInstance() { return enemyInstance.combatInstance; }

    protected override IEnumerable setupAndInvokeAbility()
    {
        EffectDocument document = new EffectDocument();
        document.map.AddItem(EffectDocument.ORIGIN, this.enemyInstance);
        document.originEntityType = EntityType.Enemy;
        yield return EffectManager.Instance.invokeEffectWorkflowCoroutine(document, ability.effectSteps, null);
    }
}