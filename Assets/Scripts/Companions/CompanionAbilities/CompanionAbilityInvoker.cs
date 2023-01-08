using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CompanionInstance))]
[RequireComponent(typeof(EffectTargetSuppliedEventListener))]
public class CompanionAbilityInvoker : MonoBehaviour
{

    public delegate void TurnPhaseEventDelegate(TurnPhaseEventInfo eventInfo);
    public Dictionary<TurnPhase, CompanionAbility> turnPhaseEventAbilities = new Dictionary<TurnPhase, CompanionAbility>();
    private CompanionAbilityContext context;
    private List<CompanionAbility> abilities;
    private TurnManager turnPhaseManager;
    private IEnumerator targettingCoroutine; 
    public UIStateEvent uiStateEvent;
    public EffectTargetRequestEvent effectTargetRequestEvent;
    private CompanionInstance companionInstance;

    private TargettableEntity requestedTarget;

    private PlayerHand playerHand;

    void Start() {
        companionInstance = GetComponent<CompanionInstance>();
        abilities = companionInstance.companion.abilities;
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
        context = new CompanionAbilityContext(this, turnPhaseManager, playerHand);
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

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo eventInfo)
    {
        if(turnPhaseEventAbilities.ContainsKey(eventInfo.newPhase))
        {
            StartCoroutine(turnPhaseEventAbilities[eventInfo.newPhase].invoke(context));
        }
    }

    public void requestTarget(List<EntityType> validTargets, CompanionAbility ability){
        this.targettingCoroutine = getTargetCoroutine(validTargets, ability);
        StartCoroutine(targettingCoroutine);
    }

    public void effectTargetSuppliedHandler(EffectTargetSuppliedEventInfo eventInfo){
        requestedTarget = eventInfo.target;
    }

    private IEnumerator getTargetCoroutine(List<EntityType> validTargets, CompanionAbility ability) {
        requestedTarget = null;
        StartCoroutine(effectTargetRequestEvent.RaiseAtEndOfFrameCoroutine(
                new EffectTargetRequestEventInfo(validTargets, companionInstance)));
        // Waits until the effectTargetSuppliedHandler is called
        yield return new WaitUntil(() => requestedTarget != null);
        ability.targetsSupplied(new List<TargettableEntity>() { requestedTarget });
    }
}

