using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(
    fileName = "NewCrossPackCardPool",
    menuName = "Cards/Cross Pack Card Pool")]
public class CrossPackCardPoolSO : ScriptableObject {
    public CardPoolSO crossPackCardPool;
    public PackSO associatedPack1;
    public PackSO associatedPack2;
}
