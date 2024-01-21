using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuUIController : MonoBehaviour
{
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        GenerateMap generateMap = GetComponent<GenerateMap>();
        root.Q<Button>("playButton").clicked += () => generateMap.generateMapAndChangeScenes();
    }
}
