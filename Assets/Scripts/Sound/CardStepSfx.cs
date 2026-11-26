using System.Collections.Generic;
using System.Linq;

/*
    Sounds for card effects that otherwise play nothing: drawing, gaining energy, creating or upgrading cards,
    scrying, removing a card, playing a Power.

    Rules (so cards that already make noise don't get louder):
      - Only for card effects (a played card, or its on-discard/on-exhaust/on-draw/in-hand workflows). Power
        triggers and rat abilities are left alone.
      - Only when the workflow the step belongs to makes no sound of its own (no status, heal, HP change or
        attack sound; see EffectStep.PlaysSoundFor).
      - At most one step sound per workflow run: the first step that asks wins.

    To give a specific card a different sound (or silence it), add it to CardOverrides.
*/
public static class CardStepSfx
{
    public enum Kind { Power, Draw, Energy, CreateCard, Upgrade, Scry, CardInHand, Removal, StatUp }

    private const string CardHandle = "event:/SFX/SFX_CardHandle";
    private const string UpgradeThud = "event:/SFX/SFX_CompanionUpgradeThud";
    private const string PositiveEffect = "event:/SFX/SFX_PositiveEffect";
    private const string CardRemoval = "event:/SFX/SFX_CardRemoval";

    private static readonly Dictionary<Kind, string> KindSounds = new Dictionary<Kind, string>
    {
        { Kind.Power, UpgradeThud },        // a weighty "this rat changed" thump when the Power is played
        { Kind.Draw, CardHandle },
        { Kind.Energy, PositiveEffect },
        { Kind.CreateCard, CardHandle },
        { Kind.Upgrade, UpgradeThud },
        { Kind.Scry, CardHandle },
        { Kind.CardInHand, CardHandle },
        { Kind.Removal, CardRemoval },
        { Kind.StatUp, PositiveEffect },
    };

    // Card name (without trailing "+") -> sound. An empty string keeps that card silent.
    private static readonly Dictionary<string, string> CardOverrides = new Dictionary<string, string>
    {
    };

    private const string PlayedKey = "__card_step_sfx_played";

    private static readonly Dictionary<EffectWorkflow, bool> workflowHasOwnSound = new Dictionary<EffectWorkflow, bool>();

    public static void Play(EffectStep step, EffectDocument document, Kind kind)
    {
        if (document == null || MusicController.Instance == null) return;
        PlayableCard origin = document.map.TryGetItem<PlayableCard>(EffectDocument.ORIGIN, 0);
        if (origin == null || origin.card == null || origin.card.cardType == null) return;
        if (document.intMap.ContainsKey(PlayedKey)) return;

        CardType cardType = origin.card.cardType;
        if (WorkflowPlaysOwnSound(cardType, step)) return;

        document.intMap[PlayedKey] = 1;
        string key = cardType.Name == null ? "" : cardType.Name.Trim().TrimEnd('+').Trim();
        string sfx = CardOverrides.TryGetValue(key, out string overrideSfx) ? overrideSfx : KindSounds[kind];
        if (!string.IsNullOrEmpty(sfx)) MusicController.Instance.PlaySFX(sfx);
    }

    // Whether the workflow containing this step already plays a sound. Step instances are shared with the
    // CardType asset, so the workflow can be found by reference. Falls back to all of the card's workflows.
    private static bool WorkflowPlaysOwnSound(CardType cardType, EffectStep step)
    {
        List<EffectWorkflow> workflows = AllWorkflows(cardType);
        EffectWorkflow owner = workflows.FirstOrDefault(w => w.effectSteps != null && w.effectSteps.Contains(step));
        if (owner != null) return WorkflowHasSound(owner, cardType);
        return workflows.Any(w => WorkflowHasSound(w, cardType));
    }

    private static bool WorkflowHasSound(EffectWorkflow workflow, CardType cardType)
    {
        if (workflowHasOwnSound.TryGetValue(workflow, out bool cached)) return cached;
        bool hasSound = workflow.effectSteps != null && workflow.effectSteps.Any(s => s != null && s.PlaysSoundFor(cardType));
        workflowHasOwnSound[workflow] = hasSound;
        return hasSound;
    }

    private static List<EffectWorkflow> AllWorkflows(CardType cardType)
    {
        List<EffectWorkflow> workflows = new List<EffectWorkflow>();
        if (cardType.effectWorkflows != null) workflows.AddRange(cardType.effectWorkflows);
        if (cardType.onExhaustEffectWorkflow != null) workflows.Add(cardType.onExhaustEffectWorkflow);
        if (cardType.onDiscardEffectWorkflow != null) workflows.Add(cardType.onDiscardEffectWorkflow);
        if (cardType.inPlayerHandEndOfTurnWorkflow != null) workflows.Add(cardType.inPlayerHandEndOfTurnWorkflow);
        if (cardType.onDrawEffectWorkflow != null) workflows.Add(cardType.onDrawEffectWorkflow);
        workflows.RemoveAll(w => w == null);
        return workflows;
    }
}
