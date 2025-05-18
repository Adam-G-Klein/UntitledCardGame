using System.Collections.Generic;
using UnityEngine;

public class KeyboardControlsManager : GenericSingleton<KeyboardControlsManager> {

    private NonMouseInputManager inputManager;
    private List<IControlsReceiver> controlsReceivers;

    void Awake() {
        controlsReceivers = new List<IControlsReceiver>();
    }

    void Start()
    {
        inputManager = NonMouseInputManager.Instance;        
        controlsReceivers.Add(inputManager);
    }

    public void RegisterControlsReceiver(IControlsReceiver controlsReceiver) {
        controlsReceivers.Add(controlsReceiver);
    }

    void Update() {
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
        if(Input.GetKeyDown(KeyCode.W)) {
            ProcessInput(GFGInputAction.UP);
        } else if(Input.GetKeyDown(KeyCode.S)) {
            ProcessInput(GFGInputAction.DOWN);
        } else if(Input.GetKeyDown(KeyCode.A)) {
            ProcessInput(GFGInputAction.LEFT);
        } else if(Input.GetKeyDown(KeyCode.D)) {
            ProcessInput(GFGInputAction.RIGHT);
        } 
        else if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
            ProcessInput(GFGInputAction.SELECT);
        } else if(Input.GetKeyDown(KeyCode.Backspace) )
        // okay but like hear me out
         {
            ProcessInput(GFGInputAction.BACK);
        } else if(Input.GetKeyDown(KeyCode.Space)) {
            ProcessInput(GFGInputAction.END_TURN);
        } else if(Input.GetKeyDown(KeyCode.Alpha1)) {
            ProcessInput(GFGInputAction.OPEN_COMPANION_1_DRAW);
        } else if(Input.GetKeyDown(KeyCode.Alpha2)) {
            ProcessInput(GFGInputAction.OPEN_COMPANION_2_DRAW);
        } else if(Input.GetKeyDown(KeyCode.Alpha3)) {
            ProcessInput(GFGInputAction.OPEN_COMPANION_3_DRAW);
        } else if(Input.GetKeyDown(KeyCode.Alpha4)) {
            ProcessInput(GFGInputAction.OPEN_COMPANION_4_DRAW);
        } else if(Input.GetKeyDown(KeyCode.Alpha5)) {
            ProcessInput(GFGInputAction.OPEN_COMPANION_5_DRAW);
        }
    }

    private void ProcessInput(GFGInputAction action) {
        foreach (IControlsReceiver receiver in controlsReceivers) {
            receiver.ProcessGFGInputAction(action);
        }
    }
}