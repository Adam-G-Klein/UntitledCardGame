using UnityEngine;
// everything responds to mouse rn, this just resets state if you move the mouse
public class MouseControlsManager : GenericSingleton<MouseControlsManager> {

    void Update() {
        if(Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) {
            Cursor.visible = true;
            if(NonMouseInputManager.Instance.currentlyHovered) {
                NonMouseInputManager.Instance.ClearHoverState();
            }
        }
    }
}