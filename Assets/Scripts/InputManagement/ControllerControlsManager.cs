using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum ControllerAxisState {
    CENTER,
    LEFT,
    RIGHT,
    UP,
    DOWN
}

public class ControllerControlsManager : GenericSingleton<KeyboardControlsManager> {

    private NonMouseInputManager inputManager;

    public ControllerAxisState currentAxisState = ControllerAxisState.CENTER;
    public ControllerAxisState lastAxisStateProcessed = ControllerAxisState.CENTER;
    public GameStateVariableSO gameState;

    public Dictionary<ControllerAxisState, GFGInputAction> axisStateToInputAction = new Dictionary<ControllerAxisState, GFGInputAction> {
        {ControllerAxisState.LEFT, GFGInputAction.LEFT},
        {ControllerAxisState.RIGHT, GFGInputAction.RIGHT},
        {ControllerAxisState.UP, GFGInputAction.UP},
        {ControllerAxisState.DOWN, GFGInputAction.DOWN},
        {ControllerAxisState.CENTER, GFGInputAction.NONE}
    };

    public float stickDeadZone = 0.8f;

    void Start()
    {
        inputManager = NonMouseInputManager.Instance;
    }

    void FixedUpdate() {
        /***
        * Does look like this is the standard way of doing this:
        * https://docs.unity3d.com/Packages/com.unity.inputsystem@1.13/api/UnityEngine.InputSystem.InputSystem.html#UnityEngine_InputSystem_InputSystem_onAnyButtonPress
        * https://docs.unity3d.com/Packages/com.unity.inputsystem@1.13/manual/HowDoI.html 
        * Event subscription exists, but it's not what we want to use idt:
        * https://docs.unity3d.com/Packages/com.unity.inputsystem@1.13/manual/Events.html
        * Subscribing would save us checking every frame, but we'd still have to run all these checks on the struct the singular event we can subscribe to passes us.
        * Maybe if we get CPU-bound it'll matter.
        * Doubt that's what's gonna kill our perf long term though.
        ***/
        /*
        if(Input.GetKeyDown(KeyCode.W)) {
            inputManager.ProcessInput(InputAction.UP);
        } else if(Input.GetKeyDown(KeyCode.S)) {
            inputManager.ProcessInput(InputAction.DOWN);
        } else if(Input.GetKeyDown(KeyCode.A)) {
            inputManager.ProcessInput(InputAction.LEFT);
        } else if(Input.GetKeyDown(KeyCode.D)) {
            inputManager.ProcessInput(InputAction.RIGHT);
        } 
        if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
            inputManager.ProcessInput(InputAction.SELECT);
        } else if(Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.S)) {
            inputManager.ProcessInput(InputAction.BACK);
        } else if(Input.GetKeyDown(KeyCode.Space)) {
            inputManager.ProcessInput(InputAction.END_TURN);
        } else if(Input.GetKeyDown(KeyCode.Alpha1)) {
            inputManager.ProcessInput(InputAction.OPEN_COMPANION_1_DRAW);
        } else if(Input.GetKeyDown(KeyCode.Alpha2)) {
            inputManager.ProcessInput(InputAction.OPEN_COMPANION_2_DRAW);
        } else if(Input.GetKeyDown(KeyCode.Alpha3)) {
            inputManager.ProcessInput(InputAction.OPEN_COMPANION_3_DRAW);
        } else if(Input.GetKeyDown(KeyCode.Alpha4)) {
            inputManager.ProcessInput(InputAction.OPEN_COMPANION_4_DRAW);
        } else if(Input.GetKeyDown(KeyCode.Alpha5)) {
            inputManager.ProcessInput(InputAction.OPEN_COMPANION_5_DRAW);
        }
        */

        Gamepad gamepad = Gamepad.current;
        if(gamepad == null) {
            return;
        }


        Vector2 leftStickValue = gamepad.leftStick.ReadValue();

        if (currentAxisState == ControllerAxisState.CENTER) {
            if (Mathf.Abs(leftStickValue.x) > stickDeadZone) {
                currentAxisState = leftStickValue.x > 0 ? ControllerAxisState.RIGHT : ControllerAxisState.LEFT;
            } else if (Mathf.Abs(leftStickValue.y) > stickDeadZone) {
                currentAxisState = leftStickValue.y > 0 ? ControllerAxisState.UP : ControllerAxisState.DOWN;
            }
        } else if (Mathf.Abs(leftStickValue.x) <= stickDeadZone && Mathf.Abs(leftStickValue.y) <= stickDeadZone) {
            currentAxisState = ControllerAxisState.CENTER;
        }

        if (currentAxisState != lastAxisStateProcessed) {
            Debug.Log($"[ControlsManager] Axis state changed from {lastAxisStateProcessed} to {currentAxisState}");
            inputManager.ProcessInput(axisStateToInputAction[currentAxisState]);
            lastAxisStateProcessed = currentAxisState;
        }
        /*

        if (gamepad.buttonEast.wasPressedThisFrame) {
            Debug.Log("[ControlsManager] Select button pressed");
            inputManager.ProcessInput(InputAction.SELECT);
        }

        if (gamepad.buttonSouth.wasPressedThisFrame) {
            Debug.Log("[ControlsManager] Back button pressed");
            inputManager.ProcessInput(InputAction.BACK);
        }
        */

    }

    public void handleSelect(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleSelect called");
            inputManager.ProcessInput(GFGInputAction.SELECT);
        }
    }

    public void handleBack(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            Debug.Log("[ControlsManager] handleBack called");
            inputManager.ProcessInput(GFGInputAction.BACK);
        }
    }   

    public void handleCutsceneSkip(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            if(SceneManager.GetActiveScene().name == gameState.locationToScene[Location.INTRO_CUTSCENE]) {
                Debug.Log("[ControlsManager] handleCutsceneSkip called");
                inputManager.ProcessInput(GFGInputAction.CUTSCENE_SKIP);
            }
            // I assume these hotkeys should be migrated over to the new input system but it is not clear to me
            if (HotkeyManager.Instance.endTurnHotkeyEnabled) {
                Debug.Log("[ControlsManager] end turn action called");
                HotkeyManager.Instance.EndTurn();
                inputManager.ProcessInput(GFGInputAction.END_TURN);
            }

        }
    }
}