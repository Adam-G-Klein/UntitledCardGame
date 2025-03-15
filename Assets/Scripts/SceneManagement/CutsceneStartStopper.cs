using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneStartStopper : GenericSingleton<CutsceneStartStopper> 
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
    }

    void FixedUpdate() {
        if (Input.GetMouseButtonDown(0) 
            || Input.GetKeyDown(KeyCode.Space) 
            || Input.GetKeyDown(KeyCode.Return)
            ) {
            if (isStopped) {
                playableDirector.Play();
            }
        }
    }

    public void StopTimeline() {
        playableDirector.Pause();
        isStopped = true;
    }
}
