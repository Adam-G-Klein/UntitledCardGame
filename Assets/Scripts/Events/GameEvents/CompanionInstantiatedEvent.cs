using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CompanionInstantiatedEventInfo {

    public CompanionInstance companionInstance;

    public CompanionInstantiatedEventInfo(CompanionInstance companion) {
        this.companionInstance = companion;
    }
}
[CreateAssetMenu(
    fileName = "CompanionInstantiatedEvent", 
    menuName = "Events/Game Event/Companion Instantiated Event")]
public class CompanionInstantiatedEvent : BaseGameEvent<CompanionInstantiatedEventInfo> { }
