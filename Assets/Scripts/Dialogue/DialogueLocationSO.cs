using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(
    fileName = "NewDialogueLocation", 
    menuName = "Dialogue/Dialogue Location")]
public class DialogueLocationSO: ScriptableObject
{
    public List<DialogueSequenceSO> sequencesAtLocation;
}