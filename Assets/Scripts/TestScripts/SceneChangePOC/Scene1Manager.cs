using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Scene1Manager : Manager 
{
    public static Scene1Manager instance;
    private Scene2Manager scene2Manager;
    private Button sceneChangeButton; 

    private TextMeshPro text1;
    private TextMeshPro text2;
    private string onLoadText = "This text was set by Scene1Manager on loading the scene";
    private string passedText = "This text was passed to Scene2Manager using Scene2Manager.loadScene2()";

    void Awake()
    {
        // If the instance reference has not been set yet, 
        if (instance == null)
        {
            // Set this instance as the instance reference.
            instance = this;
            // no one called loadScene...() on us, so findObjectsInScene won't be called from there
            scene1Init("");
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

    public void loadScene1(string placeholderText){
        loadScene("SceneChangeExample1", placeholderText, scene1Init); 
    }

    public void scene1Init(string placeholderText){
        findObjectsInScene();
        text1.SetText(onLoadText);
        text2.SetText(placeholderText);
        //very interesting finding, the listener needed to be
        // re-added to the button when the scene was loaded
        // even though the Start() method on the button should be running,
        // as we're not preserving it between scenes.
        //Definitely watch out for stuff like this, initialization actions that have to be
        // done when loading a scene asynchronously even though they wouldn't normally be necessary

        //doing it through a lil wait to stop the flickering between scenes 
        Invoke("addButtonListener", 0.5f);
    }

    private void addButtonListener(){
        sceneChangeButton.onClick.AddListener(sceneChangeButtonClicked);
    }

    public void sceneChangeButtonClicked(){
        print("scene change button click");
        scene2Manager.loadScene2(passedText);
    }

    private void findObjectsInScene() {

        GameObject canvas = GameObject.Find("Canvas");
        text1 = canvas.transform.Find("text1").GetComponent<TextMeshPro>();
        text2 = canvas.transform.Find("text2").GetComponent<TextMeshPro>();
        sceneChangeButton = canvas.transform.Find("changeScene").GetComponent<Button>();
    }


}
