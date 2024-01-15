using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpeakerType {
    MainCharacter, //Aiden
    Concierge,
    Companion
    // Could add Enemies, Maria etc later 
}

[CreateAssetMenu(
    fileName = "NewSpeakerType", 
    menuName = "Dialogue/Speaker Type")]
public class SpeakerTypeSO : ScriptableObject
{
    public SpeakerType speakerType;
    public CompanionTypeSO companionType;
}