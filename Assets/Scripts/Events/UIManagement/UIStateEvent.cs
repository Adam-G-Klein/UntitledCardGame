using UnityEngine;

public enum UIState {
    DEFAULT, // Currently everything that's not targetting
    EFFECT_TARGETTING,
    // Rest of these are currently unused
    CARD_SELECTION_DISPLAY, // Something like when Slay the spire puts up cards you can choose from a potion
    COMPANION_SUBMENU, // When you click on a companion, you get a submenu displayed 
    END_ENCOUNTER

}

[System.Serializable]
public class UIStateEventInfo {
    public UIState newState;

    public UIStateEventInfo (UIState newState) {
        this.newState = newState;
    }

    public override string ToString()
    {
        return "UIStateEventInfo: " + newState;
    }
}

[CreateAssetMenu(
    fileName = "NewUIStateEvent", 
    menuName = "Events/UI Management/UI State Event")]
public class UIStateEvent : BaseGameEvent<UIStateEventInfo> { }
