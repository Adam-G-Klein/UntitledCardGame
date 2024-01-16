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
    // TODO: https://trello.com/c/jpOEdZm7/75-speaker-portraits-are-retrieved-from-speakertypeso-currently-blocked-by-need-for-consistently-sized-portrait-assets
    public Sprite portrait;
    [HideInInspector]
    public static SpeakerType[] NonCompanionSpeakers = new SpeakerType[] {
        SpeakerType.MainCharacter,
        SpeakerType.Concierge
    };
}