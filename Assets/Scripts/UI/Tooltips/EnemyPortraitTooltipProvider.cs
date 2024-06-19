using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;

/*
Just grab the tooltiop from the companionType on the attached CompanionInstance
*/


[RequireComponent(typeof(TooltipOnHover))]
public class EnemyPortraitTooltipProvder : MonoBehaviour
{
    private TooltipOnHover tooltipOnHover;
    [SerializeField]
    private EnemyPortrait portrait;
    public TurnPhaseTriggerEvent registerTurnPhaseTriggerEvent;
    private List<TurnPhaseTrigger> turnPhaseTriggers = new List<TurnPhaseTrigger>();
    void Start() {
        tooltipOnHover = GetComponent<TooltipOnHover>();
        tooltipOnHover.tooltip = new TooltipViewModel();
        if(!portrait) {
            Debug.LogError("No portrait attached to EnemyPortraitTooltipProvider");
            return;
        }
        RegisterTurnPhaseTriggers();
    }
    private void RegisterTurnPhaseTriggers() {
        TurnPhaseTrigger trigger = new TurnPhaseTrigger(TurnPhase.END_PLAYER_TURN, UpdateIntent());
        portrait.enemyInstance.turnPhaseTriggers.Add(trigger); // add here so EnemyInstance can remove it later
        registerTurnPhaseTriggerEvent.Raise(new TurnPhaseTriggerEventInfo(trigger));
    }

    private IEnumerable UpdateIntent() {
        tooltipOnHover.tooltip = GetTooltipFromIntent(portrait.enemyInstance.currentIntent);
        yield return null;
    }

    private TooltipViewModel GetTooltipFromIntent(EnemyIntent intent) {
        // TODO: make this more intelligently grab information from the intent.
        switch(intent.intentType) {
            case EnemyIntentType.BigAttack:
            case EnemyIntentType.SmallAttack:
                // TODO: add name of target? Maybe wait for UI overhaul
                return KeywordTooltipProvider.Instance.GetTooltip(TooltipKeyword.AttackIntent);
            case EnemyIntentType.Debuff:
                return KeywordTooltipProvider.Instance.GetTooltip(TooltipKeyword.DebuffIntent);
            case EnemyIntentType.ChargingUp:
                return KeywordTooltipProvider.Instance.GetTooltip(TooltipKeyword.ChargingIntent);
            default:
                return new TooltipViewModel("Unknown", "This enemy will do something.");
        }
    }

}