using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(
    fileName = "NewDialogueLocationList", 
    menuName = "Dialogue/Dialogue Location List")]
public class DialogueLocationListSO: ScriptableObject
{
    public List<DialogueLocationSO> dialogueLocations;
}