using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PersistentProgressSO",
    menuName = "ScriptableObjects/Persistent Progress SO")]
public class PersistentProgressSO : ScriptableObject {
    public List<CardType> unlockedCards;
    public List<CardType> cardsUnlockedThisRun;
}
