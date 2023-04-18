using System.Collections;

public class GainBaseAttackDamageOnDeath : CompanionAbility {

    private AbilityEventTrigger abilityEventTrigger;

    public GainBaseAttackDamageOnDeath ()
    {
        abilityName = "GainBaseStrengthOnDeath";
    }
    
    public override void setupAbility(CompanionAbilityContext context)
    {
    }

    public override IEnumerable invoke(CompanionAbilityContext context) {
        yield return null;
    }

    public override void onDeath(CompanionAbilityContext context)
    {
        context.companionInstance.baseStats.setBaseAttackDamage(context.companionInstance.baseStats.getBaseAttackDamage() + 1);
    }

}