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
        cardDisplay.cardInfo = card;
        cardPlayable.SetCardInfo(card);
        cardPlayable.SetDeckFrom(deckFrom);
        return cardPlayable;
    }

    public static CompanionInstance InstantiateCompanion(
        GameObject companionPrefab,
        Companion companion,
        Vector2 position)
    {
        GameObject companionGO = GameObject.Instantiate(
            companionPrefab,
            position,
            Quaternion.identity);
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

    public static EnemyInstance instantiateEnemy(GameObject enemyPrefab, Enemy enemy, Vector2 position){
        GameObject enemyGO = GameObject.Instantiate(
            enemyPrefab,
            position,
            Quaternion.identity);
        EnemyInstance enemyInstance = enemyGO.GetComponent<EnemyInstance>();
        enemyInstance.enemy = enemy;
        return enemyInstance;
    }

    // public static MinionInstance instantiateMinion(GameObject minionPrefab, Minion minion, Vector3 position){
    //     CombatEntityWithDeckInstance deckedInstance = instantiateCombatEntityWithDeck(minionPrefab, minion, position);
    //     MinionInstance minionInstance = deckedInstance.GetComponent<MinionInstance>();
    //     minionInstance.minion = minion;
    //     return minionInstance;
    // }

    // public static void instantiateRandomBackground(GameObject randomBackgroundPrefab, Vector3 position){
    //     GameObject.Instantiate(randomBackgroundPrefab, position, Quaternion.identity);
    // }

    // private static CombatEntityWithDeckInstance instantiateCombatEntityWithDeck(GameObject combatEntityPrefab, CombatEntityWithDeck baseStats, Vector3 position){
    //     CombatEntityWithDeckInstance deckedInstance = instantiateCombatEntity(combatEntityPrefab, baseStats, position).GetComponent<CombatEntityWithDeckInstance>();
    //     deckedInstance.deckEntity = baseStats;
    //     return deckedInstance;
    // }

    // private static CombatEntityInstance instantiateCombatEntity(GameObject combatEntityPrefab, CombatEntityBaseStats baseStats, Vector3 position){
    //     GameObject newCombatEntity = GameObject.Instantiate(combatEntityPrefab, position, Quaternion.identity);
    //     CombatEntityInstance combatEntityInstance = newCombatEntity.GetComponent<CombatEntityInstance>();
    //     combatEntityInstance.baseStats = baseStats;
    //     combatEntityInstance.id = baseStats.getId();
    //     return combatEntityInstance;
    // }

}