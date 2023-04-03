using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCardPool",
    menuName = "Cards/Card Pool")]
public class CardPool: ScriptableObject {
    public List<CardType> cards;
}