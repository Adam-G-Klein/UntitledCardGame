using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The list of events we can currently trigger 
// companion abilities on
// Should definitely include turn phases rather than 
// doing that directly through the turn phase manager, as abilities currently do
public enum AbilityEvent {
    ON_COMBAT_ENTITY_INSTANCE_DEATH
}

[RequireComponent(typeof(CompanionInstance))]
public class CompanionAbilityInvoker : TargetProvider 
{
    private Dictionary<AbilityEvent, List<AbilityEventTrigger>> abilityEventTriggers = new Dictionary<AbilityEvent, List<AbilityEventTrigger>>(){
        {AbilityEvent.ON_COMBAT_ENTITY_INSTANCE_DEATH, new List<AbilityEventTrigger>()}
    };
    private CompanionAbilityContext context;
    private List<CompanionAbility> abilities;
    private TurnManager turnPhaseManager;

    private CompanionInstance companionInstance;

    private PlayerHand playerHand;
    private CompanionManager companionManager;
    
    [SerializeField]
    private TurnPhaseTriggerEvent registerTurnPhaseTriggerEvent;
    [SerializeField]
    private TurnPhaseTriggerEvent removeTurnPhaseTriggerEvent;

    void Start() {
        companionInstance = GetComponent<CompanionInstance>();
        abilities = companionInstance.companion.abilities;
        targetRequestingEntity = companionInstance;
        GameObject turnPhaseManagerGO = GameObject.Find("TurnManager");
        if(turnPhaseManagerGO != null)
            turnPhaseManager = GameObject.Find("TurnManager").GetComponent<TurnManager>();
        else 
            Debug.LogError("CompanionAbilityInvoker: TurnManager not found. Companion abilities triggered on turn phases won't fire");
        GameObject playerHandGO = GameObject.Find("PlayerHand");
        if(playerHandGO != null)
            playerHand = playerHandGO.GetComponent<PlayerHand>();
        else 
            Debug.LogError("CompanionAbilityInvoker: PlayerHand not found. Companion abilities that require a target from the player's hand won't fire");

        GameObject companionManagerGO = GameObject.Find("CompanionManager");
        if(companionManagerGO != null)
            companionManager = companionManagerGO.GetComponent<CompanionManager>();
        else 
            Debug.LogError("CompanionAbilityInvoker: CompanionManager not found. Companion abilities that require information about other companions won't work");
        context = new CompanionAbilityContext(this, turnPhaseManager, playerHand, companionManager, companionInstance);
        setupAbilities(abilities, context);
    }

    // Called by the CompanionInstance's Start() method
    public void setupAbilities(List<CompanionAbility> abilities, CompanionAbilityContext context)
    {
        foreach (CompanionAbility ability in abilities)
        {
            ability.setupAbility(context);
        }
    }

    public void onDeath(){
        foreach (CompanionAbility ability in abilities)
        {
            ability.onDeath(context);
        }
    }

    public void addAbilityEventTrigger(AbilityEventTrigger abilityEventTrigger)
    {
        abilityEventTriggers[abilityEventTrigger.abilityEvent].Add(abilityEventTrigger);
    }

    public void removeAbilityEventTrigger(AbilityEventTrigger abilityEventTrigger)
    {
        abilityEventTriggers[abilityEventTrigger.abilityEvent].Remove(abilityEventTrigger);
    }

    // Can find a way to generalize this later
    public void combatEntityInstanceDeathEventHandler(CombatEntityDeathEventInfo eventInfo) {
        foreach (AbilityEventTrigger abilityEventTrigger in abilityEventTriggers[AbilityEvent.ON_COMBAT_ENTITY_INSTANCE_DEATH])
        {
            if(abilityEventTrigger.triggerResponse != null)
                StartCoroutine(abilityEventTrigger.triggerResponse.GetEnumerator());
        }
    }

    public void registerTurnPhaseTrigger(TurnPhaseTrigger turnPhaseTrigger)
    {
        StartCoroutine(registerTurnPhaseTriggerEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseTriggerEventInfo(turnPhaseTrigger)));
    }

    public void removeTurnPhaseTrigger(TurnPhaseTrigger turnPhaseTrigger)
    {
        StartCoroutine(removeTurnPhaseTriggerEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseTriggerEventInfo(turnPhaseTrigger)));
    }

    // using this as a hack to be able to tell the Purge Cards ability
    // that it can yield to the end of the encounter (After completing the card selection request)
    // if that's not necessary as you're reading this please feel free to 
    // delete it :)
    public void cardEffectHandler(CardEffectEventInfo eventInfo)
    {
        if(eventInfo.cardEffects.ContainsKey(CardEffect.Purge))
        {
            foreach (CompanionAbility ability in abilities)
            {
                if(ability.abilityName == "PurgeCardOnEncounterEnd")
                {
                    // provide a dummy target so the ability can yield to the end of the encounter
                    ability.targetsSupplied(new List<TargettableEntity> {companionInstance});
                }
            }
        }
    }

}

