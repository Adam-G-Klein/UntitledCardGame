using UnityEngine;

public class OptionsMenuListener : MonoBehaviour {

    [SerializeField]
    private GameObject optionsUI;

    void Update() {
        // haha gross but lazy bool evaluation is a thing so bite me I guess
        if(Input.GetKeyDown(KeyCode.Escape) && !GameObject.Find("options-menu")) {
            // instantiate the options menu with the name "options-menu"
            GameObject optionsMenu = GameObject.Instantiate(optionsUI);
            optionsMenu.name = "options-menu";
        }
    }
}