using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIState {
    DEFAULT, // Currently everything that's not targetting
    EFFECT_TARGETTING,
    // Rest of these are currently unused
    CARD_SET_DISPLAY, // Something like when Slay the spire puts up cards you can choose from a potion
    COMPANION_SUBMENU // When you click on a companion, you get a submenu displayed 

}

[System.Serializable]
public class UIStateEventInfo {
    public UIState newState;

    public UIStateEventInfo (UIState newState) {
        this.newState = newState;
    }
}
[CreateAssetMenu(
    fileName = "UIStateEvent", 
    menuName = "Events/Game Event/UI State Event")]
public class UIStateEvent : BaseGameEvent<UIStateEventInfo> { }