using UnityEngine;

public enum UIState {
    DEFAULT, // Currently everything that's not targetting
    EFFECT_TARGETTING,
    CARD_SELECTION_DISPLAY, 
    COMPANION_SUBMENU, // UNUSED, here so that our enum numbering doesn't mess up in the UI
    END_ENCOUNTER,
    DRAGGING_COMPANION, // Used in the shop when dragging companions to rearrange them
    PURCHASING_CARD,
    SELLING_COMPANION,
    UPGRADING_COMPANION,
    REMOVING_CARD,
    OPTIONS_MENU
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
