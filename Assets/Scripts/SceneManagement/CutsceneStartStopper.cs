using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneStartStopper : MonoBehaviour
{

    public PlayableDirector playableDirector;

    private bool isStopped = false;

    // Start is called before the first frame update
    void Start()
    {
        if (playableDirector == null) {
            playableDirector = GetComponent<PlayableDirector>();
        }
    }

    void Update() {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
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
