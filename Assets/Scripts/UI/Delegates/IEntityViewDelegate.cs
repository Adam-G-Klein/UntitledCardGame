using System;
using System.Collections.Generic;
using UnityEngine;


public interface IEntityViewDelegate {
    Sprite GetStatusEffectSprite(StatusEffectType statusEffectType);
    Sprite GetEnemyIntentImage(EnemyIntentType enemyIntentType);
    void InstantiateCardView(List<Card> cardList, string promptText);
    MonoBehaviour GetMonoBehaviour();
}