using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLoadEncounter : MonoBehaviour
{
    [SerializeField] private Encounter encounter;
    [SerializeField] private string sceneName;
    [SerializeField] private EncounterVariable activeEncounter;

    public void LoadEncounter()
    {
        activeEncounter.encounter = this.encounter;
        SceneManager.LoadScene(this.sceneName);
    }
}
