using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerDataReference : Reference<PlayerData, PlayerDataVariableSO> {
    public PlayerDataReference(PlayerData Value) : base(Value) { }
    public PlayerDataReference() { }
}

[CreateAssetMenu(
    fileName = "PlayerDataVariable",
    menuName = "Player/Player Data Variable")]
public class PlayerDataVariableSO : VariableSO<PlayerData> { }
