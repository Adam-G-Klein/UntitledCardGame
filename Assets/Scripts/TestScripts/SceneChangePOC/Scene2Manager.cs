using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Scene2Manager : MonoBehaviour
{
    public static Scene2Manager instance;
    private Scene1Manager scene1Manager;

    private TextMeshProUGUI text1;
    private TextMeshProUGUI text2;

    private string onLoadText = "This text was set by Scene2Manager on loading the scene";
    private string passedText = "This text was passed to Scene1Manager using Scene1Manager.loadScene2()";

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
        scene1Manager = GetComponent<Scene1Manager>();

    }

    public void loadScene2(string inText){
        SceneManager.LoadScene("SceneChangeExample2");
        //below function is failing to find, presumably because the scene isn't loaded
        // need to use async load and a coroutine, to be done.
        findObjectsInScene();
        text1.SetText(inText);
        text2.SetText(onLoadText);
    }

    public void sceneChangeButtonClicked(){
        scene1Manager.loadScene1(passedText);
    }

    private void findObjectsInScene() {
        GameObject canvas = GameObject.Find("Canvas");
        text1 = canvas.transform.Find("text1").GetComponent<TextMeshProUGUI>();
        print("text1: " + text1);
        text2 = canvas.transform.Find("text2").GetComponent<TextMeshProUGUI>();
    }
}
