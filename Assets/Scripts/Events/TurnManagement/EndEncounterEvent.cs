using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EncounterOutcome {
    Victory,
    Defeat
    // Could end up putting something here like the
    // Slay the Spire "Mugged" outcome where you 
    // don't get any combat rewards
}
[System.Serializable]
public class EndEncounterEventInfo {
    public EncounterOutcome outcome;

    public EndEncounterEventInfo(EncounterOutcome outcome) {
        this.outcome = outcome;
    }
}
[CreateAssetMenu(
    fileName = "EndEncounterEvent", 
    menuName = "Events/Encounter Management/End Encounter Event")]
public class EndEncounterEvent : BaseGameEvent<EndEncounterEventInfo> { }
