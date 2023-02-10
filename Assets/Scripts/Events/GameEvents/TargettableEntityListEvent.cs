using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


[System.Serializable]
public class TargettableEntityListEventInfo {
    public TargettableEntity target;

    public TargettableEntityListEventInfo(TargettableEntity target){
        this.target = target;
    }
}

[CreateAssetMenu(
    fileName = "TargettableEntityListEvent",
    menuName = "Events/Game Event/TargettableEntityListEvent")]
public class TargettableEntityListEvent : BaseGameEvent<TargettableEntityListEventInfo> { }
