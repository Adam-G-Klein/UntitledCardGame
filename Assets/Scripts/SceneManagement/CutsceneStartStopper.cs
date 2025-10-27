using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneStartStopper : GenericSingleton<CutsceneStartStopper>, IControlsReceiver
{

    public PlayableDirector playableDirector;

    // HACK
    [SerializeField]
    private GameStateVariableSO gameState;

    private bool isStopped = false;

    // Start is called before the first frame update
    void Start()
    {
        if (playableDirector == null) {
            playableDirector = GetComponent<PlayableDirector>();
        }
        // hack for when player spams skip right after leaving main menu
        if (gameState.currentLocation != Location.INTRO_CUTSCENE) {
            gameState.LoadCurrentLocationScene();
        }

        ControlsManager.Instance.RegisterControlsReceiver(this);
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            if (isStopped) {
                playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
                isStopped = false;
            }
        }
    }

    public void StopTimeline() {
        playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
        playableDirector.Evaluate();
        isStopped = true;
    }

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        if (action == GFGInputAction.SELECT) {
            if (isStopped) {
                playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
                isStopped = false;
            }
        }
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        return;
    }
}
