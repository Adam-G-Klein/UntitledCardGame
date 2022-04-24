using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Scene1Manager : MonoBehaviour
{
    public static Scene1Manager instance;
    private Scene2Manager scene2Manager;

    private TextMeshProUGUI text1;
    private TextMeshProUGUI text2;
    private string onLoadText = "This text was set by Scene1Manager on loading the scene";
    private string passedText = "This text was passed to Scene2Manager using Scene2Manager.loadScene2()";

    void Awake()
    {
        // If the instance reference has not been set yet, 
        if (instance == null)
        {
            // Set this instance as the instance reference.
            instance = this;
            // no one called loadScene...() on us, so findObjectsInScene won't be called from Start()
            findObjectsInScene();
        }
        else if(instance != this)
        {
            // If the instance reference has already been set, and this is not the
            // the instance reference, destroy this game object.
            Destroy(gameObject);
        }

        // Do not destroy this object when we load a new scene
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        //normally you'd do the "findObjectsInScene()" stuff here
        // but start is called only when the object Scene1Manager is attached to
        // is instantiated. It won't be called when new scenes load because
        // we've set it to Destroy any new manager gameObjects found in new scenes.

        //references to scripts on the same gameObject only need to be found once,
        // the same object is passed around between scenes.
        scene2Manager = GetComponent<Scene2Manager>();
        print("scene2Manager found? " + (scene2Manager != null ? "True" : "False"));

    }

    public void loadScene1(string inText){
        SceneManager.LoadScene("SceneChangeExample1");
        //below function is failing to find, presumably because the scene isn't loaded
        // need to use async load and a coroutine, to be done.
        findObjectsInScene();
        text1.SetText(onLoadText);
        text2.SetText(inText);
    }

    public void sceneChangeButtonClicked(){
        print("onclick called");
        scene2Manager.loadScene2(passedText);
    }

    private void findObjectsInScene() {
        text1 = GameObject.Find("text1").GetComponent<TextMeshProUGUI>();
        text2 = GameObject.Find("text2").GetComponent<TextMeshProUGUI>();
    }
}
