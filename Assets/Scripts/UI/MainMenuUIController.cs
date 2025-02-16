using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuUIController : MonoBehaviour
{

    [Header("Real delay is found in mainMenuDisplayDelay.cs")]
    public float delay = 1.5f;
    public bool clicked = false;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        GenerateMap generateMap = GetComponent<GenerateMap>();
        root.Q<Button>("playButton").clicked += () => generateMap.generateMapAndChangeScenes();
        IEnumerator entrance = DramaticEntrance(root);
        StartCoroutine(entrance);
    }

    void Update() {
        if(Input.GetMouseButtonDown(0)) {
            clicked = true;
        }
    }

    private IEnumerator DramaticEntrance(VisualElement root) {
        float time = Time.realtimeSinceStartup;
        yield return new WaitUntil(() => clicked || Time.realtimeSinceStartup - time > delay);
        root.Q("background").AddToClassList("light");
    }

}
