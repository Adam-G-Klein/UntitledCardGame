using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject optionsUIPrefab;

    public void startButtonHandler() {
        SceneManager.LoadScene("GenerateMap");
    }

    public void optionsButtonHandler() {
        Instantiate(optionsUIPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    }

    public void exitButtonHandler() {
        Application.Quit();
    }
}
