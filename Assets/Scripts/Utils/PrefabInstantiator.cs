using System.Collections.Generic;
using UnityEngine;

public static class PrefabInstantiator {

    //We're probably going to end up doing a lot of this
    //I'm willing to bet something fancy with generic types involved
    //will be a move. Not gonna do that until we write our second function like this though
    public static PlayableCard instantiateCard(GameObject cardPrefab, Transform parent, Card card, CombatEntityInEncounterStats fromStats, InCombatDeck fromDeck){
        GameObject newCard = GameObject.Instantiate(cardPrefab, parent);
        CardDisplay cardDisplay = newCard.GetComponent<CardDisplay>();
        PlayableCard cardPlayable = newCard.GetComponent<PlayableCard>();
        cardDisplay.cardInfo = card;
        cardPlayable.setCardInfo(card);
        cardPlayable.setCompanionFrom(fromStats);
        cardPlayable.setDeckFrom(fromDeck);
        return cardPlayable;
    }

    public static CompanionInstance instantiateCompanion(GameObject companionPrefab, Companion companion, Vector2 position){
        CombatEntityWithDeckInstance deckedInstance = instantiateCombatEntityWithDeck(companionPrefab, companion, position);
        CompanionInstance companionInstance = deckedInstance.GetComponent<CompanionInstance>();
        companionInstance.companion = companion;
        return companionInstance;
    }

    public static EnemyInstance instantiateEnemy(GameObject enemyPrefab, Enemy enemy, Vector2 position){
        CombatEntityInstance combatEntityInstance = instantiateCombatEntity(enemyPrefab, enemy, position);
        EnemyInstance enemyInstance = combatEntityInstance.GetComponent<EnemyInstance>();
        enemyInstance.enemy = enemy;
        return enemyInstance;
    }

    public static MinionInstance instantiateMinion(GameObject minionPrefab, Minion minion, Vector2 position){
        GameObject newMinion = GameObject.Instantiate(minionPrefab, position, Quaternion.identity);
        MinionInstance minionInstance = newMinion.GetComponent<MinionInstance>();
        minionInstance.minion = minion;
        return minionInstance;
    }

    private static CombatEntityWithDeckInstance instantiateCombatEntityWithDeck(GameObject combatEntityPrefab, CombatEntityWithDeck baseStats, Vector2 position){
        CombatEntityWithDeckInstance deckedInstance = instantiateCombatEntity(combatEntityPrefab, baseStats, position).GetComponent<CombatEntityWithDeckInstance>();
        deckedInstance.deckEntity = baseStats;
        return deckedInstance;
    }

    private static CombatEntityInstance instantiateCombatEntity(GameObject combatEntityPrefab, CombatEntityBaseStats baseStats, Vector2 position){
        GameObject newCombatEntity = GameObject.Instantiate(combatEntityPrefab, position, Quaternion.identity);
        CombatEntityInstance combatEntityInstance = newCombatEntity.GetComponent<CombatEntityInstance>();
        combatEntityInstance.baseStats = baseStats;
        combatEntityInstance.id = baseStats.getId();
        return combatEntityInstance;
    }

}