using System.Collections.Generic;
using UnityEngine;

public static class PrefabInstantiator {
    public static PlayableCard InstantiateCard(
        GameObject cardPrefab,
        Transform parent,
        Card card,
        DeckInstance deckFrom)
    {
        GameObject newCard = GameObject.Instantiate(cardPrefab, parent);
        CardDisplay cardDisplay = newCard.GetComponent<CardDisplay>();
        PlayableCard cardPlayable = newCard.GetComponent<PlayableCard>();
        // information flowing the wrong direction, deck should have it set
        CompanionInstance companionInstance = deckFrom.gameObject.GetComponent<CompanionInstance>();
        card.setCompanionFrom(companionInstance.companion.companionType);
        if (companionInstance) {
            cardDisplay.Initialize(card);
        } else {
            cardDisplay.Initialize(card);
        }
        cardPlayable.SetCardInfo(card);
        cardPlayable.SetDeckFrom(deckFrom);
        return cardPlayable;
    }

    public static CompanionInstance InstantiateCompanion(
        GameObject companionPrefab,
        Companion companion,
        Vector2 position,
        Transform parent)
    {
        GameObject companionGO = GameObject.Instantiate(
            companionPrefab,
            position,
            Quaternion.identity,
            parent);
        companionGO.name = companion.companionType.name;
        CompanionInstance companionInstance = companionGO.GetComponent<CompanionInstance>();
        companionInstance.companion = companion;
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

    public static EnemyInstance instantiateEnemy(GameObject enemyPrefab, Enemy enemy, Vector2 position, Transform parent){
        GameObject enemyGO = GameObject.Instantiate(
            enemyPrefab,
            position,
            Quaternion.identity,
            parent);
        EnemyInstance enemyInstance = enemyGO.GetComponent<EnemyInstance>();
        enemyInstance.enemy = enemy;
        return enemyInstance;
    }

    public static TooltipView instantiateTooltipView(GameObject tooltipPrefab, TooltipViewModel tooltip, Vector2 position, Transform parent) {
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

}