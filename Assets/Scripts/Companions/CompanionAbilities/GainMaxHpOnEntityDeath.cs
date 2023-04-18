using System.Collections;

public class GainMaxHpOnEntityDeath : CompanionAbility {

    private AbilityEventTrigger abilityEventTrigger;

    public GainMaxHpOnEntityDeath ()
    {
        abilityName = "GainMaxHpOnEntityDeath";
    }
    
    public override void setupAbility(CompanionAbilityContext context)
    {
        abilityEventTrigger = new AbilityEventTrigger(AbilityEvent.ON_COMBAT_ENTITY_INSTANCE_DEATH, invoke(context));
        context.invoker.addAbilityEventTrigger(abilityEventTrigger);
    }

    public override IEnumerable invoke(CompanionAbilityContext context) {
        context.companionInstance.baseStats.setMaxHealth(context.companionInstance.baseStats.getMaxHealth() + 1);
        yield return null;
    }

    public override void onDeath(CompanionAbilityContext context)
    {
        context.invoker.removeAbilityEventTrigger(abilityEventTrigger);
    }

}