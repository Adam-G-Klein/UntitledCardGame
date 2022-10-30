using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CompanionInstantiatedEventInfo {

    public Companion companion;

    public CompanionInstantiatedEventInfo(Companion companion) {
        this.companion = companion;
    }
}
[CreateAssetMenu(
    fileName = "CompanionInstantiatedEvent", 
    menuName = "Events/Game Event/Companion Instantiated Event")]
public class CompanionInstantiatedEvent : BaseGameEvent<CompanionInstantiatedEventInfo> { }
