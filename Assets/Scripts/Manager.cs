using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public void loadScene(GameScene scene) {
        LoadSceneArgs args = new LoadSceneArgs(scene, scene.build);
        StartCoroutine(loadSceneCoroutine(args));
    }

    private IEnumerator loadSceneCoroutine(LoadSceneArgs args)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(args.scene.getSceneString());

        // Wait for the scene to actually fully load
        while(!asyncLoad.isDone)
        {
            yield return null;
        }
        args.callback();
    }   
    public static void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_WEBPLAYER
        Application.OpenURL(webplayerQuitURL);
        #else
        Application.Quit();
        #endif
    }

}

class LoadSceneArgs 
{
    public GameScene scene;
    public Action callback;

    public LoadSceneArgs(GameScene scene, Action callback)
    {
        this.scene = scene;
        this.callback = callback;
    }
}