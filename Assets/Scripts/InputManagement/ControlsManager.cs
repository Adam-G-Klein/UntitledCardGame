using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class ControlsManager : GenericSingleton<ControlsManager>
{
    [SerializeField]
    private PersistentControlInfoSO persistentControls;
    [SerializeField]
    private InputSystemUIInputModule inputSystemUIInputModule;
    [SerializeField]
    private PlayerInput playerInput;
    [Space(10)]
    [SerializeField]
    private List<ControlsSpriteMapping> spriteMappings;
    private List<IControlsReceiver> controlsReceivers;
    private List<IIconChange> iconChangers = new List<IIconChange>();
    private ControlMethod controlMethod = ControlMethod.Mouse;
    private ControlScheme controlScheme = ControlScheme.Keyboard;
    private Dictionary<ControlScheme, Dictionary<GFGInputAction, Sprite>> controlsSpriteDict;
    private List<GFGInputAction> actionsThatSwapControlMethod = new List<GFGInputAction> {
        GFGInputAction.UP,
        GFGInputAction.DOWN,
        GFGInputAction.LEFT,
        GFGInputAction.RIGHT,
        GFGInputAction.END_TURN,
        GFGInputAction.SECONDARY_UP,
        GFGInputAction.SECONDARY_DOWN,
        GFGInputAction.SECONDARY_RIGHT,
        GFGInputAction.SECONDARY_LEFT,
        GFGInputAction.VIEW_DECK,
        GFGInputAction.VIEW_DISCARD,
        GFGInputAction.SELL_COMPANION
    };
    private bool controlsInitialized = false;

    void Awake() {
        controlsReceivers = new List<IControlsReceiver>();
        iconChangers = new List<IIconChange>();
        ConvertStupidListToCoolDictionary();
        controlMethod = persistentControls.currentControlMethod;
        controlScheme = persistentControls.currentControlScheme;
        ManuallyUpdateControlScheme(controlScheme);
        controlsInitialized = true;
    }

    void Update() {
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) {
            CheckSwapControlMethod(ControlMethod.Mouse);
        }
    }

    private void ManuallyUpdateControlScheme(ControlScheme controlScheme) {
        // Find the control scheme by name from the InputActionAsset
            var scheme = playerInput.actions.controlSchemes.FirstOrDefault(cs => cs.name == controlScheme.ToString());
            if (scheme != null)
            {
                // Get devices matching this control scheme
                var devices = new List<InputDevice>();
                foreach (var device in InputSystem.devices)
                {
                    if (scheme.SupportsDevice(device))
                        devices.Add(device);
                }

                // Switch to the control scheme with the devices
                playerInput.SwitchCurrentControlScheme(controlScheme.ToString(), devices.ToArray());
            }
    }

    private void ConvertStupidListToCoolDictionary() {
        controlsSpriteDict = new Dictionary<ControlScheme, Dictionary<GFGInputAction, Sprite>>();
        foreach (ControlsSpriteMapping mapping in spriteMappings) {
            if (!controlsSpriteDict.ContainsKey(mapping.controlScheme)) {
                controlsSpriteDict[mapping.controlScheme] = new Dictionary<GFGInputAction, Sprite>();
            }

            foreach (ControlsSpriteMappingItem item in mapping.items) {
                controlsSpriteDict[mapping.controlScheme][item.action] = item.sprite;
            }
        }
    }

    public void RegisterControlsReceiver(IControlsReceiver controlsReceiver) {
        controlsReceivers.Add(controlsReceiver);
    }

    public void UnregisterControlsReceiver(IControlsReceiver controlsReceiver) {
        controlsReceivers.Remove(controlsReceiver);
    }

    public void RegisterIconChanger(IIconChange iconChange) {
        iconChangers.Add(iconChange);
    }

    public void UnregisterIconChanger(IIconChange iconChange) {
        iconChangers.Remove(iconChange);
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
        if (actionsThatSwapControlMethod.Contains(action)) CheckSwapControlMethod(ControlMethod.KeyboardController);
        
        List<IControlsReceiver> immutableControlsReceivers = new List<IControlsReceiver>(controlsReceivers);
        foreach (IControlsReceiver receiver in immutableControlsReceivers) {
            receiver.ProcessGFGInputAction(action);
        }
    }

    public Sprite GetSpriteForGFGAction(GFGInputAction action) {
        try {
            return controlsSpriteDict[controlScheme][action];
        } catch (Exception e) {
            return null;
        }
    }

    public void OnControlsChanged(PlayerInput input) {
        if (!controlsInitialized) return;

        if (Enum.TryParse<ControlScheme>(input.currentControlScheme, out ControlScheme scheme)) {
            this.controlScheme = scheme;
            Debug.Log("ControlsManager: Changed controls scheme to " + controlScheme.ToString());
        } else {
            Debug.LogError("ControlsManager: Unable to parse control scheme " + input.currentControlScheme);
            return;
        }

        persistentControls.currentControlScheme = controlScheme;

        foreach (IIconChange iconChange in iconChangers) {
            GFGInputAction action = iconChange.GetAction();
            iconChange.SetIcon(GetSpriteForGFGAction(action));
        }
    }

    private void CheckSwapControlMethod(ControlMethod newControlMethod) {
        if (controlMethod == newControlMethod) return;

        // Do something about the control method being swapped

        switch (newControlMethod) {
            case ControlMethod.KeyboardController:
                Cursor.visible = false;
                inputSystemUIInputModule.enabled = false;
            break;

            case ControlMethod.Mouse:
                Cursor.visible = true;
                inputSystemUIInputModule.enabled = true;
            break;
        }

        controlMethod = newControlMethod;
        persistentControls.currentControlMethod = controlMethod;
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

    // This needs to be directly mapped to the input actions asset
    // controls schemes
    public enum ControlScheme {
        Mouse,
        Gamepad,
        Switch,
        Xbox,
        Keyboard
    }

    [System.Serializable]
    public class ControlsSpriteMapping {
        public ControlScheme controlScheme;
        public List<ControlsSpriteMappingItem> items;
    }

    [System.Serializable]
    public class ControlsSpriteMappingItem {
        public GFGInputAction action;
        public Sprite sprite;
    }
}
