using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewGameStateVariable",
    menuName = "Variables/Game State Variable")]
public class GameStateVariableSO : ScriptableObject
{
    public CompanionListVariableSO companions;
    public PlayerDataVariableSO playerData;
    public MapVariableSO map;
    public EncounterVariableSO activeEncounter;
    public EncounterVariableSO nextEncounter;

}
