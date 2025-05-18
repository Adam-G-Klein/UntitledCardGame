using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ControlsManager : GenericSingleton<ControlsManager>
{
    private List<IControlsReceiver> controlsReceivers;
    private ControlMethod controlMethod = ControlMethod.Mouse;

    void Awake() {
        controlsReceivers = new List<IControlsReceiver>();
    }

    void Start() {}

    void Update() {
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) {
            CheckSwapControlMethod(ControlMethod.Mouse);
        }
    }

    public void RegisterControlsReceiver(IControlsReceiver controlsReceiver) {
        controlsReceivers.Add(controlsReceiver);
    }

    public void UnregisterControlsReceiver(IControlsReceiver controlsReceiver) {
        controlsReceivers.Remove(controlsReceiver);
    }


    public void handleSelect(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            Debug.Log("SelectDOWN");
            ProcessInput(GFGInputAction.SELECT_DOWN);
        }
        else if (context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleSelect called");
            ProcessInput(GFGInputAction.SELECT);
        } else if (context.phase == InputActionPhase.Canceled) {
            Debug.Log("SelectUP");
            ProcessInput(GFGInputAction.SELECT_UP);
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

    public void handleEndTurn(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleEndTurn called");
            ProcessInput(GFGInputAction.END_TURN);
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

    public void handleSecondaryNavigateUp(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleSecondaryNavigateUp called");
            ProcessInput(GFGInputAction.SECONDARY_UP);
        }
    }

    public void handleSecondaryNavigateDown(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleSecondaryNavigateDown called");
            ProcessInput(GFGInputAction.SECONDARY_DOWN);
        }
    }

    public void handleSecondaryNavigateLeft(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleSecondaryNavigateLeft called");
            ProcessInput(GFGInputAction.SECONDARY_LEFT);
        }
    }

    public void handleSecondaryNavigateRight(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleSecondaryNavigateRight called");
            ProcessInput(GFGInputAction.SECONDARY_RIGHT);
        }
    }
    public void handleViewDeck(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleViewDeck called");
            ProcessInput(GFGInputAction.VIEW_DECK);
        }
    }

    public void handleViewDiscard(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleViewDiscard called");
            ProcessInput(GFGInputAction.VIEW_DISCARD);
        }
    }

    public void handleSellCompanion(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleSellCompanion called");
            ProcessInput(GFGInputAction.SELL_COMPANION);
        }
    }

    public void handleOptions(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleOptions called");
            ProcessInput(GFGInputAction.OPTIONS);
        }
    }

    private void ProcessInput(GFGInputAction action) {
        CheckSwapControlMethod(ControlMethod.KeyboardController);
        List<IControlsReceiver> immutableControlsReceivers = new List<IControlsReceiver>(controlsReceivers);
        foreach (IControlsReceiver receiver in immutableControlsReceivers) {
            receiver.ProcessGFGInputAction(action);
        }
    }

    private void CheckSwapControlMethod(ControlMethod newControlMethod) {
        if (controlMethod == newControlMethod) return;

        // Do something about the control method being swapped

        switch (newControlMethod) {
            case ControlMethod.KeyboardController:
                Cursor.visible = false;
            break;

            case ControlMethod.Mouse:
                Cursor.visible = true;
            break;
        }

        controlMethod = newControlMethod;
        foreach (IControlsReceiver receiver in controlsReceivers) {
            receiver.SwappedControlMethod(controlMethod);
        }
    }

    public ControlMethod GetControlMethod() {
        return controlMethod;
    }

    public enum ControlMethod {
        Mouse,
        KeyboardController
    }
}
