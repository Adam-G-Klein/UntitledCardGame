
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadSceneArgs {
    //Unity's coroutines only allow one argument, 
    // using a data class like this for that one argument
    // is a common pattern

    //see comment about placeholderText below, it's a placeholder
    public string placeholderText;
    public string sceneName;
    public Action<string> sceneInitCallback;

    public LoadSceneArgs(string sceneName, string placeholderText, Action<string> sceneInitCallback){
        this.placeholderText = placeholderText;
        this.sceneName = sceneName;
        this.sceneInitCallback = sceneInitCallback;
    }

}
public class Manager : MonoBehaviour {
    //the root class for the manager scripts

    public void loadScene(string sceneName, string placeholderText, Action<string> sceneInitCallback){
        //placeholderText takes the place of (symbolizes?) whatever (probably massive) GameInformation 
        // object we end up passing between scenes

        StartCoroutine(loadSceneCorout(new LoadSceneArgs(sceneName, placeholderText, sceneInitCallback)));

    }

    private IEnumerator loadSceneCorout(LoadSceneArgs args){
        //Coroutine waits for Unity to tell us the scene is loaded
        // then does all the init stuff for the scene
        // straight from these unity docs: https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html

        print("load corout started");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(args.sceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        print("load done, calling callback");
        args.sceneInitCallback(args.placeholderText);
        yield return null;

    }

}