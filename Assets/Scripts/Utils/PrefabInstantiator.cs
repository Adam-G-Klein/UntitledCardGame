using System.Collections.Generic;
using UnityEngine;

public static class PrefabInstantiator {
    public static PlayableCard InstantiateCard(
        GameObject cardPrefab,
        Transform parent,
        Card card,
        DeckInstance deckFrom,
        Vector3 position)
    {
        GameObject newCard = GameObject.Instantiate(cardPrefab, position, Quaternion.identity, parent);
        PlayableCard cardPlayable = newCard.GetComponent<PlayableCard>();
        // information flowing the wrong direction, deck should have it set
        CompanionInstance companionInstance = deckFrom.gameObject.GetComponent<CompanionInstance>();
        card.setCompanionFrom(companionInstance.companion.companionType);
        cardPlayable.SetCardInfo(card);
        cardPlayable.SetDeckFrom(deckFrom);
        return cardPlayable;
    }

    public static CompanionInstance InstantiateCompanion(
        GameObject companionPrefab,
        Vector2 position,
        Transform parent)
    {
        GameObject companionGO = GameObject.Instantiate(
            companionPrefab,
            position,
            Quaternion.identity,
            parent);
        CompanionInstance companionInstance = companionGO.GetComponent<CompanionInstance>();
        return companionInstance;
    }

    public static MinionInstance InstantiateMinion(
        GameObject minionPrefab,
        Minion minion,
        Vector2 position)
    {
        GameObject minionGO = GameObject.Instantiate(
            minionPrefab,
            position,
            Quaternion.identity);
        MinionInstance minionInstance = minionGO.GetComponent<MinionInstance>();
        minionInstance.minion = minion;
        return minionInstance;
    }

    public static EnemyInstance instantiateEnemy(GameObject enemyPrefab, Vector2 position, Transform parent){
        GameObject enemyGO = GameObject.Instantiate(
            enemyPrefab,
            position,
            Quaternion.identity,
            parent);
        EnemyInstance enemyInstance = enemyGO.GetComponent<EnemyInstance>();
        return enemyInstance;
    }

    public static TooltipView instantiateTooltipView(GameObject tooltipPrefab, TooltipViewModel tooltip, Vector3 position, Transform parent = null) {
        GameObject tooltipGO = GameObject.Instantiate(
            tooltipPrefab,
            position,
            Quaternion.identity,
            parent);

        TooltipView view = tooltipGO.GetComponent<TooltipView>();
        Debug.Log("Tooltip: view after instantiation: " + view + " tooltip empty? " + tooltip.empty);
        view.tooltip = tooltip;
        return view;
    }

    public static FXExperience instantiateFXExperience(GameObject prefab, Vector2 position) {
        GameObject fxExperienceGO = GameObject.Instantiate(prefab, position, Quaternion.identity);
        return fxExperienceGO.GetComponent<FXExperience>();
    }
}