using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CompanionInstance))]
public class CompanionAbilityInvoker : TargetProvider 
{
    public delegate void TurnPhaseEventDelegate(TurnPhaseEventInfo eventInfo);
    private CompanionAbilityContext context;
    private List<CompanionAbility> abilities;
    private TurnManager turnPhaseManager;

    private CompanionInstance companionInstance;

    private PlayerHand playerHand;
    private CompanionManager companionManager;

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

}

