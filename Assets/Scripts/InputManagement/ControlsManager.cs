using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ControlsManager : GenericSingleton<ControlsManager>
{
    private List<IControlsReceiver> controlsReceivers;

    void Awake() {
        controlsReceivers = new List<IControlsReceiver>();
    }

    void Start() {}

    public void RegisterControlsReceiver(IControlsReceiver controlsReceiver) {
        controlsReceivers.Add(controlsReceiver);
    }

    public void handleSelect(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleSelect called");
            ProcessInput(GFGInputAction.SELECT);
        }
    }

    public void handleBack(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleBack called");
            ProcessInput(GFGInputAction.BACK);
        }
    }   

    public void handleCutsceneSkip(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            ProcessInput(GFGInputAction.CUTSCENE_SKIP);
        }
    }

    public void handleNavigateUp(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleNavigateUp called");
            ProcessInput(GFGInputAction.UP);
        }
    }

    public void handleNavigateDown(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleNavigateDown called");
            ProcessInput(GFGInputAction.DOWN);
        }
    }

    public void handleNavigateLeft(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleNavigateLeft called");
            ProcessInput(GFGInputAction.LEFT);
        }
    }

    public void handleNavigateRight(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleNavigateRight called");
            ProcessInput(GFGInputAction.RIGHT);
        }
    }

    private void ProcessInput(GFGInputAction action) {
        foreach (IControlsReceiver receiver in controlsReceivers) {
            receiver.ProcessGFGInputAction(action);
        }
    }
}
