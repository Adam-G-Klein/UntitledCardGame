using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;


public class GameStartManager : GenericSingleton<GameStartManager>
{
    public void Start()
    {
        StartCoroutine(WaitForSaveManagerAndStart());
    }

    private IEnumerator WaitForSaveManagerAndStart()
    {
        yield return new WaitUntil(() => SaveManager.Instance != null);
        // Load player progress and settings
        SaveManager.Instance.GameStartHandler();
        // Start music after loading settings to ensure music volume is respected
        StartCoroutine(StartMainMenuMusic());
    }

    private IEnumerator StartMainMenuMusic()
    {
        yield return new WaitUntil(() => MusicController2.Instance != null);
        MusicController2.Instance.PlayMainMenuMusic();
    }
}
