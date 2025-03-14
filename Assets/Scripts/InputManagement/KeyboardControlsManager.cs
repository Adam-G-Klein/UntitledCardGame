using UnityEngine;

public class KeyboardControlsManager : GenericSingleton<KeyboardControlsManager> {

    private NonMouseInputManager inputManager;

    void Start()
    {
        inputManager = NonMouseInputManager.Instance;
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
            inputManager.ProcessInput(GFGInputAction.UP);
        } else if(Input.GetKeyDown(KeyCode.S)) {
            inputManager.ProcessInput(GFGInputAction.DOWN);
        } else if(Input.GetKeyDown(KeyCode.A)) {
            inputManager.ProcessInput(GFGInputAction.LEFT);
        } else if(Input.GetKeyDown(KeyCode.D)) {
            inputManager.ProcessInput(GFGInputAction.RIGHT);
        } 
        else if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
            inputManager.ProcessInput(GFGInputAction.SELECT);
        } else if(Input.GetKeyDown(KeyCode.Backspace) )
        // okay but like hear me out
         {
            inputManager.ProcessInput(GFGInputAction.BACK);
        } else if(Input.GetKeyDown(KeyCode.Space)) {
            inputManager.ProcessInput(GFGInputAction.END_TURN);
        } else if(Input.GetKeyDown(KeyCode.Alpha1)) {
            inputManager.ProcessInput(GFGInputAction.OPEN_COMPANION_1_DRAW);
        } else if(Input.GetKeyDown(KeyCode.Alpha2)) {
            inputManager.ProcessInput(GFGInputAction.OPEN_COMPANION_2_DRAW);
        } else if(Input.GetKeyDown(KeyCode.Alpha3)) {
            inputManager.ProcessInput(GFGInputAction.OPEN_COMPANION_3_DRAW);
        } else if(Input.GetKeyDown(KeyCode.Alpha4)) {
            inputManager.ProcessInput(GFGInputAction.OPEN_COMPANION_4_DRAW);
        } else if(Input.GetKeyDown(KeyCode.Alpha5)) {
            inputManager.ProcessInput(GFGInputAction.OPEN_COMPANION_5_DRAW);
        }
    }



}