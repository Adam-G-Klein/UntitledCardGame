using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EncounterManager : MonoBehaviour
{
    private Encounter activeEncounter = null;

    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update() {}

    public void loadEncounter(Encounter encounter) {
        LoadEncounterArgs args = new LoadEncounterArgs(encounter, encounter.buildEncounter);
        
        activeEncounter = encounter;
        StartCoroutine(loadEncounterCoroutine(args));
    }

    private IEnumerator loadEncounterCoroutine(LoadEncounterArgs args)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(args.encounter.getEncounterSceneString());

        // Wait for the scene to actually fully load
        while(!asyncLoad.isDone)
        {
            yield return null;
        }
        args.callback();
    }
}

class LoadEncounterArgs
{
    public Encounter encounter;
    public Action callback;

    public LoadEncounterArgs(Encounter encounter, Action callback)
    {
        this.encounter = encounter;
        this.callback = callback;
    }
}
